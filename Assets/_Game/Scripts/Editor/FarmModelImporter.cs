#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DevGames.Editor
{
    /// <summary>
    /// Preserves the coordinate conversion authored inside third-party FBX files.
    /// Quaternius models already target Unity; baking their axes again lays props sideways.
    /// </summary>
    public sealed class FarmModelImporter : AssetPostprocessor
    {
        const string ThirdPartyRoot = "Assets/_Game/ThirdParty/";

        /// <summary>
        /// Configures model-axis conversion before Unity imports an FBX. Unity invokes
        /// this callback automatically, so it has no arguments or return value.
        /// </summary>
        void OnPreprocessModel()
        {
            if (!assetPath.StartsWith(ThirdPartyRoot) || !assetPath.EndsWith(".fbx")) return;
            var importer = (ModelImporter)assetImporter;
            importer.bakeAxisConversion = false;
            // Some packs export their Blender camera and lights; placed in the scene, that camera
            // renders over the gameplay one and the view freezes.
            importer.importCameras = false;
            importer.importLights = false;

            // The farmer plays the shared humanoid clips (jump included) through retargeting.
            if (assetPath.EndsWith("/Farmer.fbx"))
            {
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            }

            // Mixamo character and clips: one humanoid rig, so every clip retargets onto the character.
            if (assetPath.Contains("/Mixamo/"))
            {
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                // Keyframe reduction makes planted feet drift; these clips are small, keep every key.
                importer.animationCompression = ModelImporterAnimationCompression.Off;
            }
        }

        /// <summary>
        /// Clip setup runs here, not in OnPreprocessModel: on a file's first import the default clips are
        /// only known at this point, so settings made earlier were silently dropped.
        /// </summary>
        void OnPreprocessAnimation()
        {
            var importer = (ModelImporter)assetImporter;
            if (assetPath.StartsWith(ThirdPartyRoot + "Mixamo/"))
                ConfigureMixamoClips(importer, System.IO.Path.GetFileNameWithoutExtension(assetPath));

            // The hawk's only clip is its wing flap, which must cycle.
            if (assetPath.EndsWith("/Hawk.fbx"))
            {
                var clips = importer.defaultClipAnimations;
                foreach (var clip in clips) clip.loopTime = true;
                importer.clipAnimations = clips;
            }
        }

        /// <summary>
        /// Mixamo clips play in place (the motor moves the player, not the clip). Loops cycle and keep
        /// their bounce; the jump is cut into take-off and landing, skipping the long crouch because the
        /// motor jumps the moment Space is pressed, and leaves height to the physics.
        /// </summary>
        static void ConfigureMixamoClips(ModelImporter importer, string file)
        {
            var source = importer.defaultClipAnimations;
            if (source.Length == 0 || file == "Character") return;
            var take = source[0];

            // Root XZ is NOT baked into the pose: the clip's forward travel is extracted as root motion and,
            // with root motion off, dropped, so the body stays on the player. Height is either baked (the
            // body keeps its bob), baked from the feet (landings: feet stay planted while the body crouches)
            // or extracted (airborne/ledge clips: physics or LedgeClimber owns the height).
            // travel = true bakes the clip's forward motion into the pose too: the ledge pull-up carries the
            // body from the hang onto the top by itself while LedgeClimber holds the player still.
            ModelImporterClipAnimation Cut(string name, float from, float to, bool loop, Height height, bool travel = false) => new()
            {
                name = name,
                takeName = take.takeName,
                firstFrame = Mathf.Lerp(take.firstFrame, take.lastFrame, from),
                lastFrame = Mathf.Lerp(take.firstFrame, take.lastFrame, to),
                loopTime = loop,
                loopPose = loop,
                lockRootRotation = true,
                keepOriginalOrientation = false, // "Body Orientation": the Blender re-export turned the original root 180°
                lockRootPositionXZ = travel,
                lockRootHeightY = height != Height.Physics,
                keepOriginalPositionY = height == Height.Pose,
                heightFromFeet = height == Height.Feet,
            };

            // Cut points measured on the Mixamo "Jumping" curve: feet leave the ground at 26%, apex at 42%,
            // touch-down at 55%, standing again at 81%.
            importer.clipAnimations = file switch
            {
                "Jump" => new[] { Cut("JumpStart", 0.26f, 0.42f, false, Height.Physics), Cut("JumpLand", 0.55f, 0.81f, false, Height.Feet) },
                "HardLanding" => new[] { Cut(file, 0f, 1f, false, Height.Feet) },
                "ClimbUp" => new[] { Cut(file, 0f, 1f, false, Height.Pose, travel: true) },
                _ => new[] { Cut(file, 0f, 1f, true, Height.Pose) },
            };
        }

        /// <summary>
        /// Mixamo textures were pulled out of the character FBX (Unity left them embedded and the model
        /// came out white): normal maps flagged as such, and 4K capped at 2K for a third-person camera.
        /// </summary>
        void OnPreprocessAudio()
        {
            // The 3-minute music track streams from disk instead of being decompressed whole into memory.
            if (!assetPath.StartsWith(ThirdPartyRoot + "ZaneLittle/")) return;
            var importer = (AudioImporter)assetImporter;
            var settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.Streaming;
            importer.defaultSampleSettings = settings;
        }

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ThirdPartyRoot + "Mixamo/")) return;
            var importer = (TextureImporter)assetImporter;
            importer.maxTextureSize = 2048;
            if (assetPath.Contains("_Normal")) importer.textureType = TextureImporterType.NormalMap;
        }

        /// <summary>Bumping this makes Unity reimport every third-party model with the rules above.</summary>
        public override uint GetVersion() => 11;

        enum Height { Pose, Feet, Physics }

        /// <summary>
        /// The styloo Cozy Farm palette washes out under glossy PBR; its author recommends a
        /// rough, matte look, so its imported materials get almost no smoothness.
        /// </summary>
        void OnPostprocessMaterial(Material material)
        {
            if (assetPath.StartsWith(ThirdPartyRoot + "Styloo/")) material.SetFloat("_Smoothness", 0.1f);

            // Mixamo hair is strands painted on cards: cut out by the texture's alpha and seen from both sides.
            if (assetPath.StartsWith(ThirdPartyRoot + "Mixamo/") && material.name.Contains("Hair"))
            {
                material.SetFloat("_AlphaClip", 1f);
                material.SetFloat("_Cutoff", 0.5f);
                material.EnableKeyword("_ALPHATEST_ON");
                material.SetFloat("_Cull", 0f);
            }
            if (assetPath.StartsWith(ThirdPartyRoot + "Mixamo/")) material.SetFloat("_Smoothness", 0.25f);
        }
    }
}
#endif

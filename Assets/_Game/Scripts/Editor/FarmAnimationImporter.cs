#if UNITY_EDITOR
using System.Linq;
using DevGames.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DevGames.Editor
{
    /// <summary>
    /// Keeps the locomotion clips from the third-party animation library configured
    /// as seamless loops whenever Unity imports or reimports the source FBX.
    /// </summary>
    public sealed class FarmAnimationImporter : AssetPostprocessor
    {
        const string AnimationLibraryPath =
            "Assets/_Game/ThirdParty/Quaternius/UniversalAnimations/UAL1_Standard.fbx";
        const string CharacterModelPath =
            "Assets/_Game/ThirdParty/Quaternius/UniversalCharacter/Models/MaleCharacter.fbx";

        /// <summary>
        /// Reimports the locomotion clips as loops and assigns the character's valid
        /// humanoid Avatar to the Animator in the currently open Farm scene.
        /// </summary>
        [MenuItem("DevGames/Fix Character Locomotion")]
        public static void FixCharacterLocomotion()
        {
            var importer = AssetImporter.GetAtPath(AnimationLibraryPath) as ModelImporter;
            if (importer == null)
            {
                Debug.LogError($"Animation library not found at {AnimationLibraryPath}.");
                return;
            }

            ConfigureLoopingClips(importer);
            importer.SaveAndReimport();

            var avatar = AssetDatabase.LoadAllAssetsAtPath(CharacterModelPath)
                .OfType<Avatar>()
                .FirstOrDefault(candidate => candidate.isValid && candidate.isHuman);
            var playerAnimator = Object.FindFirstObjectByType<PlayerAnimator>();
            var animator = playerAnimator != null ? playerAnimator.GetComponentInChildren<Animator>() : null;

            if (avatar == null || animator == null)
            {
                Debug.LogError("Could not find a valid humanoid Avatar or the Farm player Animator.");
                return;
            }

            animator.avatar = avatar;
            EditorUtility.SetDirty(animator);
            EditorSceneManager.MarkSceneDirty(animator.gameObject.scene);
            EditorSceneManager.SaveScene(animator.gameObject.scene);
            Debug.Log("Character locomotion fixed: humanoid Avatar assigned and locomotion clips set to loop.");
        }

        /// <summary>
        /// Applies loop settings before Unity imports the model. This method has no
        /// arguments or return value because Unity invokes it as an import callback.
        /// </summary>
        void OnPreprocessModel()
        {
            if (assetPath != AnimationLibraryPath) return;

            var importer = (ModelImporter)assetImporter;
            ConfigureLoopingClips(importer);
        }

        /// <summary>
        /// Marks idle, walking and sprinting clips as cyclic animations.
        /// </summary>
        /// <param name="importer">Model importer that owns the embedded animation clips.</param>
        static void ConfigureLoopingClips(ModelImporter importer)
        {
            var clips = importer.defaultClipAnimations;
            var changed = false;

            for (var index = 0; index < clips.Length; index++)
            {
                var name = clips[index].name;
                if (!name.Contains("Idle_Loop") &&
                    !name.Contains("Walk_Loop") &&
                    !name.Contains("Sprint_Loop")) continue;

                clips[index].loopTime = true;
                clips[index].loopPose = true;
                changed = true;
            }

            if (changed) importer.clipAnimations = clips;
        }
    }
}
#endif

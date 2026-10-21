#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using DevGames.Core;
using DevGames.Level;
using DevGames.Player;
using DevGames.UI;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DevGames.Editor
{
    public static class FarmProjectBuilder
    {
        const string Root = "Assets/_Game";
        const string MaterialRoot = Root + "/Art/Materials";
        const string LevelRoot = Root + "/Settings/Levels";
        const string SceneRoot = Root + "/Scenes";
        const string FarmBuildings = Root + "/ThirdParty/Quaternius/FarmBuildings/Models/";
        const string Nature = Root + "/ThirdParty/Quaternius/StylizedNature/Models/";
        const string Crops = Root + "/ThirdParty/Quaternius/UltimateCrops/Models/";
        const string Animals = Root + "/ThirdParty/Quaternius/FarmAnimals/Models/";
        const string CharacterModel = Root + "/ThirdParty/Quaternius/UniversalCharacter/Models/MaleCharacter.fbx";
        const string CharacterAnimations = Root + "/ThirdParty/Quaternius/UniversalAnimations/UAL1_Standard.fbx";
        const string TractorModel = Root + "/ThirdParty/Innerscene/Tractor/Tractor.fbx";
        const string BirdModel = Root + "/ThirdParty/Quaternius/Bird/Ernest.fbx";
        const string FarmProps = Root + "/ThirdParty/Quaternius/FarmProps/";

        static readonly Color BarnRed = new(0.48f, 0.10f, 0.07f);
        static readonly Color Wood = new(0.30f, 0.17f, 0.08f);
        static readonly Color Straw = new(0.78f, 0.57f, 0.18f);
        static readonly Color Green = new(0.20f, 0.36f, 0.16f);
        static readonly Color Roof = new(0.12f, 0.15f, 0.17f);
        static readonly Color Cream = new(0.86f, 0.80f, 0.65f);

        [MenuItem("DevGames/Build Farm Project")]
        public static void BuildAll()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("Pare o Play Mode antes de usar DevGames > Build Farm Project.");
                return;
            }

            ConfigureLayers();
            var materials = CreateMaterials();
            var definition = CreateLevelDefinition();
            CreateBootstrapScene();
            CreateMainMenuScene(materials);
            CreateLevelSelectScene(materials);
            CreateFarmScene(materials, definition);
            ConfigureBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("DevGames Fazenda construída com sucesso.");
        }

        /// <summary>Applies the stable visual revision and emits an unmistakable confirmation.</summary>
        [MenuItem("DevGames/APLICAR CENARIO SEM TRILHO V7")]
        public static void ApplyVisualFixV6()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("Pare o Play Mode antes de aplicar o CENARIO SEM TRILHO V7.");
                return;
            }

            BuildAll();
            Debug.Log("CENARIO SEM TRILHO V7 SALVO EM Farm.unity - abra Bootstrap e pressione Play.");
        }

        /// <summary>Disables the scene generator while Play Mode or compilation is active.</summary>
        /// <returns>True only when it is safe for the editor to replace project scenes.</returns>
        [MenuItem("DevGames/Build Farm Project", true)]
        static bool CanBuildAll()
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling;
        }

        static void ConfigureLayers()
        {
            var tagManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0];
            var serialized = new SerializedObject(tagManager);
            var layers = serialized.FindProperty("layers");
            var names = new[] { "Player", "Ground", "Climbable", "MovingPlatform", "CameraBlocker", "Collectible" };
            for (var i = 0; i < names.Length; i++) layers.GetArrayElementAtIndex(8 + i).stringValue = names[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static Dictionary<string, Material> CreateMaterials()
        {
            var result = new Dictionary<string, Material>();
            CreateMaterial("BarnRed", BarnRed, result);
            CreateMaterial("Wood", Wood, result);
            CreateMaterial("Straw", Straw, result);
            CreateMaterial("Green", Green, result);
            CreateMaterial("Roof", Roof, result);
            CreateMaterial("Cream", Cream, result);
            CreateMaterial("Metal", new Color(0.30f, 0.36f, 0.38f), result);
            CreateMaterial("Goal", new Color(1f, 0.55f, 0.12f), result, true);
            return result;
        }

        static void CreateMaterial(string name, Color color, IDictionary<string, Material> materials, bool emission = false)
        {
            var path = $"{MaterialRoot}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.color = color;
            if (emission)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 1.6f);
            }
            materials[name] = material;
        }

        static LevelDefinition CreateLevelDefinition()
        {
            var path = $"{LevelRoot}/Farm.asset";
            var definition = AssetDatabase.LoadAssetAtPath<LevelDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<LevelDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }
            definition.id = 1;
            definition.displayName = "Fazenda";
            definition.sceneName = "Farm";
            definition.nextLevelId = 2;
            definition.referenceTimeSeconds = 300f;
            EditorUtility.SetDirty(definition);
            return definition;
        }

        static void CreateBootstrapScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("Bootstrap").AddComponent<BootstrapLoader>();
            SaveScene(scene, $"{SceneRoot}/Bootstrap.unity");
        }

        static void CreateMainMenuScene(Dictionary<string, Material> materials)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateMenuBackdrop(materials, "FAZENDA AO ENTARDECER");
            var canvas = CreateCanvas("Main Menu Canvas");
            var controller = canvas.gameObject.AddComponent<MenuController>();

            CreateText(canvas.transform, "Title", "O MENINO LOIRO\nE OS DEDOS PERDIDOS", 46, new Vector2(0.5f, 0.72f), new Vector2(900f, 180f), TextAnchor.MiddleCenter, Color.white);
            CreateText(canvas.transform, "Subtitle", "Uma aventura de parkour vertical", 22, new Vector2(0.5f, 0.59f), new Vector2(700f, 50f), TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.58f));
            var play = CreateButton(canvas.transform, "Play", "JOGAR", new Vector2(0.5f, 0.42f));
            var quit = CreateButton(canvas.transform, "Quit", "SAIR", new Vector2(0.5f, 0.31f));
            UnityEventTools.AddPersistentListener(play.onClick, controller.LoadLevelSelect);
            UnityEventTools.AddPersistentListener(quit.onClick, controller.Quit);
            SaveScene(scene, $"{SceneRoot}/Menus/MainMenu.unity");
        }

        static void CreateLevelSelectScene(Dictionary<string, Material> materials)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateMenuBackdrop(materials, "SELEÇÃO DE FASE");
            var canvas = CreateCanvas("Level Select Canvas");
            var controller = canvas.gameObject.AddComponent<MenuController>();
            CreateText(canvas.transform, "Title", "SELEÇÃO DE FASE", 42, new Vector2(0.5f, 0.79f), new Vector2(800f, 80f), TextAnchor.MiddleCenter, Color.white);
            var farm = CreateButton(canvas.transform, "Farm", "1  FAZENDA", new Vector2(0.5f, 0.55f), new Vector2(420f, 85f));
            UnityEventTools.AddPersistentListener(farm.onClick, controller.LoadFarm);
            var best = CreateText(canvas.transform, "Farm Best", "Melhor tempo: --:--.--", 18, new Vector2(0.5f, 0.46f), new Vector2(420f, 40f), TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.60f));
            SetObjectReference(controller, "farmButton", farm);
            SetObjectReference(controller, "farmBestText", best);
            CreateText(canvas.transform, "Locked", "2  CIDADE  •  BLOQUEADA\n3  MONTANHAS  •  BLOQUEADA\n4  ESPAÇO  •  BLOQUEADA\n5  DIMENSÃO  •  BLOQUEADA", 20, new Vector2(0.5f, 0.25f), new Vector2(540f, 150f), TextAnchor.MiddleCenter, new Color(0.72f, 0.72f, 0.72f));
            SaveScene(scene, $"{SceneRoot}/Menus/LevelSelect.unity");
        }

        static void CreateFarmScene(Dictionary<string, Material> materials, LevelDefinition definition)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientLight = new Color(0.48f, 0.50f, 0.52f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.55f, 0.68f, 0.72f);
            RenderSettings.fogDensity = 0.0025f;
            CreateSun();

            var session = new GameObject("Level Session").AddComponent<LevelSession>();
            session.Configure(definition);
            var input = new GameObject("Input").AddComponent<InputReader>();
            input.SetGameplayCursor(true);
            var player = CreatePlayer(materials, input);
            var camera = CreateCamera(player.transform, input);
            player.GetComponent<PlayerMotor>().Configure(input, camera.transform);
            player.GetComponent<PlayerRespawn>().Configure(input);

            CreateComposedCourse(materials);
            CreateFarmLandmarks(materials);
            CreateBirdFlock();
            var hud = CreateGameplayUI(input, player.GetComponent<PlayerRespawn>());
            CreateTutorials(hud);
            CreateGoal(materials);

            SaveScene(scene, $"{SceneRoot}/Levels/Farm.unity");
        }

        static GameObject CreatePlayer(Dictionary<string, Material> materials, InputReader input)
        {
            var player = new GameObject("Player");
            player.tag = "Player";
            player.layer = LayerMask.NameToLayer("Player");
            player.transform.position = new Vector3(0f, 0.05f, 0f);
            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.34f;
            controller.center = Vector3.up * 0.9f;
            controller.stepOffset = 0.25f;
            var ledge = player.AddComponent<LedgeClimber>();
            ledge.Configure(LayerMask.GetMask("Climbable"));
            player.AddComponent<PlayerMotor>().Configure(input, null);
            player.AddComponent<PlayerRespawn>().Configure(input);

            var character = CreateModel(CharacterModel, "Character Visual", player.transform, Vector3.zero, Quaternion.Euler(0f, 180f, 0f), 1.75f, false);
            if (character == null)
            {
                var body = CreateVisual(PrimitiveType.Capsule, "Body", player.transform, Vector3.up * 0.95f, new Vector3(0.58f, 0.68f, 0.42f), materials["Cream"]);
                CreateVisual(PrimitiveType.Sphere, "Head", body.transform, new Vector3(0f, 0.92f, 0f), Vector3.one * 0.55f, materials["Cream"]);
                CreateVisual(PrimitiveType.Sphere, "Hair", body.transform, new Vector3(0f, 1.07f, -0.01f), new Vector3(0.58f, 0.27f, 0.58f), materials["Straw"]);
                CreateVisual(PrimitiveType.Cube, "Shirt", body.transform, new Vector3(0f, 0.18f, 0f), new Vector3(0.65f, 0.48f, 0.38f), materials["BarnRed"]);
            }
            else ConfigureCharacterAnimation(player, character);
            return player;
        }

        static void ConfigureCharacterAnimation(GameObject player, GameObject character)
        {
            var animator = character.GetComponent<Animator>();
            if (animator == null) animator = character.AddComponent<Animator>();
            animator.avatar = AssetDatabase.LoadAllAssetsAtPath(CharacterModel).OfType<Avatar>().FirstOrDefault(avatar => avatar.isValid && avatar.isHuman);
            animator.runtimeAnimatorController = CreateCharacterController();
            animator.applyRootMotion = false;

            var playerAnimator = player.AddComponent<PlayerAnimator>();
            SetObjectReference(playerAnimator, "motor", player.GetComponent<PlayerMotor>());
            SetObjectReference(playerAnimator, "animator", animator);
        }

        static AnimatorController CreateCharacterController()
        {
            const string path = Root + "/Settings/FarmCharacter.controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller != null && controller.parameters.Length > 0) return controller;
            if (controller != null) AssetDatabase.DeleteAsset(path);

            controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("VerticalSpeed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Hanging", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Climbing", AnimatorControllerParameterType.Bool);

            var blendTree = new BlendTree { name = "Locomotion", blendParameter = "Speed" };
            AssetDatabase.AddObjectToAsset(blendTree, controller);
            blendTree.AddChild(FindAnimation("Armature|Idle_Loop"), 0f);
            blendTree.AddChild(FindAnimation("Armature|Walk_Loop"), 4.5f);
            blendTree.AddChild(FindAnimation("Armature|Sprint_Loop"), 7f);
            blendTree.useAutomaticThresholds = false;

            var stateMachine = controller.layers[0].stateMachine;
            var locomotion = stateMachine.AddState("Locomotion");
            locomotion.motion = blendTree;
            stateMachine.defaultState = locomotion;
            return controller;
        }

        static AnimationClip FindAnimation(string name)
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(CharacterAnimations))
                if (asset is AnimationClip clip && clip.name == name) return clip;
            return null;
        }

        static Camera CreateCamera(Transform target, InputReader input)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 62f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 600f;
            cameraObject.AddComponent<AudioListener>();
            var follow = cameraObject.AddComponent<ThirdPersonCamera>();
            follow.Configure(target, input);
            return camera;
        }

        static void CreateCourse(Dictionary<string, Material> materials)
        {
            // A single broad foundation keeps the entrance, crops, animals and buildings
            // visually grounded instead of leaving decorations outside the playable floor.
            CreateBlock("Terreno da Fazenda", new Vector3(0f, -0.55f, 92f), new Vector3(100f, 1f, 230f), materials["Green"], "Ground");
            var position = new Vector3(0f, 0.35f, 4f);
            for (var index = 0; index < 56; index++)
            {
                var sector = index / 8;
                var within = index % 8;
                var width = Mathf.Lerp(4.2f, 2.1f, sector / 6f);
                position += new Vector3((within % 2 == 0 ? 1f : -1f) * (2.4f + sector * 0.08f), 0.72f + sector * 0.04f, 3.45f + sector * 0.1f);
                var platform = CreateBlock($"S{sector + 1}_Platform_{within + 1}", position, new Vector3(width + 0.7f, 0.45f, width), within % 3 == 0 ? materials["Straw"] : materials["Wood"], "Climbable");
                if (index == 50)
                {
                    platform.name = "Moinho Plataforma Móvel";
                    platform.layer = LayerMask.NameToLayer("MovingPlatform");
                    platform.AddComponent<MovingPlatform>();
                }

                if (within == 0)
                {
                    var recovery = CreateBlock($"S{sector + 1}_Recovery", position + new Vector3(0f, -2.4f, -4f), new Vector3(13f, 0.7f, 11f), materials["Straw"], "Ground");
                    recovery.transform.rotation = Quaternion.Euler(0f, sector * 13f, 0f);
                    CreateSplitTrigger(position + Vector3.up * 1.2f, $"Setor {sector + 1}");
                }
            }
        }

        /// <summary>
        /// Builds a route through the farm itself. Every visible landing is a real rural
        /// prop; simple cubes are retained only as invisible, predictable gameplay colliders.
        /// </summary>
        /// <param name="materials">Shared visual materials used by the course.</param>
        static void CreateComposedCourse(Dictionary<string, Material> materials)
        {
            new GameObject("CENARIO_V7_SEM_TRILHO");
            CreateBlock("Terreno da Fazenda", new Vector3(0f, -0.55f, 55f),
                new Vector3(100f, 1f, 150f), materials["Green"], "Ground");

            var anchors = new[]
            {
                new Vector3(0f, 0.35f, 4f),       // entrada
                new Vector3(-10f, 4.5f, 18f),     // feno e cercado
                new Vector3(12f, 11f, 34f),       // telhado do celeiro
                new Vector3(-12f, 14f, 49f),      // carga do galpao
                new Vector3(-10f, 18.5f, 64f),    // cobertura do silo
                new Vector3(12f, 22f, 78f),       // pomar e sacos
                new Vector3(10f, 26f, 92f),       // moinho
                new Vector3(-10f, 32f, 106f),     // armazem alto
                new Vector3(0f, 38f, 120f)        // dedo
            };

            for (var sector = 0; sector < anchors.Length - 1; sector++)
                CreateCourseSection(sector, anchors[sector], anchors[sector + 1], materials);
        }

        /// <summary>
        /// Creates one themed section. Hay, crates, barrels, carts and wooden stairs
        /// replace the former repeated slabs, so the route reads as part of the farm.
        /// </summary>
        /// <param name="sector">Zero-based sector number used for names and difficulty.</param>
        /// <param name="start">World-space beginning of the section.</param>
        /// <param name="end">World-space destination of the section.</param>
        /// <param name="materials">Shared visual materials used by the platforms.</param>
        static void CreateCourseSection(int sector, Vector3 start, Vector3 end,
            Dictionary<string, Material> materials)
        {
            var route = end - start;
            var horizontal = new Vector3(route.x, 0f, route.z).normalized;
            var side = Vector3.Cross(Vector3.up, horizontal).normalized;
            var propPaths = new[]
            {
                FarmProps + "Hay.fbx", FarmProps + "Crate.fbx",
                FarmProps + "Barrel.fbx", FarmProps + "Bags.fbx"
            };

            // Two isolated props bridge each landmark. Dense repeated sequences resemble
            // railway sleepers, so the landmark itself becomes the third landing.
            for (var step = 0; step < 2; step++)
            {
                var progress = (step + 1f) / 3f;
                var position = Vector3.Lerp(start, end, progress);
                var weave = step == 0 ? -5.5f : 5.5f;
                position += side * weave;

                var propIndex = (sector * 2 + step) % propPaths.Length;
                var colliderSize = propIndex switch
                {
                    0 => new Vector3(2.5f, 1.15f, 2.2f), // feno
                    1 => new Vector3(2.3f, 1.15f, 2.3f), // caixa
                    2 => new Vector3(2.1f, 1.4f, 2.1f),  // barril
                    _ => new Vector3(2.6f, 1.05f, 2.2f)  // sacos
                };
                var rotation = Quaternion.LookRotation(horizontal, Vector3.up) *
                    Quaternion.Euler(0f, (step % 3 - 1) * 8f, 0f);
                var platform = CreateFarmPropLanding(
                    $"Setor {sector + 1} - {System.IO.Path.GetFileNameWithoutExtension(propPaths[propIndex])} {step + 1}",
                    propPaths[propIndex], position, rotation, colliderSize);

                if (sector == 5 && step == 4)
                {
                    platform.name = "Plataforma Movel do Moinho";
                    platform.layer = LayerMask.NameToLayer("MovingPlatform");
                    platform.AddComponent<MovingPlatform>();
                }
            }

            // The checkpoint surface is deliberately invisible. Nearby barn, silo and
            // windmill geometry provide its visual context without another broad slab.
            var restDeck = CreateBlock($"Setor {sector + 1} - Checkpoint Invisivel",
                end - Vector3.up * 0.3f, new Vector3(4.2f, 0.6f, 4.2f), null, "Climbable");
            SetVisible(restDeck, false);
            CreateSplitTrigger(end + Vector3.up * 1.5f, $"Setor {sector + 1}");
        }

        /// <summary>
        /// Places one farm model over an invisible box collider. This keeps the silhouette
        /// authentic while providing a fair and stable surface for the player controller.
        /// </summary>
        /// <param name="name">Readable scene name for the obstacle.</param>
        /// <param name="modelPath">Project path of the FBX used as the visible prop.</param>
        /// <param name="topCenter">World position of the playable top surface.</param>
        /// <param name="rotation">Orientation that follows the course direction.</param>
        /// <param name="colliderSize">Width, height and depth of the gameplay collider.</param>
        /// <returns>The collider root, or the visible fallback block if the model is missing.</returns>
        static GameObject CreateFarmPropLanding(string name, string modelPath, Vector3 topCenter,
            Quaternion rotation, Vector3 colliderSize)
        {
            var center = topCenter - Vector3.up * colliderSize.y * 0.5f;
            var collider = CreateBlock(name + " Collider", center, colliderSize, null, "Climbable");
            collider.transform.rotation = rotation;

            SetVisible(collider, false);
            CreateStableFarmProp(name, modelPath, center, rotation, colliderSize);

            return collider;
        }

        /// <summary>
        /// Creates a recognisable farm obstacle with exact metric dimensions. This is used
        /// for gameplay because several downloaded FBX props contain inconsistent axes.
        /// </summary>
        /// <param name="name">Name used in the hierarchy.</param>
        /// <param name="sourceHint">Original asset path; its filename selects the prop style.</param>
        /// <param name="center">World-space centre of the obstacle.</param>
        /// <param name="rotation">World-space orientation of the obstacle.</param>
        /// <param name="size">Exact gameplay dimensions.</param>
        /// <returns>The visual-only root object.</returns>
        static GameObject CreateStableFarmProp(string name, string sourceHint, Vector3 center,
            Quaternion rotation, Vector3 size)
        {
            var root = new GameObject(name + " Visual");
            root.transform.SetPositionAndRotation(center, rotation);
            var straw = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialRoot}/Straw.mat");
            var wood = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialRoot}/Wood.mat");
            var metal = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialRoot}/Metal.mat");
            var file = System.IO.Path.GetFileNameWithoutExtension(sourceHint);

            if (file == "Barrel")
            {
                var body = CreateVisual(PrimitiveType.Cylinder, "Barril", root.transform, Vector3.zero,
                    new Vector3(size.x * 0.42f, size.y * 0.5f, size.z * 0.42f), wood);
                body.transform.localRotation = Quaternion.identity;
                CreateVisual(PrimitiveType.Cylinder, "Aro Superior", root.transform, Vector3.up * size.y * 0.34f,
                    new Vector3(size.x * 0.46f, 0.035f, size.z * 0.46f), metal);
                CreateVisual(PrimitiveType.Cylinder, "Aro Inferior", root.transform, Vector3.down * size.y * 0.34f,
                    new Vector3(size.x * 0.46f, 0.035f, size.z * 0.46f), metal);
            }
            else if (file == "Crate")
            {
                CreateVisual(PrimitiveType.Cube, "Caixa", root.transform, Vector3.zero, size * 0.9f, wood);
                for (var i = -1; i <= 1; i += 2)
                    CreateVisual(PrimitiveType.Cube, $"Ripa {i}", root.transform,
                        new Vector3(i * size.x * 0.28f, 0f, -size.z * 0.46f),
                        new Vector3(0.14f, size.y * 0.92f, 0.12f), straw);
            }
            else if (file == "Bags")
            {
                for (var i = -1; i <= 1; i++)
                    CreateVisual(PrimitiveType.Sphere, $"Saco {i + 2}", root.transform,
                        new Vector3(i * size.x * 0.27f, -size.y * 0.08f, 0f),
                        new Vector3(size.x * 0.42f, size.y * 0.72f, size.z * 0.72f), straw);
            }
            else
            {
                CreateVisual(PrimitiveType.Cube, "Fardo de Feno", root.transform, Vector3.zero, size * 0.92f, straw);
                for (var i = -1; i <= 1; i += 2)
                    CreateVisual(PrimitiveType.Cube, $"Corda {i}", root.transform,
                        new Vector3(i * size.x * 0.24f, 0f, 0f),
                        new Vector3(0.09f, size.y * 0.98f, size.z * 0.98f), wood);
            }

            return root;
        }

        static void CreateFarmLandmarks(Dictionary<string, Material> materials)
        {
            CreateBarn(new Vector3(12f, 0f, 34f), materials);
            CreateSilo(new Vector3(-10f, 0f, 64f), materials);
            CreateWindmill(new Vector3(10f, 0f, 92f), materials);

            // Trees frame the farm entrance at ground level. Elevated landmarks remain
            // dedicated to the parkour route, avoiding the previous floating-tree effect.
            var treePositions = new[]
            {
                new Vector3(-29f, 0f, -12f), new Vector3(-22f, 0f, -18f),
                new Vector3(-30f, 0f, 0f), new Vector3(-27f, 0f, 14f),
                new Vector3(-31f, 0f, 29f), new Vector3(-24f, 0f, 42f),
                new Vector3(-34f, 0f, 72f), new Vector3(-32f, 0f, 112f),
                new Vector3(-35f, 0f, 148f), new Vector3(-31f, 0f, 187f),
                new Vector3(29f, 0f, -13f), new Vector3(22f, 0f, -19f),
                new Vector3(31f, 0f, 1f), new Vector3(27f, 0f, 16f),
                new Vector3(30f, 0f, 30f), new Vector3(23f, 0f, 44f),
                new Vector3(34f, 0f, 78f), new Vector3(31f, 0f, 118f),
                new Vector3(35f, 0f, 157f), new Vector3(30f, 0f, 198f)
            };
            for (var i = 0; i < treePositions.Length; i++)
            {
                var treePath = Nature + $"CommonTree_{i % 5 + 1}.fbx";
                CreateModel(treePath, $"Arvore {i + 1}", null, treePositions[i], Quaternion.Euler(0f, i * 47f, 0f), 6.5f + i % 3, true);
            }

            CreateFarmDecorations();
            CreateSiloYard(new Vector3(-10f, 0f, 64f));
            CreateWindmillYard(new Vector3(10f, 0f, 92f));
        }

        static void CreateBarn(Vector3 groundPosition, Dictionary<string, Material> materials)
        {
            var barn = CreateBlock("Celeiro Collider", groundPosition + Vector3.up * 4.5f, new Vector3(12f, 9f, 15f), materials["BarnRed"], "CameraBlocker");
            var roof = CreateBlock("Celeiro Telhado", groundPosition + Vector3.up * 10.5f, new Vector3(13f, 1.1f, 16f), materials["Roof"], "Climbable");
            roof.transform.rotation = Quaternion.Euler(0f, 0f, 7f);
            var model = CreateModel(FarmBuildings + "BigBarn.fbx", "Celeiro", null, groundPosition, Quaternion.Euler(0f, 90f, 0f), 13f, true);
            if (model != null)
            {
                SetVisible(barn, false);
                SetVisible(roof, false);
            }
        }

        static void CreateSilo(Vector3 groundPosition, Dictionary<string, Material> materials)
        {
            var silo = CreateBlock("Silo Collider", groundPosition + Vector3.up * 9f, new Vector3(7f, 18f, 7f), materials["Metal"], "CameraBlocker");
            silo.layer = LayerMask.NameToLayer("CameraBlocker");
            SetVisible(silo, false);
            var visual = new GameObject("Silo Vertical");
            visual.transform.position = groundPosition;
            CreateVisual(PrimitiveType.Cylinder, "Corpo Metalico", visual.transform, Vector3.up * 7.5f,
                new Vector3(3.3f, 7.5f, 3.3f), materials["Metal"]);
            CreateVisual(PrimitiveType.Sphere, "Cobertura Curva", visual.transform, Vector3.up * 15.2f,
                new Vector3(3.45f, 1.7f, 3.45f), materials["Roof"]);
            for (var i = 0; i < 4; i++)
                CreateVisual(PrimitiveType.Cube, $"Cinta {i + 1}", visual.transform,
                    Vector3.up * (3f + i * 3f), new Vector3(6.9f, 0.12f, 0.18f), materials["Cream"]);
        }

        static void CreateWindmill(Vector3 groundPosition, Dictionary<string, Material> materials)
        {
            var tower = CreateBlock("Torre do Moinho", groundPosition + Vector3.up * 7f, new Vector3(4f, 14f, 4f), materials["Cream"], "CameraBlocker");
            var rotor = new GameObject("Pás do Moinho");
            rotor.transform.position = groundPosition + new Vector3(0f, 12f, -2.2f);
            for (var i = 0; i < 4; i++)
            {
                var blade = CreateVisual(PrimitiveType.Cube, $"Pá {i + 1}", rotor.transform, Quaternion.Euler(0f, 0f, i * 90f) * Vector3.up * 3f, new Vector3(0.7f, 6f, 0.25f), materials["Wood"]);
                blade.transform.localRotation = Quaternion.Euler(0f, 0f, i * 90f);
            }
            rotor.AddComponent<WindmillRotor>();
            if (CreateModel(FarmBuildings + "TowerWindmill.fbx", "Moinho Visual", null, groundPosition, Quaternion.Euler(0f, 180f, 0f), 21f, true) != null)
            {
                SetVisible(tower, false);
                SetVisible(rotor, false);
            }
        }

        static void CreateFarmDecorations()
        {
            // The entrance remains open for the tutorial while buildings form readable
            // clusters on the sides of the playable route.
            CreateModel(TractorModel, "Trator", null, new Vector3(16f, 0f, -7f), Quaternion.Euler(0f, -25f, 0f), 2.7f, true);
            CreateModel(FarmBuildings + "OpenBarn.fbx", "Galpao", null, new Vector3(-19f, 0f, -7f), Quaternion.Euler(0f, 18f, 0f), 6.5f, true);
            CreateModel(FarmBuildings + "ChickenCoop.fbx", "Galinheiro", null, new Vector3(-20f, 0f, 15f), Quaternion.Euler(0f, 12f, 0f), 3.2f, true);
            CreateModel(FarmBuildings + "Well.fbx", "Poco", null, new Vector3(18f, 0f, 12f), Quaternion.identity, 2.8f, true);

            // Two straight fence lines define a crop field without blocking the first jumps.
            for (var i = 0; i < 8; i++)
            {
                var z = 23f + i * 3.1f;
                CreateModel(FarmBuildings + (i % 2 == 0 ? "Fence.fbx" : "Fence2.fbx"), $"Cerca Esquerda {i + 1}", null, new Vector3(-25f, 0f, z), Quaternion.Euler(0f, 90f, 0f), 1.4f, true);
                CreateModel(FarmBuildings + (i % 2 == 0 ? "Fence2.fbx" : "Fence.fbx"), $"Cerca Direita {i + 1}", null, new Vector3(-8f, 0f, z), Quaternion.Euler(0f, 90f, 0f), 1.4f, true);
            }

            for (var row = 0; row < 3; row++)
            {
                for (var column = 0; column < 5; column++)
                {
                    var crop = (row + column) % 2 == 0 ? "Wheat_Crop.fbx" : "Corn_Crop.fbx";
                    CreateModel(Crops + crop, $"Plantacao {row + 1}-{column + 1}", null,
                        new Vector3(-21f + column * 2.5f, 0f, 26f + row * 7f), Quaternion.identity, 1.5f, true);
                }
            }

            CreateGroundPropClusters();
            CreateWanderingAnimal("Vaca 1", "Cow.fbx", new Vector3(22f, 0f, 24f), 1.7f, 7f, 0.65f);
            CreateWanderingAnimal("Vaca 2", "Cow.fbx", new Vector3(29f, 0f, 52f), 1.6f, 6f, 0.58f);
            CreateWanderingAnimal("Cavalo", "Horse.fbx", new Vector3(27f, 0f, 34f), 2.1f, 8f, 0.85f);
            CreateWanderingAnimal("Ovelha 1", "Sheep.fbx", new Vector3(18f, 0f, 42f), 1.2f, 6f, 0.48f);
            CreateWanderingAnimal("Ovelha 2", "Sheep.fbx", new Vector3(24f, 0f, 47f), 1.1f, 5f, 0.52f);
            CreateWanderingAnimal("Porco", "Pig.fbx", new Vector3(-31f, 0f, 58f), 1.0f, 5f, 0.45f);
            CreateWanderingAnimal("Lhama", "Llama.fbx", new Vector3(31f, 0f, 79f), 1.8f, 6f, 0.62f);
        }

        /// <summary>Creates believable work piles across the lower farm without blocking the route.</summary>
        static void CreateGroundPropClusters()
        {
            var positions = new[]
            {
                new Vector3(9f, 0f, 10f), new Vector3(12f, 0f, 13f), new Vector3(-7f, 0f, 18f),
                new Vector3(7f, 0f, 31f), new Vector3(20f, 0f, 57f), new Vector3(-29f, 0f, 70f),
                new Vector3(8f, 0f, 83f), new Vector3(28f, 0f, 105f)
            };
            var models = new[] { "Hay.fbx", "Crate.fbx", "Barrel.fbx", "Bags.fbx" };
            for (var i = 0; i < positions.Length; i++)
            {
                var size = new Vector3(2.2f, 1.2f + i % 3 * 0.2f, 2f);
                CreateStableFarmProp($"Material da Fazenda {i + 1}", FarmProps + models[i % models.Length],
                    positions[i] + Vector3.up * size.y * 0.5f, Quaternion.Euler(0f, i * 41f, 0f), size);
            }
            CreateStableFarmProp("Carga do Celeiro", FarmProps + "Crate.fbx",
                new Vector3(22f, 0.8f, 58f), Quaternion.Euler(0f, -35f, 0f), new Vector3(3f, 1.6f, 2.6f));
            CreateStableFarmProp("Feno do Galpao", FarmProps + "Hay.fbx",
                new Vector3(-14f, 0.7f, 2f), Quaternion.Euler(0f, 90f, 0f), new Vector3(3f, 1.4f, 2.2f));
        }

        /// <summary>Surrounds the silo with storage and a fence so it belongs to the farmyard.</summary>
        /// <param name="center">Ground position of the silo.</param>
        static void CreateSiloYard(Vector3 center)
        {
            CreateModel(FarmBuildings + "SmallBarn.fbx", "Deposito do Silo", null,
                center + new Vector3(12f, 0f, 4f), Quaternion.Euler(0f, -90f, 0f), 5.5f, true);
            for (var i = 0; i < 5; i++)
                CreateModel(FarmBuildings + "Fence.fbx", $"Cerca do Silo {i + 1}", null,
                    center + new Vector3(-8f + i * 4f, 0f, -8f), Quaternion.identity, 1.3f, true);
            CreateStableFarmProp("Feno do Silo", FarmProps + "Hay.fbx", center + new Vector3(7f, 0.7f, -4f), Quaternion.identity, new Vector3(2.8f, 1.4f, 2.2f));
            CreateStableFarmProp("Barril do Silo", FarmProps + "Barrel.fbx", center + new Vector3(-6f, 0.8f, 4f), Quaternion.identity, new Vector3(1.6f, 1.6f, 1.6f));
        }

        /// <summary>Creates crops and supplies around the windmill to explain its location.</summary>
        /// <param name="center">Ground position of the windmill.</param>
        static void CreateWindmillYard(Vector3 center)
        {
            for (var row = 0; row < 3; row++)
                for (var column = 0; column < 4; column++)
                    CreateModel(Crops + (column % 2 == 0 ? "Wheat_4.fbx" : "Corn_4.fbx"),
                        $"Plantacao do Moinho {row + 1}-{column + 1}", null,
                        center + new Vector3(-14f + column * 2.4f, 0f, -5f + row * 3f), Quaternion.identity, 1.3f, true);
            CreateStableFarmProp("Carga do Moinho", FarmProps + "Crate.fbx",
                center + new Vector3(8f, 0.8f, -5f), Quaternion.Euler(0f, 30f, 0f), new Vector3(2.8f, 1.6f, 2.4f));
            CreateStableFarmProp("Sacos de Graos do Moinho", FarmProps + "Bags.fbx",
                center + new Vector3(5f, 0.65f, 3f), Quaternion.Euler(0f, -20f, 0f), new Vector3(2.6f, 1.3f, 2.2f));
        }

        /// <summary>Creates one animal and gives it a bounded autonomous walking area.</summary>
        /// <param name="name">Name displayed in the scene hierarchy.</param>
        /// <param name="file">Animal FBX filename in the Quaternius pack.</param>
        /// <param name="position">Centre of the animal's grazing area.</param>
        /// <param name="height">Desired visual height in metres.</param>
        /// <param name="radius">Maximum distance travelled from the grazing centre.</param>
        /// <param name="speed">Walking speed in metres per second.</param>
        static void CreateWanderingAnimal(string name, string file, Vector3 position, float height, float radius, float speed)
        {
            var animal = CreateModel(Animals + file, name, null, position, Quaternion.identity, height, false);
            if (animal != null) animal.AddComponent<AnimalWander>().Configure(position, radius, speed);
        }

        static void CreateBirdFlock()
        {
            var flock = new GameObject("Bando de Passaros");
            var centers = new[]
            {
                new Vector3(0f, 15f, 34f),
                new Vector3(-8f, 25f, 96f),
                new Vector3(10f, 35f, 154f)
            };

            for (var i = 0; i < 7; i++)
            {
                var bird = new GameObject($"Passaro {i + 1}");
                bird.transform.SetParent(flock.transform);
                bird.AddComponent<BirdFlight>().Configure(
                    centers[i % centers.Length], 7f + i * 1.3f, 7f + i * 0.8f, i * 0.85f);
                CreateModel(BirdModel, "Modelo", bird.transform, Vector3.zero,
                    Quaternion.Euler(0f, 90f, 0f), 1.15f + i % 3 * 0.12f, false);
            }
        }

        static GameplayHUD CreateGameplayUI(InputReader input, PlayerRespawn respawn)
        {
            var canvas = CreateCanvas("Gameplay Canvas");
            var timer = CreateText(canvas.transform, "Timer", "00:00.00", 30, new Vector2(0.08f, 0.92f), new Vector2(230f, 55f), TextAnchor.MiddleLeft, Color.white);
            var prompt = CreateText(canvas.transform, "Prompt", string.Empty, 24, new Vector2(0.5f, 0.16f), new Vector2(760f, 80f), TextAnchor.MiddleCenter, Color.white);
            var hud = canvas.gameObject.AddComponent<GameplayHUD>();
            hud.Configure(timer, prompt);

            var pausePanel = CreatePanel(canvas.transform, "Pause Panel", new Color(0.03f, 0.04f, 0.04f, 0.94f));
            CreateText(pausePanel.transform, "Pause Title", "PAUSADO", 40, new Vector2(0.5f, 0.72f), new Vector2(500f, 80f), TextAnchor.MiddleCenter, Color.white);
            var pause = canvas.gameObject.AddComponent<PauseMenu>();
            var resume = CreateButton(pausePanel.transform, "Resume", "CONTINUAR", new Vector2(0.5f, 0.53f));
            var restart = CreateButton(pausePanel.transform, "Restart", "VOLTAR AO INÍCIO", new Vector2(0.5f, 0.41f));
            var menu = CreateButton(pausePanel.transform, "Menu", "MENU PRINCIPAL", new Vector2(0.5f, 0.29f));
            UnityEventTools.AddPersistentListener(resume.onClick, pause.Resume);
            UnityEventTools.AddPersistentListener(restart.onClick, pause.RestartPosition);
            UnityEventTools.AddPersistentListener(menu.onClick, pause.MainMenu);
            pause.Configure(input, pausePanel, respawn);

            var completionPanel = CreatePanel(canvas.transform, "Completion Panel", new Color(0.06f, 0.07f, 0.06f, 0.96f));
            var result = CreateText(completionPanel.transform, "Result", "FASE CONCLUÍDA", 32, new Vector2(0.5f, 0.64f), new Vector2(650f, 260f), TextAnchor.MiddleCenter, Color.white);
            var completion = canvas.gameObject.AddComponent<CompletionPanel>();
            var replay = CreateButton(completionPanel.transform, "Replay", "JOGAR NOVAMENTE", new Vector2(0.5f, 0.35f));
            var select = CreateButton(completionPanel.transform, "Select", "SELEÇÃO DE FASES", new Vector2(0.5f, 0.23f));
            UnityEventTools.AddPersistentListener(replay.onClick, completion.Replay);
            UnityEventTools.AddPersistentListener(select.onClick, completion.LevelSelect);
            completion.Configure(completionPanel, result);
            return hud;
        }

        static void CreateTutorials(GameplayHUD hud)
        {
            CreateTutorialTrigger(new Vector3(0f, 1f, 2f), new Vector3(8f, 3f, 5f), hud, "WASD para mover  •  Mouse para olhar  •  Espaço para pular");
            CreateTutorialTrigger(new Vector3(0f, 19f, 43f), new Vector3(10f, 5f, 8f), hud, "Segure Shift para correr e ganhar impulso");
            CreateTutorialTrigger(new Vector3(0f, 34f, 72f), new Vector3(10f, 5f, 8f), hud, "Ao agarrar uma borda, pressione Espaço para subir");
            CreateTutorialTrigger(new Vector3(0f, 2f, -5f), new Vector3(10f, 4f, 5f), hud, "R retorna ao início sem reiniciar o cronômetro");
        }

        static void CreateGoal(Dictionary<string, Material> materials)
        {
            var goal = new GameObject("Primeiro Dedo");
            goal.transform.position = new Vector3(0f, 39.5f, 120f);
            goal.layer = LayerMask.NameToLayer("Collectible");
            var collider = goal.AddComponent<CapsuleCollider>();
            collider.isTrigger = true;
            collider.height = 2.1f;
            collider.radius = 0.65f;
            goal.AddComponent<FingerCollectible>();
            CreateVisual(PrimitiveType.Capsule, "Dedo", goal.transform, Vector3.zero, new Vector3(0.8f, 1.2f, 0.8f), materials["Goal"]);
            var light = goal.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.55f, 0.18f);
            light.range = 8f;
            light.intensity = 4f;
        }

        static void CreateTutorialTrigger(Vector3 position, Vector3 size, GameplayHUD hud, string message)
        {
            var go = new GameObject("Tutorial " + message.Split(' ')[0]);
            go.transform.position = position;
            var collider = go.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = size;
            go.AddComponent<TutorialTrigger>().Configure(hud, message);
        }

        static void CreateSplitTrigger(Vector3 position, string sectorName)
        {
            var go = new GameObject(sectorName + " Split");
            go.transform.position = position;
            var collider = go.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(9f, 4f, 9f);
            var split = go.AddComponent<SplitTimerTrigger>();
            SetString(split, "sectorName", sectorName);
        }

        static void CreateSun()
        {
            var sun = new GameObject("Sun");
            sun.transform.rotation = Quaternion.Euler(42f, -32f, 0f);
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.78f, 0.58f);
            light.intensity = 1.25f;
            light.shadows = LightShadows.Soft;
        }

        static void CreateMenuBackdrop(Dictionary<string, Material> materials, string name)
        {
            CreateSun();
            var camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.SetPositionAndRotation(new Vector3(0f, 7f, -18f), Quaternion.Euler(12f, 0f, 0f));
            camera.backgroundColor = new Color(0.12f, 0.20f, 0.24f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.gameObject.AddComponent<AudioListener>();
            CreateBlock(name, new Vector3(0f, 0f, 7f), new Vector3(18f, 9f, 2f), materials["BarnRed"], "Ground");
            CreateBlock("Roof", new Vector3(0f, 5f, 7f), new Vector3(20f, 1.4f, 3f), materials["Roof"], "Ground");
        }

        static Canvas CreateCanvas(string name)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem.transform.SetParent(go.transform);
            return canvas;
        }

        static Text CreateText(Transform parent, string name, string value, int size, Vector2 anchor, Vector2 dimensions, TextAnchor alignment, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = anchor;
            rect.sizeDelta = dimensions;
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        static Button CreateButton(Transform parent, string name, string label, Vector2 anchor, Vector2? dimensions = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = anchor;
            rect.sizeDelta = dimensions ?? new Vector2(360f, 72f);
            go.GetComponent<Image>().color = new Color(0.55f, 0.16f, 0.09f, 0.96f);
            var button = go.GetComponent<Button>();
            CreateText(go.transform, "Label", label, 22, new Vector2(0.5f, 0.5f), rect.sizeDelta, TextAnchor.MiddleCenter, Color.white);
            return button;
        }

        static GameObject CreatePanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = color;
            return go;
        }

        static GameObject CreateBlock(string name, Vector3 position, Vector3 scale, Material material, string layer)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetPositionAndRotation(position, Quaternion.identity);
            go.transform.localScale = scale;
            go.layer = LayerMask.NameToLayer(layer);
            var renderer = go.GetComponent<Renderer>();
            if (material != null) renderer.sharedMaterial = material;
            return go;
        }

        static GameObject CreateVisual(PrimitiveType type, string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
                go.transform.localPosition = position;
            }
            else go.transform.position = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        static GameObject CreateModel(string path, string name, Transform parent, Vector3 bottomCenter, Quaternion rotation, float targetHeight, bool makeStatic)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null)
            {
                Debug.LogWarning($"Modelo nao encontrado: {path}");
                return null;
            }

            var instance = PrefabUtility.InstantiatePrefab(asset) as GameObject;
            if (instance == null) return null;

            instance.name = name;
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = rotation;
            instance.transform.localScale = new Vector3(
                Mathf.Abs(instance.transform.localScale.x),
                Mathf.Abs(instance.transform.localScale.y),
                Mathf.Abs(instance.transform.localScale.z));

            foreach (var collider in instance.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(collider);

            var renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                Object.DestroyImmediate(instance);
                Debug.LogWarning($"Modelo sem renderer: {path}");
                return null;
            }

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            if (bounds.size.y > 0.001f) instance.transform.localScale *= targetHeight / bounds.size.y;

            bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            var target = parent == null ? bottomCenter : parent.TransformPoint(bottomCenter);
            instance.transform.position += target - new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);

            if (makeStatic)
            {
                foreach (var child in instance.GetComponentsInChildren<Transform>(true))
                    GameObjectUtility.SetStaticEditorFlags(child.gameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.ReflectionProbeStatic);
            }

            return instance;
        }

        static void SetVisible(GameObject root, bool visible)
        {
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true)) renderer.enabled = visible;
        }

        static void SaveScene(Scene scene, string path)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, path);
        }

        static void ConfigureBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene($"{SceneRoot}/Bootstrap.unity", true),
                new EditorBuildSettingsScene($"{SceneRoot}/Menus/MainMenu.unity", true),
                new EditorBuildSettingsScene($"{SceneRoot}/Menus/LevelSelect.unity", true),
                new EditorBuildSettingsScene($"{SceneRoot}/Levels/Farm.unity", true)
            };
        }

        static void SetObjectReference(Object target, string propertyName, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(propertyName).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetString(Object target, string propertyName, string value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(propertyName).stringValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }

}
#endif

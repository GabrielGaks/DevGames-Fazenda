#if UNITY_EDITOR
using System.Collections.Generic;
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

        static readonly Color BarnRed = new(0.48f, 0.10f, 0.07f);
        static readonly Color Wood = new(0.30f, 0.17f, 0.08f);
        static readonly Color Straw = new(0.78f, 0.57f, 0.18f);
        static readonly Color Green = new(0.20f, 0.36f, 0.16f);
        static readonly Color Roof = new(0.12f, 0.15f, 0.17f);
        static readonly Color Cream = new(0.86f, 0.80f, 0.65f);

        [MenuItem("DevGames/Build Farm Project")]
        public static void BuildAll()
        {
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

            CreateCourse(materials);
            CreateFarmLandmarks(materials);
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
            blendTree.AddChild(FindAnimation("Armature|Walk_Loop"), 2f);
            blendTree.AddChild(FindAnimation("Armature|Sprint_Loop"), 7f);

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
            CreateBlock("Terreiro", new Vector3(0f, -0.55f, 0f), new Vector3(34f, 1f, 34f), materials["Green"], "Ground");
            var position = new Vector3(0f, 0.35f, 4f);
            for (var index = 0; index < 56; index++)
            {
                var sector = index / 8;
                var within = index % 8;
                var width = Mathf.Lerp(4.2f, 2.1f, sector / 6f);
                position += new Vector3((within % 2 == 0 ? 1f : -1f) * (2.0f + sector * 0.12f), 1.35f, 2.65f + sector * 0.14f);
                var platform = CreateBlock($"S{sector + 1}_Platform_{within + 1}", position, new Vector3(width, 0.45f, width * 0.9f), within % 3 == 0 ? materials["Straw"] : materials["Wood"], "Climbable");
                if (index == 50)
                {
                    platform.name = "Moinho Plataforma Móvel";
                    platform.layer = LayerMask.NameToLayer("MovingPlatform");
                    platform.AddComponent<MovingPlatform>();
                }

                if (within == 0)
                {
                    var recovery = CreateBlock($"S{sector + 1}_Recovery", position + new Vector3(0f, -4.3f, -4f), new Vector3(11f, 0.7f, 10f), materials["Straw"], "Ground");
                    recovery.transform.rotation = Quaternion.Euler(0f, sector * 13f, 0f);
                    CreateSplitTrigger(position + Vector3.up * 1.2f, $"Setor {sector + 1}");
                }
            }
        }

        static void CreateFarmLandmarks(Dictionary<string, Material> materials)
        {
            CreateBarn(new Vector3(11f, 8f, 35f), materials);
            CreateSilo(new Vector3(-12f, 43f, 105f), materials);
            CreateWindmill(new Vector3(9f, 72f, 161f), materials);
            for (var i = 0; i < 18; i++)
            {
                var x = i % 2 == 0 ? -12f : 12f;
                var z = 8f + i * 8f;
                var treePath = Nature + $"CommonTree_{i % 5 + 1}.fbx";
                CreateModel(treePath, $"Tree_{i + 1}", null, new Vector3(x, Mathf.Max(0f, i * 4f), z), Quaternion.Euler(0f, i * 37f, 0f), 6.5f + i % 3, true);
            }

            CreateFarmDecorations();
        }

        static void CreateBarn(Vector3 position, Dictionary<string, Material> materials)
        {
            var barn = CreateBlock("Celeiro Collider", position, new Vector3(12f, 9f, 15f), materials["BarnRed"], "CameraBlocker");
            var roof = CreateBlock("Celeiro Telhado", position + Vector3.up * 6f, new Vector3(13f, 1.1f, 16f), materials["Roof"], "Climbable");
            roof.transform.rotation = Quaternion.Euler(0f, 0f, 7f);
            var model = CreateModel(FarmBuildings + "BigBarn.fbx", "Celeiro", null, position + Vector3.down * 4.5f, Quaternion.Euler(0f, 90f, 0f), 13f, true);
            if (model != null)
            {
                SetVisible(barn, false);
                SetVisible(roof, false);
            }
        }

        static void CreateSilo(Vector3 position, Dictionary<string, Material> materials)
        {
            var silo = CreateVisual(PrimitiveType.Cylinder, "Silo", null, position, new Vector3(5f, 12f, 5f), materials["Metal"]);
            silo.layer = LayerMask.NameToLayer("CameraBlocker");
            if (CreateModel(FarmBuildings + "Silo.fbx", "Silo Visual", null, position + Vector3.down * 12f, Quaternion.identity, 24f, true) != null) SetVisible(silo, false);
        }

        static void CreateWindmill(Vector3 position, Dictionary<string, Material> materials)
        {
            var tower = CreateBlock("Torre do Moinho", position, new Vector3(4f, 14f, 4f), materials["Cream"], "CameraBlocker");
            var rotor = new GameObject("Pás do Moinho");
            rotor.transform.position = position + new Vector3(0f, 5f, -2.2f);
            for (var i = 0; i < 4; i++)
            {
                var blade = CreateVisual(PrimitiveType.Cube, $"Pá {i + 1}", rotor.transform, Quaternion.Euler(0f, 0f, i * 90f) * Vector3.up * 3f, new Vector3(0.7f, 6f, 0.25f), materials["Wood"]);
                blade.transform.localRotation = Quaternion.Euler(0f, 0f, i * 90f);
            }
            rotor.AddComponent<WindmillRotor>();
            if (CreateModel(FarmBuildings + "TowerWindmill.fbx", "Moinho Visual", null, position + Vector3.down * 7f, Quaternion.Euler(0f, 180f, 0f), 21f, true) != null)
            {
                SetVisible(tower, false);
                SetVisible(rotor, false);
            }
        }

        static void CreateFarmDecorations()
        {
            CreateModel(TractorModel, "Trator", null, new Vector3(8f, 0f, -5f), Quaternion.Euler(0f, -35f, 0f), 2.7f, true);
            CreateModel(FarmBuildings + "OpenBarn.fbx", "Galpao", null, new Vector3(-11f, 0f, -5f), Quaternion.Euler(0f, 25f, 0f), 6.5f, true);
            CreateModel(FarmBuildings + "ChickenCoop.fbx", "Galinheiro", null, new Vector3(-10f, 0f, 15f), Quaternion.Euler(0f, 20f, 0f), 3.2f, true);
            CreateModel(FarmBuildings + "Well.fbx", "Poco", null, new Vector3(10f, 0f, 13f), Quaternion.identity, 2.8f, true);

            for (var i = 0; i < 7; i++)
            {
                CreateModel(FarmBuildings + (i % 2 == 0 ? "Fence.fbx" : "Fence2.fbx"), $"Cerca {i + 1}", null, new Vector3(-15f + i * 5f, 0f, 20f), Quaternion.identity, 1.4f, true);
                CreateModel(Crops + (i % 2 == 0 ? "Wheat_Crop.fbx" : "Corn_Crop.fbx"), $"Plantacao {i + 1}", null, new Vector3(-13f + i * 4.2f, 0f, 26f), Quaternion.identity, 1.5f, true);
            }

            CreateModel(Animals + "Cow.fbx", "Vaca", null, new Vector3(-10f, 0f, 8f), Quaternion.Euler(0f, 35f, 0f), 1.7f, false);
            CreateModel(Animals + "Horse.fbx", "Cavalo", null, new Vector3(11f, 0f, 22f), Quaternion.Euler(0f, -60f, 0f), 2.1f, false);
            CreateModel(Animals + "Sheep.fbx", "Ovelha", null, new Vector3(-8f, 0f, 23f), Quaternion.Euler(0f, 15f, 0f), 1.2f, false);
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
            goal.transform.position = new Vector3(0f, 78f, 179f);
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
            go.GetComponent<Renderer>().sharedMaterial = material;
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

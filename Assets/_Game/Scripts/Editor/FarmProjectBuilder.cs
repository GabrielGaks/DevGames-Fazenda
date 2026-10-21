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

        /// <summary>Applies the organic farm revision and emits an unmistakable confirmation.</summary>
        [MenuItem("DevGames/APLICAR FAZENDA VERTICAL V11")]
        public static void ApplyVisualFixV6()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("Pare o Play Mode antes de aplicar a FAZENDA VERTICAL V11.");
                return;
            }

            BuildAll();
            Debug.Log("FAZENDA VERTICAL V11 salva em Farm.unity - abra Bootstrap e pressione Play.");
        }

        [InitializeOnLoadMethod]
        static void ApplyOrganicFarmOnce()
        {
            if (Application.isBatchMode) return;
            var farmScene = $"{SceneRoot}/Levels/Farm.unity";
            if (System.IO.File.Exists(farmScene) && System.IO.File.ReadAllText(farmScene).Contains("FAZENDA_VERTICAL_V11")) return;
            EditorApplication.delayCall += () =>
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling) ApplyVisualFixV6();
            };
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
            CreateMaterial("Soil", new Color(0.34f, 0.22f, 0.12f), result);
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

        /// <summary>Builds a hillside farm whose parkour is attached to working areas.</summary>
        // Course tuning. Reach comes from PlayerMotor (jumpHeight 1.6, gravity -24, sprint 7).
        const float StepRise = 1.2f;
        const float MaxRise = 1.4f;
        const float MaxReach = 5f;
        const float BridgeSpacing = 4.3f;
        const float MinPropSpacing = 3.2f;
        const float MinHeadroom = 3f;
        const float DeckThickness = 0.4f;
        const float DeckLanding = 1.8f;
        const float IslandRadius = 9f;
        const float IslandLanding = 7.5f;
        static readonly Vector3 RoutePropSize = new(3f, 1.1f, 2.6f);
        static readonly string[] RouteProps = { "Hay.fbx", "Crate.fbx", "Bags.fbx", "Hay.fbx", "Barrel.fbx" };

        // The barn stands on the field; the other landmarks float on islands above it.
        static readonly Vector2 BarnCenter = new(12f, 34f);
        static readonly Vector2 SiloCenter = new(-10f, 64f);
        static readonly Vector2 MillCenter = new(10f, 92f);
        static readonly Vector2 TowerCenter = new(0f, 116f);
        const float BarnHeight = 13f;
        const float SiloBase = 17f, SiloHeight = 16f;
        const float MillBase = 38f, MillHeight = 24f;
        const float TowerBase = 66f, TowerHeight = 14f;

        static float StationTop(float baseY, float height) => baseY + height + DeckThickness;

        /// <summary>
        /// The playable route as ordered segments. The first and last point of each segment are
        /// standing spots (ground, island or deck); every point between them is a floating prop.
        /// Order: barn stairs, then a bridge and a spiral for each floating station.
        /// </summary>
        public static IReadOnlyList<(string name, Vector3[] points)> CourseRoute()
        {
            var barnTop = StationTop(0f, BarnHeight);

            // Switchback of hay in front of the barn wall (z 26.5), ending on the ridge catwalk.
            var barn = new List<Vector3> { OnGround(5f, 20.8f) };
            var y = 1.3f;
            foreach (var x in new[] { 5f, 9f, 13f, 17f }) { barn.Add(new Vector3(x, y, 23.8f)); y += StepRise; }
            barn.Add(new Vector3(20.5f, y, 22f)); y += StepRise;
            foreach (var x in new[] { 17f, 13f, 9f, 5f }) { barn.Add(new Vector3(x, y, 20.2f)); y += StepRise; }
            barn.Add(new Vector3(7.5f, y, 23.6f));
            barn.Add(new Vector3(10.5f, barnTop, 27.3f));

            var route = new List<(string name, Vector3[] points)> { ("Escada do Celeiro", barn.ToArray()) };
            var exit = new Vector3(12f, barnTop, 40.5f);
            exit = AddFloatingStation(route, "Silo", exit, SiloCenter, SiloBase, SiloHeight, 5.2f, 45f, MillCenter);
            exit = AddFloatingStation(route, "Moinho", exit, MillCenter, MillBase, MillHeight, 4.6f, -45f, TowerCenter);
            AddFloatingStation(route, "Caixa d'Agua", exit, TowerCenter, TowerBase, TowerHeight, 5.4f, 45f, null);
            return route;
        }

        static Vector3 OnCircle(Vector2 center, float radius, float degrees, float y) =>
            new(center.x + Mathf.Sin(degrees * Mathf.Deg2Rad) * radius, y, center.y + Mathf.Cos(degrees * Mathf.Deg2Rad) * radius);

        static float AngleFrom(Vector2 center, Vector3 point) =>
            Mathf.Atan2(point.x - center.x, point.z - center.y) * Mathf.Rad2Deg;

        /// <summary>
        /// Adds a floating bridge onto the building's island and a spiral from the island to its deck.
        /// Returns the deck spot facing the next building, where the following bridge starts.
        /// </summary>
        static Vector3 AddFloatingStation(List<(string name, Vector3[] points)> route, string name, Vector3 start,
            Vector2 center, float baseY, float height, float spiralRadius, float angleStep, Vector2? next)
        {
            var arrival = AngleFrom(center, start);
            var landing = OnCircle(center, IslandLanding, arrival, baseY);
            var jumps = Mathf.CeilToInt(Vector2.Distance(new Vector2(start.x, start.z), new Vector2(landing.x, landing.z)) / BridgeSpacing);
            var bridge = new List<Vector3> { start };
            for (var k = 1; k < jumps; k++) bridge.Add(Vector3.Lerp(start, landing, (float)k / jumps));
            bridge.Add(landing);
            route.Add(($"Ponte {name}", bridge.ToArray()));

            var top = StationTop(baseY, height);
            var spiral = new List<Vector3> { landing };
            var turns = 0;
            for (; baseY + (turns + 1) * StepRise <= top - 0.3f; turns++)
                spiral.Add(OnCircle(center, spiralRadius, arrival + turns * angleStep, baseY + (turns + 1) * StepRise));
            spiral.Add(OnCircle(center, DeckLanding, arrival + (turns - 1) * angleStep, top));
            route.Add(($"Espiral {name}", spiral.ToArray()));

            return next.HasValue
                ? OnCircle(center, DeckLanding, AngleFrom(center, new Vector3(next.Value.x, 0f, next.Value.y)), top)
                : default;
        }

        /// <summary>Throws when a jump is out of reach or a prop would hit the player's head.</summary>
        public static void ValidateRoute(IReadOnlyList<(string name, Vector3[] points)> route)
        {
            var props = new List<Vector3>();
            foreach (var (name, points) in route)
            {
                for (var i = 1; i < points.Length; i++)
                {
                    var rise = points[i].y - points[i - 1].y;
                    var reach = Vector2.Distance(new Vector2(points[i].x, points[i].z), new Vector2(points[i - 1].x, points[i - 1].z));
                    if (rise <= 0f || rise > MaxRise || reach > MaxReach)
                        throw new System.InvalidOperationException(
                            $"{name}: salto {i} fora de alcance (subida {rise:0.00} m, distancia {reach:0.00} m).");
                }
                for (var i = 1; i < points.Length - 1; i++) props.Add(points[i]);
            }

            for (var i = 0; i < props.Count; i++)
                for (var j = i + 1; j < props.Count; j++)
                    if (Vector2.Distance(new Vector2(props[i].x, props[i].z), new Vector2(props[j].x, props[j].z)) < MinPropSpacing
                        && Mathf.Abs(props[i].y - props[j].y) < MinHeadroom)
                        throw new System.InvalidOperationException($"Objetos da rota sobrepostos em {props[i]} e {props[j]}.");
        }

        static void CreateComposedCourse(Dictionary<string, Material> materials)
        {
            new GameObject("FAZENDA_VERTICAL_V11");
            CreateFarmTerrain(materials);
            CreateGroundedFarmRoute();

            var route = CourseRoute();
            ValidateRoute(route);
            var props = new GameObject("Rota - Objetos Flutuantes").transform;
            var propIndex = 0;
            foreach (var (name, points) in route)
            {
                for (var i = 1; i < points.Length - 1; i++)
                {
                    var toNext = points[i + 1] - points[i];
                    var yaw = Mathf.Atan2(toNext.x, toNext.z) * Mathf.Rad2Deg;
                    CreateFarmPropLanding($"{name} {i}", FarmProps + RouteProps[propIndex++ % RouteProps.Length],
                        points[i], Quaternion.Euler(0f, yaw, 0f), RoutePropSize).transform.SetParent(props, true);
                }
            }

            // route[1 + 2k] is the bridge onto island k; its last point is the island landing.
            CreateIsland("Silo", SiloCenter, SiloBase, Last(route[1].points), materials);
            CreateIsland("Moinho", MillCenter, MillBase, Last(route[3].points), materials);
            CreateIsland("Caixa d'Agua", TowerCenter, TowerBase, Last(route[5].points), materials);

            // Each deck carries a beacon over the spot where the next bridge starts.
            CreateStation("Celeiro", new Vector3(BarnCenter.x, StationTop(0f, BarnHeight), BarnCenter.y), new Vector3(5f, DeckThickness, 15f), route[1].points[0], materials);
            CreateStation("Silo", new Vector3(SiloCenter.x, StationTop(SiloBase, SiloHeight), SiloCenter.y), new Vector3(5f, DeckThickness, 5f), route[3].points[0], materials);
            CreateStation("Moinho", new Vector3(MillCenter.x, StationTop(MillBase, MillHeight), MillCenter.y), new Vector3(4.2f, DeckThickness, 4.2f), route[5].points[0], materials);
            CreateStation("Caixa d'Agua", new Vector3(TowerCenter.x, StationTop(TowerBase, TowerHeight), TowerCenter.y), new Vector3(5f, DeckThickness, 5f), null, materials);
        }

        static Vector3 Last(Vector3[] points) => points[points.Length - 1];

        /// <summary>Flat deck on top of a building, with a glowing beacon over the next segment's start.</summary>
        static void CreateStation(string name, Vector3 deckTop, Vector3 deckSize, Vector3? beacon,
            Dictionary<string, Material> materials)
        {
            CreateBlock($"Estacao {name}", deckTop - Vector3.up * deckSize.y * 0.5f, deckSize, materials["Wood"], "Climbable");
            CreateSplitTrigger(deckTop + Vector3.up * 1.5f, name);
            if (!beacon.HasValue) return;

            var post = beacon.Value;
            CreateVisual(PrimitiveType.Cube, $"Poste {name}", null, post + Vector3.up * 1.2f, new Vector3(0.15f, 2.4f, 0.15f), materials["Wood"]);
            var lamp = CreateVisual(PrimitiveType.Sphere, $"Farol {name}", null, post + Vector3.up * 2.6f, Vector3.one * 0.6f, materials["Goal"]);
            var light = lamp.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.55f, 0.18f);
            light.range = 10f;
            light.intensity = 3f;
        }

        /// <summary>
        /// Grass disc with a hanging rock underside. It is solid, so a fall from the spiral lands
        /// back on the island and the player retries from there instead of from the field.
        /// </summary>
        static void CreateIsland(string name, Vector2 center, float top, Vector3 landing, Dictionary<string, Material> materials)
        {
            var root = new GameObject($"Ilha {name}").transform;
            var grass = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            grass.name = "Gramado";
            grass.layer = LayerMask.NameToLayer("Ground");
            grass.transform.SetParent(root, false);
            grass.transform.position = new Vector3(center.x, top - 0.25f, center.y);
            grass.transform.localScale = new Vector3(IslandRadius * 2f, 0.25f, IslandRadius * 2f);
            grass.GetComponent<Renderer>().sharedMaterial = materials["Green"];
            Object.DestroyImmediate(grass.GetComponent<Collider>());
            grass.AddComponent<MeshCollider>();

            CreateVisual(PrimitiveType.Sphere, "Terra", root, Vector3.zero, new Vector3(IslandRadius * 1.95f, 5f, IslandRadius * 1.95f), materials["Soil"])
                .transform.position = new Vector3(center.x, top - 3f, center.y);
            CreateVisual(PrimitiveType.Sphere, "Rocha", root, Vector3.zero, new Vector3(IslandRadius * 1.1f, 6f, IslandRadius * 1.1f), materials["Soil"])
                .transform.position = new Vector3(center.x, top - 6f, center.y);

            // Decor sits on the rim, away from where the bridge arrives, outside the spiral's footprint.
            var arrival = AngleFrom(center, landing);
            var decor = new[] { FarmProps + "Hay.fbx", Nature + "Bush_Common_Flowers.fbx", FarmProps + "Barrel.fbx",
                Nature + "Flower_3_Group.fbx", FarmProps + "Bags.fbx", Nature + "Grass_Common_Tall.fbx", Nature + "Bush_Common.fbx" };
            var heights = new[] { 1.1f, 1.2f, 1.1f, 0.6f, 0.8f, 0.7f, 1f };
            for (var i = 0; i < decor.Length; i++)
            {
                var point = OnCircle(center, 8.1f, arrival + 70f + i * 37f, top);
                CreateModel(decor[i], $"Enfeite {i + 1}", root, root.InverseTransformPoint(point),
                    Quaternion.Euler(0f, i * 53f, 0f), heights[i], true);
            }
        }

        static void CreateFarmTerrain(Dictionary<string, Material> materials)
        {
            CreateBlock("Pasto", new Vector3(0f, -0.6f, 60f), new Vector3(150f, 1.2f, 200f), materials["Green"], "Ground");
            // Dirt surfaces are visual only, a hair above the grass, so they never trip the player.
            CreateVisual(PrimitiveType.Cube, "Estrada de Terra", null, new Vector3(0f, 0.01f, 4f), new Vector3(6f, 0.02f, 34f), materials["Soil"]);
            CreateVisual(PrimitiveType.Cube, "Terreiro do Celeiro", null, new Vector3(11f, 0.01f, 22.5f), new Vector3(24f, 0.02f, 8f), materials["Soil"]);
        }

        static Vector3 OnGround(float x, float z) => new(x, 0f, z);

        /// <summary>Low hops on the ground that lead the player to the barn stairs.</summary>
        static void CreateGroundedFarmRoute()
        {
            var positions = new[] { OnGround(0f, 7f), OnGround(-3f, 12f), OnGround(1f, 16.5f) };
            var models = new[] { "Hay.fbx", "Crate.fbx", "Cart.fbx" };
            for (var i = 0; i < positions.Length; i++)
            {
                var size = new Vector3(i == 2 ? 3.2f : 2.5f, 1.05f + i % 2 * 0.25f, 2.4f);
                CreateFarmPropLanding($"Rota Rural {i + 1}", FarmProps + models[i],
                    positions[i] + Vector3.up * size.y, Quaternion.Euler(0f, i * 31f - 20f, 0f), size);
            }
            CreateSplitTrigger(new Vector3(0f, 1.5f, 6f), "Largada");
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
            FitToBox(CreateStableFarmProp(name, modelPath, center, Quaternion.identity, colliderSize),
                center, rotation, colliderSize);

            return collider;
        }

        /// <summary>Stretches a visual so it fills the gameplay box exactly: no invisible ledges.</summary>
        static void FitToBox(GameObject visual, Vector3 center, Quaternion rotation, Vector3 size)
        {
            var renderers = visual.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return;
            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);

            // A world-aligned parent avoids skewing FBX roots that carry their own axis rotation.
            var pivot = new GameObject(visual.name + " Ajuste").transform;
            pivot.position = bounds.center;
            visual.transform.SetParent(pivot, true);
            pivot.localScale = new Vector3(size.x / Mathf.Max(bounds.size.x, 0.01f),
                size.y / Mathf.Max(bounds.size.y, 0.01f), size.z / Mathf.Max(bounds.size.z, 0.01f));
            pivot.SetPositionAndRotation(center, rotation);
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
            var model = CreateModel(sourceHint, name, null, center - Vector3.up * size.y * 0.5f,
                rotation, size.y, true);
            if (model != null) return model;

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
            CreateBarn(OnGround(BarnCenter.x, BarnCenter.y), materials);
            CreateSilo(new Vector3(SiloCenter.x, SiloBase, SiloCenter.y), materials);
            CreateWindmill(new Vector3(MillCenter.x, MillBase, MillCenter.y), materials);
            CreateModel(FarmBuildings + "WaterTower.fbx", "Caixa d'Agua do Mirante", null,
                new Vector3(TowerCenter.x, TowerBase, TowerCenter.y), Quaternion.Euler(0f, 20f, 0f), TowerHeight, true);
            var towerBody = CreateBlock("Caixa d'Agua Collider", new Vector3(TowerCenter.x, TowerBase + TowerHeight * 0.5f, TowerCenter.y),
                new Vector3(4f, TowerHeight, 4f), materials["Metal"], "CameraBlocker");
            SetVisible(towerBody, false);

            CreateGroundFarm();
        }

        static void CreateBarn(Vector3 groundPosition, Dictionary<string, Material> materials)
        {
            var barn = CreateBlock("Celeiro Collider", groundPosition + Vector3.up * 4.5f, new Vector3(12f, 9f, 15f), materials["BarnRed"], "CameraBlocker");
            var roof = CreateBlock("Celeiro Telhado", groundPosition + Vector3.up * 10.5f, new Vector3(13f, 1.1f, 16f), materials["Roof"], "Climbable");
            roof.transform.rotation = Quaternion.Euler(0f, 0f, 7f);
            var model = CreateModel(FarmBuildings + "BigBarn.fbx", "Celeiro", null, groundPosition, Quaternion.Euler(0f, 90f, 0f), BarnHeight, true);
            if (model != null)
            {
                SetVisible(barn, false);
                SetVisible(roof, false);
            }
        }

        static void CreateSilo(Vector3 basePosition, Dictionary<string, Material> materials)
        {
            // The body keeps the primitive's capsule collider, so the spiral hugs a round wall.
            var silo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            silo.name = "Silo Vertical";
            silo.layer = LayerMask.NameToLayer("CameraBlocker");
            silo.transform.position = basePosition + Vector3.up * SiloHeight * 0.5f;
            silo.transform.localScale = new Vector3(7f, SiloHeight * 0.5f, 7f);
            silo.GetComponent<Renderer>().sharedMaterial = materials["Metal"];
            for (var y = 3f; y < SiloHeight; y += 3.5f)
                CreateVisual(PrimitiveType.Cylinder, $"Cinta {y:0}", null,
                    basePosition + Vector3.up * y, new Vector3(7.12f, 0.06f, 7.12f), materials["Cream"]);
        }

        static void CreateWindmill(Vector3 basePosition, Dictionary<string, Material> materials)
        {
            // Fallback geometry was authored for a 44 m windmill; scale it with the model.
            var s = MillHeight / 44f;
            var tower = CreateBlock("Torre do Moinho", basePosition + Vector3.up * MillHeight * 0.5f, new Vector3(5f * s, MillHeight, 5f * s), materials["Cream"], "CameraBlocker");
            var rotor = new GameObject("Pás do Moinho");
            rotor.transform.position = basePosition + new Vector3(0f, 35f * s, -2.8f * s);
            for (var i = 0; i < 4; i++)
            {
                var blade = CreateVisual(PrimitiveType.Cube, $"Pá {i + 1}", rotor.transform, Quaternion.Euler(0f, 0f, i * 90f) * Vector3.up * 3f * s, new Vector3(0.7f, 6f, 0.25f) * s, materials["Wood"]);
                blade.transform.localRotation = Quaternion.Euler(0f, 0f, i * 90f);
            }
            rotor.AddComponent<WindmillRotor>();
            if (CreateModel(FarmBuildings + "TowerWindmill.fbx", "Moinho Visual", null, basePosition, Quaternion.Euler(0f, 180f, 0f), MillHeight, true) != null)
            {
                SetVisible(tower, false);
                SetVisible(rotor, false);
            }
        }

        // Ground areas kept free of scattered vegetation: road, buildings, stairs, field and pen.
        static readonly Rect[] BusyGround =
        {
            new(-5f, -14f, 10f, 37f),    // road and the first hops
            new(-1f, 17f, 25f, 27f),     // barn, its stairs and yard
            new(19f, 28f, 9f, 18f),      // barn storage
            new(-29f, 19f, 20f, 30f),    // vegetable field
            new(22f, 0f, 22f, 32f),      // animal pen
            new(-26f, -14f, 15f, 15f),   // machine shed
            new(11f, -12f, 10f, 10f),    // tractor
            new(-25f, 10f, 10f, 10f),    // chicken coop
            new(14f, 8f, 7f, 7f),        // well
            new(-25f, 50f, 12f, 10f),    // harvest storehouse
            new(21f, 65f, 10f, 10f),     // small windmill
            new(-31f, 93f, 10f, 10f)     // silo house
        };

        /// <summary>
        /// The field the player starts in, laid out in readable zones: entrance, vegetable field
        /// on the left, animal pen on the right, storage beside the barn, pasture under the islands.
        /// </summary>
        static void CreateGroundFarm()
        {
            var entrance = new GameObject("Zona - Entrada").transform;
            CreateModel(FarmBuildings + "OpenBarn.fbx", "Galpao de Maquinas", entrance, OnGround(-19f, -7f), Quaternion.Euler(0f, 90f, 0f), 6.5f, true);
            CreateModel(TractorModel, "Trator", entrance, OnGround(16f, -7f), Quaternion.Euler(0f, -25f, 0f), 2.7f, true);
            CreateModel(FarmProps + "Cart.fbx", "Carroca", entrance, OnGround(-12f, -3f), Quaternion.Euler(0f, 30f, 0f), 1.6f, true);
            CreateModel(FarmBuildings + "ChickenCoop.fbx", "Galinheiro", entrance, OnGround(-20f, 15f), Quaternion.Euler(0f, 90f, 0f), 3.2f, true);
            CreateModel(FarmBuildings + "Well.fbx", "Poco", entrance, OnGround(17.5f, 11.5f), Quaternion.identity, 2.8f, true);
            PropRow(entrance, "Feno do Galpao", "Hay.fbx", new Vector3(-23f, 0f, -1f), Vector3.right * 2.4f, 3, 1.1f);
            PropRow(entrance, "Barris do Galpao", "Barrel.fbx", new Vector3(-14f, 0f, -12f), Vector3.right * 1.3f, 3, 1.1f);
            for (var i = 0; i < 6; i++)
            {
                // Fence lines flank the entrance road and open where it meets the barn yard.
                CreateModel(FarmBuildings + "Fence.fbx", $"Cerca da Estrada E{i + 1}", entrance, OnGround(-4.5f, -12f + i * 3.1f), Quaternion.Euler(0f, 90f, 0f), 1.2f, true);
                CreateModel(FarmBuildings + "Fence.fbx", $"Cerca da Estrada D{i + 1}", entrance, OnGround(4.5f, -12f + i * 3.1f), Quaternion.Euler(0f, 90f, 0f), 1.2f, true);
            }
            CreateWanderingAnimal("Cachorro", "Pug.fbx", OnGround(6f, 2f), 0.7f, 3f, 0.9f);

            CreateVegetableField(new GameObject("Zona - Horta").transform);
            CreateAnimalPen(new GameObject("Zona - Curral").transform);

            var storage = new GameObject("Zona - Deposito do Celeiro").transform;
            PropRow(storage, "Caixas", "Crate.fbx", new Vector3(21f, 0f, 30f), Vector3.forward * 1.6f, 4, 1.2f);
            PropRow(storage, "Caixas Empilhadas", "Crate.fbx", new Vector3(21f, 1.2f, 30.8f), Vector3.forward * 1.6f, 2, 1.2f);
            PropRow(storage, "Sacos", "Bags.fbx", new Vector3(23.5f, 0f, 31f), Vector3.forward * 2f, 3, 0.9f);
            PropRow(storage, "Barris", "Barrel.fbx", new Vector3(21f, 0f, 38f), Vector3.forward * 1.3f, 4, 1.1f);
            PropRow(storage, "Fardos", "Hay.fbx", new Vector3(24f, 0f, 38f), Vector3.forward * 2.4f, 3, 1.1f);
            CreateModel(FarmProps + "Cart.fbx", "Carroca do Celeiro", storage, OnGround(25.5f, 44f), Quaternion.Euler(0f, -60f, 0f), 1.6f, true);

            var pasture = new GameObject("Zona - Pasto").transform;
            CreateModel(FarmBuildings + "SmallBarn.fbx", "Armazem da Colheita", pasture, OnGround(-19f, 55f), Quaternion.Euler(0f, 90f, 0f), 6f, true);
            CreateModel(FarmBuildings + "Windmill.fbx", "Moinho Pequeno", pasture, OnGround(26f, 70f), Quaternion.Euler(0f, -30f, 0f), 9f, true);
            CreateModel(FarmBuildings + "Silo_House.fbx", "Casa do Silo", pasture, OnGround(-26f, 98f), Quaternion.Euler(0f, 60f, 0f), 8f, true);
            for (var i = 0; i < 6; i++)
                CreateModel(FarmProps + "Hay.fbx", $"Fardo no Pasto {i + 1}", pasture,
                    OnGround(-6f + (i % 3) * 14f, 62f + (i / 3) * 34f), Quaternion.Euler(0f, i * 40f, 0f), 1.2f, true);
            CreateWanderingAnimal("Ovelha do Pasto 1", "Sheep.fbx", OnGround(2f, 75f), 1.2f, 9f, 0.45f);
            CreateWanderingAnimal("Ovelha do Pasto 2", "Sheep.fbx", OnGround(-4f, 84f), 1.1f, 9f, 0.5f);
            CreateWanderingAnimal("Vaca do Pasto", "Cow.fbx", OnGround(18f, 104f), 1.7f, 10f, 0.55f);
            CreateWanderingAnimal("Lhama", "Llama.fbx", OnGround(-14f, 120f), 1.8f, 8f, 0.6f);

            CreateNature(new GameObject("Zona - Natureza").transform);
        }

        /// <summary>Fenced vegetable plot, one crop per row so it reads as a planted field.</summary>
        static void CreateVegetableField(Transform root)
        {
            CreateVisual(PrimitiveType.Cube, "Terra Arada", root, new Vector3(-19f, 0.01f, 34f), new Vector3(16f, 0.02f, 26f),
                AssetDatabase.LoadAssetAtPath<Material>($"{MaterialRoot}/Soil.mat"));
            var crops = new[] { "Wheat_Crop.fbx", "Corn_Crop.fbx", "Carrot_Crop.fbx", "Lettuce_Crop.fbx", "Pumpkin_Crop.fbx", "Wheat_Crop.fbx" };
            var heights = new[] { 1.5f, 1.7f, 0.6f, 0.5f, 0.7f, 1.5f };
            for (var row = 0; row < crops.Length; row++)
                for (var i = 0; i < 11; i++)
                    CreateModel(Crops + crops[row], $"Canteiro {row + 1}-{i + 1}", root,
                        OnGround(-25.5f + row * 2.6f, 23f + i * 2.2f), Quaternion.Euler(0f, i * 23f, 0f), heights[row], true);

            // Gate on the side facing the road (x = -11, z 31-35).
            FenceLine(root, "Cerca da Horta Oeste", new Vector3(-27.5f, 0f, 21f), Vector3.forward, 26f);
            FenceLine(root, "Cerca da Horta Sul", new Vector3(-27.5f, 0f, 21f), Vector3.right, 16.5f);
            FenceLine(root, "Cerca da Horta Norte", new Vector3(-27.5f, 0f, 47f), Vector3.right, 16.5f);
            FenceLine(root, "Cerca da Horta Leste A", new Vector3(-11f, 0f, 21f), Vector3.forward, 10f);
            FenceLine(root, "Cerca da Horta Leste B", new Vector3(-11f, 0f, 35f), Vector3.forward, 12f);
            PropRow(root, "Cestos da Colheita", "Bags.fbx", new Vector3(-9f, 0f, 37f), Vector3.forward * 2f, 2, 0.9f);
        }

        /// <summary>Fenced pen with animals that stay inside it, plus hay and water barrels.</summary>
        static void CreateAnimalPen(Transform root)
        {
            CreateVisual(PrimitiveType.Cube, "Chao do Curral", root, new Vector3(33f, 0.01f, 16f), new Vector3(20f, 0.02f, 30f),
                AssetDatabase.LoadAssetAtPath<Material>($"{MaterialRoot}/Soil.mat"));
            // Gate on the side facing the road (x = 23, z 12-16).
            FenceLine(root, "Cerca do Curral Oeste A", new Vector3(23f, 0f, 1f), Vector3.forward, 11f);
            FenceLine(root, "Cerca do Curral Oeste B", new Vector3(23f, 0f, 16f), Vector3.forward, 15f);
            FenceLine(root, "Cerca do Curral Leste", new Vector3(43f, 0f, 1f), Vector3.forward, 30f);
            FenceLine(root, "Cerca do Curral Sul", new Vector3(23f, 0f, 1f), Vector3.right, 20f);
            FenceLine(root, "Cerca do Curral Norte", new Vector3(23f, 0f, 31f), Vector3.right, 20f);
            PropRow(root, "Feno do Curral", "Hay.fbx", new Vector3(41f, 0f, 4f), Vector3.forward * 2.4f, 4, 1.1f);
            PropRow(root, "Bebedouro", "Barrel.fbx", new Vector3(26f, 0f, 28f), Vector3.right * 1.3f, 3, 1f);

            // Each animal's wander radius keeps it inside the 20 x 30 m pen.
            CreateWanderingAnimal("Vaca 1", "Cow.fbx", OnGround(30f, 8f), 1.7f, 5f, 0.6f);
            CreateWanderingAnimal("Vaca 2", "Cow.fbx", OnGround(36f, 22f), 1.6f, 5f, 0.55f);
            CreateWanderingAnimal("Cavalo", "Horse.fbx", OnGround(34f, 15f), 2.1f, 6f, 0.8f);
            CreateWanderingAnimal("Ovelha 1", "Sheep.fbx", OnGround(28f, 22f), 1.2f, 4f, 0.45f);
            CreateWanderingAnimal("Ovelha 2", "Sheep.fbx", OnGround(30f, 26f), 1.1f, 3.5f, 0.5f);
            CreateWanderingAnimal("Ovelha 3", "Sheep.fbx", OnGround(38f, 27f), 1.15f, 3f, 0.48f);
            CreateWanderingAnimal("Porco 1", "Pig.fbx", OnGround(38f, 7f), 1f, 3.5f, 0.42f);
            CreateWanderingAnimal("Porco 2", "Pig.fbx", OnGround(27f, 6f), 0.9f, 3f, 0.4f);
        }

        /// <summary>Trees frame the field; grass, flowers, bushes and stones fill the free ground.</summary>
        static void CreateNature(Transform root)
        {
            var random = new System.Random(7);
            var trees = new[] { "CommonTree_1.fbx", "CommonTree_2.fbx", "CommonTree_3.fbx", "CommonTree_4.fbx", "CommonTree_5.fbx", "Pine_1.fbx", "Pine_2.fbx", "Pine_3.fbx" };
            var t = 0;
            for (var z = -24f; z <= 150f; z += 9f)
            {
                PlaceNature(root, $"Arvore {++t}", trees[t % trees.Length], new Vector2(-50f - Jitter(random, 4f), z + Jitter(random, 3f)), 7f, 10f, random);
                PlaceNature(root, $"Arvore {++t}", trees[t % trees.Length], new Vector2(50f + Jitter(random, 4f), z + Jitter(random, 3f)), 7f, 10f, random);
            }
            for (var x = -44f; x <= 44f; x += 8f)
                PlaceNature(root, $"Arvore {++t}", trees[t % trees.Length], new Vector2(x + Jitter(random, 2f), -26f - Jitter(random, 3f)), 7f, 10f, random);

            Scatter(root, "Arvore do Pasto", new[] { "CommonTree_2.fbx", "CommonTree_4.fbx", "TwistedTree_1.fbx", "TwistedTree_3.fbx" }, 14, new Rect(-44f, 46f, 88f, 100f), 6f, 9f, random);
            Scatter(root, "Arbusto", new[] { "Bush_Common.fbx", "Bush_Common_Flowers.fbx", "Plant_1_Big.fbx", "Plant_7_Big.fbx" }, 60, new Rect(-46f, -24f, 92f, 170f), 0.8f, 1.6f, random);
            Scatter(root, "Flores", new[] { "Flower_3_Group.fbx", "Flower_4_Group.fbx", "Flower_3_Single.fbx", "Flower_4_Single.fbx", "Clover_1.fbx" }, 120, new Rect(-46f, -24f, 92f, 170f), 0.3f, 0.6f, random);
            Scatter(root, "Capim", new[] { "Grass_Common_Tall.fbx", "Grass_Common_Short.fbx", "Grass_Wispy_Tall.fbx", "Grass_Wispy_Short.fbx", "Fern_1.fbx" }, 220, new Rect(-46f, -24f, 92f, 170f), 0.4f, 0.9f, random);
            Scatter(root, "Pedra", new[] { "Rock_Medium_1.fbx", "Rock_Medium_2.fbx", "Rock_Medium_3.fbx", "Pebble_Round_2.fbx", "Mushroom_Common.fbx" }, 30, new Rect(-46f, -24f, 92f, 170f), 0.3f, 1f, random);

            // Stepping stones from the road to the field gate and to the pen gate.
            for (var i = 0; i < 4; i++)
            {
                PlaceNature(root, $"Caminho da Horta {i + 1}", "RockPath_Round_Small_1.fbx", new Vector2(-5.5f - i * 1.4f, 33f), 0.1f, 0.1f, random);
                PlaceNature(root, $"Caminho do Curral {i + 1}", "RockPath_Round_Small_2.fbx", new Vector2(5.5f + i * 4.3f, 14f), 0.1f, 0.1f, random);
            }
        }

        static float Jitter(System.Random random, float amount) => ((float)random.NextDouble() - 0.5f) * 2f * amount;

        static void PlaceNature(Transform root, string name, string file, Vector2 point, float minHeight, float maxHeight, System.Random random) =>
            CreateModel(Nature + file, name, root, OnGround(point.x, point.y), Quaternion.Euler(0f, random.Next(360), 0f),
                Mathf.Lerp(minHeight, maxHeight, (float)random.NextDouble()), true);

        /// <summary>Places models at random spots in an area, skipping the busy ground. Seeded, so builds are stable.</summary>
        static void Scatter(Transform root, string label, string[] files, int count, Rect area, float minHeight, float maxHeight, System.Random random)
        {
            for (int placed = 0, tries = 0; placed < count && tries < count * 20; tries++)
            {
                var point = new Vector2(Mathf.Lerp(area.xMin, area.xMax, (float)random.NextDouble()), Mathf.Lerp(area.yMin, area.yMax, (float)random.NextDouble()));
                if (BusyGround.Any(zone => zone.Contains(point))) continue;
                PlaceNature(root, $"{label} {placed + 1}", files[placed % files.Length], point, minHeight, maxHeight, random);
                placed++;
            }
        }

        static void PropRow(Transform root, string name, string file, Vector3 start, Vector3 step, int count, float height)
        {
            for (var i = 0; i < count; i++)
                CreateModel(FarmProps + file, $"{name} {i + 1}", root, start + step * i, Quaternion.Euler(0f, i * 17f, 0f), height, true);
        }

        static void FenceLine(Transform root, string name, Vector3 start, Vector3 direction, float length)
        {
            const float segment = 3.1f;
            var rotation = Quaternion.LookRotation(Vector3.Cross(Vector3.up, direction));
            for (var i = 0; i * segment < length; i++)
                CreateModel(FarmBuildings + (i % 2 == 0 ? "Fence.fbx" : "Fence2.fbx"), $"{name} {i + 1}", root,
                    start + direction * (i * segment + segment * 0.5f), rotation, 1.3f, true);
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
                new Vector3(0f, 18f, 34f),
                new Vector3(-8f, 45f, 80f),
                new Vector3(10f, 70f, 120f)
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
            CreateTutorialTrigger(OnGround(5f, 20.8f) + Vector3.up, new Vector3(6f, 3f, 4f), hud, "Ao agarrar uma borda, pressione Espaço para subir");
            CreateTutorialTrigger(new Vector3(BarnCenter.x, StationTop(0f, BarnHeight) + 1.5f, BarnCenter.y), new Vector3(5f, 3f, 14f), hud, "Siga o farol laranja  •  Segure Shift para correr e ganhar impulso");
            CreateTutorialTrigger(new Vector3(0f, 2f, -5f), new Vector3(10f, 4f, 5f), hud, "R retorna ao início sem reiniciar o cronômetro");
        }

        static void CreateGoal(Dictionary<string, Material> materials)
        {
            var goal = new GameObject("Primeiro Dedo");
            goal.transform.position = new Vector3(TowerCenter.x, StationTop(TowerBase, TowerHeight) + 1.1f, TowerCenter.y);
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
            var sourceRotation = instance.transform.localRotation;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = rotation * sourceRotation;
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

#if UNITY_EDITOR
using System.IO;
using System.Linq;
using DevGames.Level;
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
    public static class AnimatedMainMenuBuilder
    {
        const string Root = "Assets/_Game";
        const string ScenePath = Root + "/Scenes/Menus/MainMenu.unity";
        const string CharacterModel = Root + "/ThirdParty/Quaternius/UniversalCharacter/Models/MaleCharacter.fbx";
        const string CharacterAnimations = Root + "/ThirdParty/Quaternius/UniversalAnimations/UAL1_Standard.fbx";
        const string BirdModel = Root + "/ThirdParty/Quaternius/Bird/Ernest.fbx";
        const string FarmBuildings = Root + "/ThirdParty/Quaternius/FarmBuildings/Models/";
        const string FarmProps = Root + "/ThirdParty/Quaternius/FarmProps/";
        const string Nature = Root + "/ThirdParty/Quaternius/StylizedNature/Models/";
        const string IslandModel = Root + "/ThirdParty/FoxDev/FloatingIsle/Models/FloatingIsleModel.fbx";
        const string Whitebird = Root + "/ThirdParty/Whitebird/LowPolyIsland/Models/";
        const string Version = "MENU_CINEMATICO_V2";

        static bool queued;
        static bool rebuilding;

        [MenuItem("DevGames/Rebuild Cinematic Main Menu")]
        public static void Rebuild()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            {
                Debug.LogWarning("Pare o Play Mode antes de reconstruir o menu cinematográfico.");
                return;
            }

            var activeScene = SceneManager.GetActiveScene();
            Scene menuScene = default;
            rebuilding = true;
            try
            {
                menuScene = BuildScene();
            }
            finally
            {
                if (activeScene.IsValid() && activeScene.isLoaded)
                    SceneManager.SetActiveScene(activeScene);
                if (!Application.isBatchMode && menuScene.IsValid() && menuScene.isLoaded)
                    EditorSceneManager.CloseScene(menuScene, true);
                rebuilding = false;
            }
        }

        [InitializeOnLoadMethod]
        static void WatchMenu()
        {
            EditorApplication.projectChanged -= QueueIfOutdated;
            EditorApplication.projectChanged += QueueIfOutdated;
            QueueIfOutdated();
        }

        static void QueueIfOutdated()
        {
            if (rebuilding || queued || Application.isBatchMode || IsCurrent()) return;
            queued = true;
            EditorApplication.update -= RebuildWhenReady;
            EditorApplication.update += RebuildWhenReady;
        }

        static void RebuildWhenReady()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            EditorApplication.update -= RebuildWhenReady;
            queued = false;
            if (!IsCurrent()) Rebuild();
        }

        static bool IsCurrent() => File.Exists(ScenePath) && File.ReadAllText(ScenePath).Contains(Version);

        static Scene BuildScene()
        {
            var mode = Application.isBatchMode ? NewSceneMode.Single : NewSceneMode.Additive;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, mode);
            SceneManager.SetActiveScene(scene);
            CreateWorld(out var camera, out var character, out var animator);
            CreateInterface(out var panel, out var group);
            camera.gameObject.AddComponent<MenuSceneAnimator>().ConfigureCourse(character, animator, camera.transform,
                panel, group, new Vector3(-5.5f, 0.32f, 5.4f), new Vector3(11.3f, 0.32f, 5.4f));
            new GameObject(Version);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("MENU CINEMATOGRÁFICO V2 salvo em MainMenu.unity.");
            return scene;
        }

        static void CreateWorld(out Camera camera, out Transform characterRoot, out Animator animator)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.43f, 0.36f, 0.3f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.26f, 0.36f, 0.39f);
            RenderSettings.fogDensity = 0.009f;

            var sun = new GameObject("Sol do Entardecer").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.75f, 0.5f);
            sun.intensity = 1.45f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(35f, -48f, 0f);

            camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.fieldOfView = 43f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 250f;
            camera.backgroundColor = new Color(0.11f, 0.22f, 0.27f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.transform.position = new Vector3(2.2f, 6.2f, -15.8f);
            camera.transform.rotation = Quaternion.LookRotation(new Vector3(3.2f, 1.2f, 7.2f) - camera.transform.position);
            camera.gameObject.AddComponent<AudioListener>();

            CreateIsland(new Vector3(2.8f, 0f, 7.2f), 19f, "Ilha Principal");
            CreateIsland(new Vector3(14f, 4.5f, 24f), 7f, "Ilha Distante");
            CreateModel(FarmBuildings + "SmallBarn.fbx", "Celeiro da Ilha", null, new Vector3(4.8f, 0.05f, 9.6f),
                Quaternion.Euler(0f, -25f, 0f), 4.2f);
            CreateModel(Nature + "CommonTree_3.fbx", "Árvore da Ilha", null, new Vector3(-1.8f, 0.05f, 10.2f),
                Quaternion.Euler(0f, 24f, 0f), 4.8f);

            var wood = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Art/Materials/Wood.mat");
            CreateBlock("Passarela", new Vector3(2.9f, 0.22f, 5.4f), new Vector3(17.5f, 0.18f, 1.7f), wood);
            CreateModel(FarmProps + "Hay.fbx", "Obstáculo Feno", null, new Vector3(-1.8f, 0.32f, 5.4f),
                Quaternion.Euler(0f, 12f, 0f), 1.05f);
            CreateModel(Whitebird + "Crate.fbx", "Obstáculo Caixa", null, new Vector3(1.75f, 0.32f, 5.4f),
                Quaternion.Euler(0f, -9f, 0f), 1.15f);
            CreateModel(FarmBuildings + "Fence2.fbx", "Obstáculo Cerca", null, new Vector3(4.9f, 0.32f, 5.4f),
                Quaternion.Euler(0f, 0f, 0f), 0.92f);

            CreateModel(Whitebird + "Cloud.fbx", "Nuvem Baixa", null, new Vector3(-2f, -3f, 11f), Quaternion.identity, 2.4f);
            CreateModel(Whitebird + "Cloud.fbx", "Nuvem Distante", null, new Vector3(12f, 2f, 27f), Quaternion.identity, 2.8f);
            CreateBirds();

            characterRoot = new GameObject("Menino do Menu").transform;
            var character = CreateModel(CharacterModel, "Modelo", characterRoot, Vector3.zero, Quaternion.Euler(0f, 90f, 0f), 2.05f);
            animator = character.GetComponent<Animator>();
            if (animator == null) animator = character.AddComponent<Animator>();
            animator.avatar = AssetDatabase.LoadAllAssetsAtPath(CharacterModel).OfType<Avatar>()
                .FirstOrDefault(avatar => avatar.isValid && avatar.isHuman);
            animator.runtimeAnimatorController = CreateCharacterController();
            animator.applyRootMotion = false;
        }

        static void CreateIsland(Vector3 topCenter, float scale, string name)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(IslandModel);
            if (asset == null)
            {
                CreateModel(Whitebird + "Land.fbx", name, null, topCenter + Vector3.down * 2f, Quaternion.identity, scale * 0.35f);
                return;
            }

            var island = Object.Instantiate(asset);
            island.name = name;
            foreach (Transform part in island.transform.Cast<Transform>().ToArray())
                if (part.name != "RockyBase" && part.name != "GeoSphere001") Object.DestroyImmediate(part.gameObject);
            island.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            island.transform.localScale = Vector3.one * scale;
            var grass = island.transform.Find("GeoSphere001").GetComponent<Renderer>().bounds;
            island.transform.position = topCenter - new Vector3(grass.center.x, grass.max.y, grass.center.z);
            foreach (var child in island.GetComponentsInChildren<Transform>(true))
                GameObjectUtility.SetStaticEditorFlags(child.gameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic);
        }

        static void CreateBirds()
        {
            var flock = new GameObject("Pássaros ao Fundo").transform;
            for (var i = 0; i < 6; i++)
            {
                var bird = new GameObject($"Pássaro {i + 1}");
                bird.transform.SetParent(flock);
                bird.AddComponent<BirdFlight>().Configure(new Vector3(4f, 9f + i * 0.45f, 17f + i),
                    6f + i * 0.9f, 8.5f + i * 1.05f, i * 1.07f);
                CreateModel(BirdModel, "Modelo", bird.transform, Vector3.zero, Quaternion.Euler(0f, 90f, 0f), 0.5f + i * 0.05f);
            }
        }

        static AnimatorController CreateCharacterController()
        {
            const string path = Root + "/Settings/MenuCharacterV2.controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            var machine = controller.layers[0].stateMachine;
            var run = State(machine, "Run", "Armature|Sprint_Loop");
            State(machine, "JumpStart", "Armature|Jump_Start");
            State(machine, "JumpLoop", "Armature|Jump_Loop");
            State(machine, "JumpLand", "Armature|Jump_Land");
            State(machine, "Fall", "Armature|Jump_Loop");
            machine.defaultState = run;
            return controller;
        }

        static AnimatorState State(AnimatorStateMachine machine, string name, string clipName)
        {
            var state = machine.states.FirstOrDefault(item => item.state.name == name).state ?? machine.AddState(name);
            state.motion = AssetDatabase.LoadAllAssetsAtPath(CharacterAnimations).OfType<AnimationClip>()
                .FirstOrDefault(clip => clip.name == clipName);
            return state;
        }

        static void CreateInterface(out RectTransform panel, out CanvasGroup group)
        {
            var canvasObject = new GameObject("Main Menu Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            var controller = canvasObject.AddComponent<MenuController>();

            CreateImage(canvas.transform, "Vignette", new Vector2(0f, 0f), new Vector2(0.43f, 1f),
                new Color(0.035f, 0.04f, 0.038f, 0.94f));
            CreateImage(canvas.transform, "Vignette Fade", new Vector2(0.43f, 0f), new Vector2(0.52f, 1f),
                new Color(0.035f, 0.04f, 0.038f, 0.48f));

            var panelObject = new GameObject("Conteúdo do Menu", typeof(RectTransform), typeof(CanvasGroup));
            panelObject.transform.SetParent(canvas.transform, false);
            panel = (RectTransform)panelObject.transform;
            panel.anchorMin = new Vector2(0.055f, 0.08f);
            panel.anchorMax = new Vector2(0.39f, 0.92f);
            panel.offsetMin = panel.offsetMax = Vector2.zero;
            group = panelObject.GetComponent<CanvasGroup>();

            CreateImage(panel, "Linha Dourada", new Vector2(0f, 0.91f), new Vector2(0.012f, 0.985f),
                new Color(0.95f, 0.62f, 0.24f, 1f));
            CreateText(panel, "Capítulo", "CAPÍTULO I  •  A FAZENDA SUSPENSA", 17, new Vector2(0.04f, 0.88f),
                new Vector2(0.96f, 0.98f), TextAnchor.MiddleLeft, new Color(0.95f, 0.68f, 0.32f), FontStyle.Bold);
            var title = CreateText(panel, "Título", "O MENINO\nLOIRO", 70, new Vector2(0f, 0.61f), new Vector2(1f, 0.9f),
                TextAnchor.MiddleLeft, new Color(1f, 0.95f, 0.84f), FontStyle.Bold);
            var shadow = title.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.12f, 0.05f, 0.03f, 0.9f);
            shadow.effectDistance = new Vector2(4f, -4f);
            CreateText(panel, "Complemento", "e os dedos perdidos", 29, new Vector2(0.01f, 0.54f), new Vector2(0.96f, 0.64f),
                TextAnchor.MiddleLeft, new Color(0.86f, 0.73f, 0.52f), FontStyle.Italic);
            CreateText(panel, "Sinopse", "Suba pelas ilhas, vença os obstáculos\ne descubra o segredo acima das nuvens.", 21,
                new Vector2(0.01f, 0.39f), new Vector2(0.96f, 0.54f), TextAnchor.MiddleLeft,
                new Color(0.84f, 0.81f, 0.72f), FontStyle.Normal);

            var play = CreateButton(panel, "Play", "COMEÇAR A AVENTURA", new Vector2(0.01f, 0.22f), new Vector2(0.94f, 0.33f), true);
            var quit = CreateButton(panel, "Quit", "SAIR", new Vector2(0.01f, 0.105f), new Vector2(0.94f, 0.19f), false);
            UnityEventTools.AddPersistentListener(play.onClick, controller.LoadLevelSelect);
            UnityEventTools.AddPersistentListener(quit.onClick, controller.Quit);
            CreateText(panel, "Ajuda", "ENTER / A  selecionar     •     ESC / B  voltar", 14, new Vector2(0.01f, 0f),
                new Vector2(0.96f, 0.075f), TextAnchor.MiddleLeft, new Color(0.62f, 0.6f, 0.54f), FontStyle.Normal);

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule)).GetComponent<EventSystem>();
            eventSystem.firstSelectedGameObject = play.gameObject;
        }

        static Image CreateImage(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        static Text CreateText(Transform parent, string name, string value, int size, Vector2 anchorMin, Vector2 anchorMax,
            TextAnchor alignment, Color color, FontStyle style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        static Button CreateButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, bool primary)
        {
            var image = CreateImage(parent, name, anchorMin, anchorMax,
                primary ? new Color(0.48f, 0.14f, 0.09f, 0.98f) : new Color(0.13f, 0.14f, 0.13f, 0.92f));
            var button = image.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = image.color;
            colors.highlightedColor = primary ? new Color(0.62f, 0.22f, 0.12f) : new Color(0.24f, 0.23f, 0.19f);
            colors.pressedColor = new Color(0.32f, 0.08f, 0.05f);
            colors.selectedColor = colors.highlightedColor;
            colors.fadeDuration = 0.18f;
            button.colors = colors;
            if (primary) CreateImage(image.transform, "Acento", new Vector2(0f, 0f), new Vector2(0.012f, 1f), new Color(0.97f, 0.68f, 0.28f));
            CreateText(image.transform, "Label", label, 20, new Vector2(0.07f, 0f), new Vector2(0.86f, 1f),
                TextAnchor.MiddleLeft, Color.white, FontStyle.Bold);
            CreateText(image.transform, "Seta", "›", 34, new Vector2(0.86f, 0f), new Vector2(0.96f, 1f),
                TextAnchor.MiddleCenter, new Color(0.96f, 0.72f, 0.38f), FontStyle.Normal);
            return button;
        }

        static GameObject CreateBlock(string name, Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetPositionAndRotation(position, Quaternion.identity);
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        static GameObject CreateModel(string path, string name, Transform parent, Vector3 bottomCenter, Quaternion rotation, float targetHeight)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null)
            {
                Debug.LogWarning($"Modelo do menu não encontrado: {path}");
                return null;
            }

            var instance = PrefabUtility.InstantiatePrefab(asset) as GameObject;
            if (instance == null) return null;
            instance.name = name;
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = rotation * instance.transform.localRotation;
            instance.transform.localScale = new Vector3(Mathf.Abs(instance.transform.localScale.x),
                Mathf.Abs(instance.transform.localScale.y), Mathf.Abs(instance.transform.localScale.z));
            foreach (var collider in instance.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(collider);
            var renderers = instance.GetComponentsInChildren<Renderer>(true);
            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds);
            if (bounds.size.y > 0.001f) instance.transform.localScale *= targetHeight / bounds.size.y;
            bounds = renderers[0].bounds;
            foreach (var renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds);
            var target = parent == null ? bottomCenter : parent.TransformPoint(bottomCenter);
            instance.transform.position += target - new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            return instance;
        }
    }
}
#endif

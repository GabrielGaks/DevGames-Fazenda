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
        static readonly Vector3 CourseStart = new(6f, 0.03f, 5.4f);

        static bool queued;
        static bool rebuilding;

        /// <summary>Menu DevGames > Rebuild Cinematic Main Menu: gera a cena do menu principal animado.</summary>
        /// <remarks>Cria o mundo 3D, o personagem com o percurso de parkour, a interface e salva a cena.</remarks>
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

        /// <summary>Ao abrir o editor, passa a verificar se o menu está desatualizado.</summary>
        [InitializeOnLoadMethod]
        static void WatchMenu()
        {
            EditorApplication.projectChanged -= QueueIfOutdated;
            EditorApplication.projectChanged += QueueIfOutdated;
            QueueIfOutdated();
        }

        /// <summary>Agenda a reconstrução se a cena não tiver o marcador da versão atual.</summary>
        static void QueueIfOutdated()
        {
            if (rebuilding || queued || Application.isBatchMode || IsCurrent()) return;
            queued = true;
            EditorApplication.update -= RebuildWhenReady;
            EditorApplication.update += RebuildWhenReady;
        }

        /// <summary>Reconstrói o menu quando o editor não estiver compilando nem em Play Mode.</summary>
        static void RebuildWhenReady()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            EditorApplication.update -= RebuildWhenReady;
            queued = false;
            if (!IsCurrent()) Rebuild();
        }

        static bool IsCurrent() => File.Exists(ScenePath) && File.ReadAllText(ScenePath).Contains(Version);

        /// <summary>Cria a cena do menu do zero.</summary>
        /// <returns>A cena criada.</returns>
        static Scene BuildScene()
        {
            var mode = Application.isBatchMode ? NewSceneMode.Single : NewSceneMode.Additive;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, mode);
            SceneManager.SetActiveScene(scene);
            CreateWorld(out var camera, out var character, out var animator, out var coursePoints);
            CreateInterface(out var panel, out var group);
            camera.gameObject.AddComponent<MenuSceneAnimator>().ConfigureCourse(character, animator, camera.transform,
                panel, group, coursePoints);
            new GameObject(Version);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("MENU CINEMATOGRÁFICO V2 salvo em MainMenu.unity.");
            return scene;
        }

        /// <summary>Monta o cenário 3D do menu: chão, ilhas, aves, câmera e personagem.</summary>
        /// <param name="camera">Saída: câmera do menu.</param>
        /// <param name="characterRoot">Saída: raiz do personagem.</param>
        /// <param name="animator">Saída: Animator do personagem.</param>
        /// <param name="coursePoints">Saída: pontos de pouso do parkour.</param>
        static void CreateWorld(out Camera camera, out Transform characterRoot, out Animator animator,
            out Vector3[] coursePoints)
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
            camera.fieldOfView = 48f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 250f;
            camera.backgroundColor = new Color(0.11f, 0.22f, 0.27f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.transform.position = new Vector3(5.2f, 7.4f, -17.5f);
            camera.transform.rotation = Quaternion.LookRotation(new Vector3(6.2f, 5f, 7.2f) - camera.transform.position);
            camera.gameObject.AddComponent<AudioListener>();

            CreateIsland(new Vector3(6.4f, 0f, 7.2f), 19f, "Ilha Principal");
            CreateIsland(new Vector3(14f, 4.5f, 24f), 7f, "Ilha Distante");
            CreateModel(FarmBuildings + "SmallBarn.fbx", "Celeiro da Ilha", null, new Vector3(3.8f, 0.05f, 10.5f),
                Quaternion.Euler(0f, -25f, 0f), 3.6f);
            CreateModel(Nature + "CommonTree_3.fbx", "Árvore da Ilha", null, new Vector3(0.5f, 0.05f, 11.5f),
                Quaternion.Euler(0f, 24f, 0f), 4.8f);

            coursePoints = CreateParkourCourse();
            var menuCamera = camera;
            Debug.Assert(coursePoints.Skip(14).Take(3)
                .All(point => menuCamera.WorldToViewportPoint(point - Vector3.up * 1.3f).y > 1f));

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

        static Vector3[] CreateParkourCourse()
        {
            var hay = CreateModel(FarmProps + "Hay.fbx", "Parkour 1 - Feno", null,
                new Vector3(8.5f, 0.35f, 3.8f), Quaternion.Euler(0f, 12f, 0f), 1.25f);
            var crate = CreateModel(FarmProps + "Crate.fbx", "Parkour 2 - Caixa", null,
                new Vector3(12f, 1.55f, 3.6f), Quaternion.Euler(0f, -10f, 0f), 1.25f);
            var barrel = CreateModel(FarmProps + "Barrel.fbx", "Parkour 3 - Barril", null,
                new Vector3(15f, 2.7f, 5.5f), Quaternion.Euler(0f, 18f, 0f), 1.3f);
            var bags = CreateModel(FarmProps + "Bags.fbx", "Parkour 4 - Sacos", null,
                new Vector3(15f, 4f, 8.8f), Quaternion.Euler(0f, -14f, 0f), 1.2f);
            var cart = CreateModel(FarmProps + "Cart.fbx", "Parkour 5 - Carroca", null,
                new Vector3(12.3f, 5f, 10.5f), Quaternion.Euler(0f, 36f, 0f), 1.4f);
            var upperHay = CreateModel(FarmProps + "Hay.fbx", "Parkour 6 - Feno Alto", null,
                new Vector3(9f, 6.35f, 9.2f), Quaternion.Euler(0f, -18f, 0f), 1.25f);
            var upperCrate = CreateModel(FarmProps + "Crate.fbx", "Parkour 7 - Caixa Alta", null,
                new Vector3(8f, 7.55f, 6.3f), Quaternion.Euler(0f, 10f, 0f), 1.25f);
            var upperBarrel = CreateModel(FarmProps + "Barrel.fbx", "Parkour 8 - Barril Alto", null,
                new Vector3(10.2f, 8.7f, 4.5f), Quaternion.Euler(0f, -16f, 0f), 1.3f);
            var upperBags = CreateModel(FarmProps + "Bags.fbx", "Parkour 9 - Sacos Altos", null,
                new Vector3(13.2f, 10f, 4.2f), Quaternion.Euler(0f, 12f, 0f), 1.2f);
            var finalHay = CreateModel(FarmProps + "Hay.fbx", "Parkour 10 - Feno do Topo", null,
                new Vector3(15.2f, 11.15f, 6.5f), Quaternion.Euler(0f, -8f, 0f), 1.25f);
            var skyCrate = CreateModel(FarmProps + "Crate.fbx", "Parkour 11 - Caixa no Céu", null,
                new Vector3(15.6f, 12.35f, 9.8f), Quaternion.Euler(0f, 14f, 0f), 1.25f);
            var skyBarrel = CreateModel(FarmProps + "Barrel.fbx", "Parkour 12 - Barril no Céu", null,
                new Vector3(13.5f, 13.5f, 11f), Quaternion.Euler(0f, -20f, 0f), 1.3f);
            var skyBags = CreateModel(FarmProps + "Bags.fbx", "Parkour 13 - Sacos no Céu", null,
                new Vector3(10.5f, 14.8f, 10.2f), Quaternion.Euler(0f, 8f, 0f), 1.2f);
            var hiddenHay = CreateModel(FarmProps + "Hay.fbx", "Parkour 14 - Feno Fora da Tela", null,
                new Vector3(8.5f, 15.95f, 7.5f), Quaternion.Euler(0f, -12f, 0f), 1.25f);
            var hiddenCrate = CreateModel(FarmProps + "Crate.fbx", "Parkour 15 - Caixa Fora da Tela", null,
                new Vector3(9.2f, 17.15f, 4.8f), Quaternion.Euler(0f, 18f, 0f), 1.25f);
            var hiddenBarrel = CreateModel(FarmProps + "Barrel.fbx", "Parkour 16 - Barril Fora da Tela", null,
                new Vector3(12.2f, 18.3f, 3.7f), Quaternion.Euler(0f, -10f, 0f), 1.3f);

            return new[]
            {
                CourseStart,
                TopPoint(hay, new Vector3(8.5f, 1.6f, 3.8f)),
                TopPoint(crate, new Vector3(12f, 2.8f, 3.6f)),
                TopPoint(barrel, new Vector3(15f, 4f, 5.5f)),
                TopPoint(bags, new Vector3(15f, 5.2f, 8.8f)),
                TopPoint(cart, new Vector3(12.3f, 6.4f, 10.5f)),
                TopPoint(upperHay, new Vector3(9f, 7.6f, 9.2f)),
                TopPoint(upperCrate, new Vector3(8f, 8.8f, 6.3f)),
                TopPoint(upperBarrel, new Vector3(10.2f, 10f, 4.5f)),
                TopPoint(upperBags, new Vector3(13.2f, 11.2f, 4.2f)),
                TopPoint(finalHay, new Vector3(15.2f, 12.4f, 6.5f)),
                TopPoint(skyCrate, new Vector3(15.6f, 13.6f, 9.8f)),
                TopPoint(skyBarrel, new Vector3(13.5f, 14.8f, 11f)),
                TopPoint(skyBags, new Vector3(10.5f, 16f, 10.2f)),
                TopPoint(hiddenHay, new Vector3(8.5f, 17.2f, 7.5f)),
                TopPoint(hiddenCrate, new Vector3(9.2f, 18.4f, 4.8f)),
                TopPoint(hiddenBarrel, new Vector3(12.2f, 19.6f, 3.7f)),
                new Vector3(15.2f, 20.8f, 5.3f)
            };
        }

        static Vector3 TopPoint(GameObject model, Vector3 fallback)
        {
            if (model == null) return fallback;
            var bounds = model.GetComponentsInChildren<Renderer>(true)
                .Select(renderer => renderer.bounds).Aggregate((result, next) => { result.Encapsulate(next); return result; });
            return new Vector3(bounds.center.x, bounds.max.y + 0.02f, bounds.center.z);
        }

        /// <summary>Cria uma pequena ilha flutuante.</summary>
        /// <param name="topCenter">Centro do topo.</param>
        /// <param name="scale">Escala.</param>
        /// <param name="name">Nome do objeto na cena.</param>
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

        /// <summary>Adiciona aves voando no fundo do menu.</summary>
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

        /// <summary>Cria o Animator Controller do personagem do menu (corrida, pulo, queda, aterrissagem).</summary>
        /// <returns>O controller salvo.</returns>
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

        /// <summary>Adiciona um estado com um clipe de animação ao Animator.</summary>
        /// <param name="machine">Máquina de estados.</param>
        /// <param name="name">Nome do estado.</param>
        /// <param name="clipName">Nome do clipe.</param>
        /// <returns>O estado criado.</returns>
        static AnimatorState State(AnimatorStateMachine machine, string name, string clipName)
        {
            var state = machine.states.FirstOrDefault(item => item.state.name == name).state ?? machine.AddState(name);
            state.motion = AssetDatabase.LoadAllAssetsAtPath(CharacterAnimations).OfType<AnimationClip>()
                .FirstOrDefault(clip => clip.name == clipName);
            return state;
        }

        /// <summary>Cria o Canvas do menu com título e botões Jogar e Sair.</summary>
        /// <param name="panel">Saída: painel dos botões.</param>
        /// <param name="group">Saída: CanvasGroup para o fade.</param>
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

            var choices = CreateImage(panel, "Seletor de Opções", new Vector2(0f, 0.075f), new Vector2(0.96f, 0.355f),
                new Color(0.055f, 0.06f, 0.055f, 0.96f));
            var outline = choices.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.64f, 0.46f, 0.24f, 0.42f);
            outline.effectDistance = new Vector2(1f, -1f);
            CreateImage(choices.transform, "Linha Superior", new Vector2(0f, 0.975f), Vector2.one,
                new Color(0.96f, 0.66f, 0.28f, 0.9f));
            CreateText(choices.transform, "Rótulo", "ESCOLHA UMA AÇÃO", 13, new Vector2(0.045f, 0.79f),
                new Vector2(0.96f, 0.96f), TextAnchor.MiddleLeft, new Color(0.78f, 0.68f, 0.52f), FontStyle.Bold);

            var play = CreateButton(choices.transform, "Play", "01", "COMEÇAR A AVENTURA",
                "Avançar para a seleção de fases", new Vector2(0.035f, 0.41f), new Vector2(0.965f, 0.78f), true);
            var quit = CreateButton(choices.transform, "Quit", "02", "SAIR",
                "Encerrar a jornada", new Vector2(0.035f, 0.06f), new Vector2(0.965f, 0.36f), false);
            UnityEventTools.AddPersistentListener(play.onClick, controller.LoadLevelSelect);
            UnityEventTools.AddPersistentListener(quit.onClick, controller.Quit);
            CreateText(panel, "Ajuda", "↑ ↓  NAVEGAR     ENTER / A  CONFIRMAR     ESC / B  VOLTAR", 12, new Vector2(0f, 0f),
                new Vector2(0.96f, 0.06f), TextAnchor.MiddleLeft, new Color(0.58f, 0.57f, 0.52f), FontStyle.Normal);

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule)).GetComponent<EventSystem>();
            eventSystem.firstSelectedGameObject = play.gameObject;
        }

        /// <summary>Cria uma imagem de UI.</summary>
        /// <param name="parent">Transform pai na hierarquia.</param>
        /// <param name="name">Nome do objeto na cena.</param>
        /// <param name="anchorMin">Canto inferior esquerdo (0-1).</param>
        /// <param name="anchorMax">Canto superior direito (0-1).</param>
        /// <param name="color">Cor.</param>
        /// <returns>A Image criada.</returns>
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

        /// <summary>Cria um texto de UI.</summary>
        /// <param name="parent">Transform pai na hierarquia.</param>
        /// <param name="name">Nome do objeto na cena.</param>
        /// <param name="value">Texto.</param>
        /// <param name="size">Tamanho da fonte.</param>
        /// <param name="anchorMin">Canto inferior esquerdo (0-1).</param>
        /// <param name="anchorMax">Canto superior direito (0-1).</param>
        /// <param name="alignment">Alinhamento.</param>
        /// <param name="color">Cor.</param>
        /// <param name="style">Estilo da fonte.</param>
        /// <returns>O Text criado.</returns>
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

        /// <summary>Cria um botão do menu com número, título e subtítulo.</summary>
        /// <param name="parent">Transform pai na hierarquia.</param>
        /// <param name="name">Nome do objeto na cena.</param>
        /// <param name="index">Número exibido (01, 02).</param>
        /// <param name="label">Título.</param>
        /// <param name="subtitle">Subtítulo.</param>
        /// <param name="anchorMin">Canto inferior esquerdo (0-1).</param>
        /// <param name="anchorMax">Canto superior direito (0-1).</param>
        /// <param name="primary">True para o botão de destaque.</param>
        /// <returns>O Button criado.</returns>
        static Button CreateButton(Transform parent, string name, string index, string label, string subtitle,
            Vector2 anchorMin, Vector2 anchorMax, bool primary)
        {
            var image = CreateImage(parent, name, anchorMin, anchorMax,
                primary ? new Color(0.42f, 0.105f, 0.065f, 1f) : new Color(0.095f, 0.105f, 0.095f, 1f));
            var button = image.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = image.color;
            colors.highlightedColor = primary ? new Color(0.58f, 0.18f, 0.09f) : new Color(0.2f, 0.2f, 0.16f);
            colors.pressedColor = new Color(0.32f, 0.08f, 0.05f);
            colors.selectedColor = colors.highlightedColor;
            colors.fadeDuration = 0.18f;
            button.colors = colors;
            CreateImage(image.transform, "Acento", Vector2.zero, new Vector2(0.012f, 1f),
                primary ? new Color(0.98f, 0.68f, 0.25f) : new Color(0.43f, 0.39f, 0.3f));
            CreateText(image.transform, "Índice", index, 14, new Vector2(0.04f, 0.08f), new Vector2(0.15f, 0.92f),
                TextAnchor.MiddleCenter, primary ? new Color(1f, 0.76f, 0.38f) : new Color(0.62f, 0.59f, 0.5f), FontStyle.Bold);
            CreateImage(image.transform, "Divisor", new Vector2(0.17f, 0.2f), new Vector2(0.173f, 0.8f),
                new Color(0.76f, 0.64f, 0.43f, 0.42f));
            CreateText(image.transform, "Label", label, 18, new Vector2(0.21f, 0.43f), new Vector2(0.82f, 0.9f),
                TextAnchor.MiddleLeft, new Color(1f, 0.96f, 0.87f), FontStyle.Bold);
            CreateText(image.transform, "Descrição", subtitle, 12, new Vector2(0.21f, 0.08f), new Vector2(0.82f, 0.47f),
                TextAnchor.MiddleLeft, new Color(0.72f, 0.69f, 0.61f), FontStyle.Normal);
            CreateText(image.transform, "Seta", "›", 32, new Vector2(0.86f, 0f), new Vector2(0.96f, 1f),
                TextAnchor.MiddleCenter, new Color(0.96f, 0.72f, 0.38f), FontStyle.Normal);
            return button;
        }

        /// <summary>Instancia um FBX numa altura e posição.</summary>
        /// <param name="path">Caminho do FBX.</param>
        /// <param name="name">Nome do objeto na cena.</param>
        /// <param name="parent">Transform pai na hierarquia.</param>
        /// <param name="bottomCenter">Ponto da base.</param>
        /// <param name="rotation">Rotação.</param>
        /// <param name="targetHeight">Altura final.</param>
        /// <returns>O modelo, ou null.</returns>
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

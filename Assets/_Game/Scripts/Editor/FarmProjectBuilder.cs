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
        const string GroundTextureRoot = Root + "/Art/Textures/Ground";
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
        const string IslandPack = Root + "/ThirdParty/Whitebird/LowPolyIsland/Models/";
        const string FoxIsle = Root + "/ThirdParty/FoxDev/FloatingIsle/Models/FloatingIsleModel.fbx";
        const string FoxIsleRoot = Root + "/ThirdParty/FoxDev/FloatingIsle";
        const string CozyFarm = Root + "/ThirdParty/Styloo/CozyFarm/Models/";
        const string SurvivalKit = Root + "/ThirdParty/Kenney/SurvivalKit/Models/";
        const string AnimatedMenuVersion = "MENU_ANIMADO_V1";

        /// <summary>Forces the Editor Play button to follow the same Bootstrap-first flow as a build.</summary>
        [InitializeOnLoadMethod]
        static void ConfigurePlayModeStartScene()
        {
            EditorApplication.delayCall += SetBootstrapAsPlayModeStartScene;
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredEditMode) SetBootstrapAsPlayModeStartScene();
            };
        }

        /// <summary>Selects Bootstrap as the scene launched by the Editor Play button.</summary>
        static void SetBootstrapAsPlayModeStartScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var bootstrap = AssetDatabase.LoadAssetAtPath<SceneAsset>($"{SceneRoot}/Bootstrap.unity");
            if (bootstrap != null && EditorSceneManager.playModeStartScene != bootstrap)
                EditorSceneManager.playModeStartScene = bootstrap;
        }

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
            CreateFoxIsleMaterials();
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

        [MenuItem("DevGames/Rebuild Animated Main Menu")]
        public static void RebuildAnimatedMainMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("Pare o Play Mode antes de reconstruir o menu principal.");
                return;
            }

            var materials = new Dictionary<string, Material>();
            foreach (var name in new[] { "BarnRed", "Wood", "Straw", "Green", "Roof", "Cream" })
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialRoot}/{name}.mat");
                if (material == null)
                {
                    Debug.LogError($"Material do menu não encontrado: {name}. Execute DevGames > Build Farm Project.");
                    return;
                }
                materials[name] = material;
            }

            CreateMainMenuScene(materials);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("MENU ANIMADO V1 salvo em MainMenu.unity.");
        }

        [InitializeOnLoadMethod]
        static void ApplyAnimatedMenuOnce()
        {
            if (Application.isBatchMode) return;
            var menuPath = $"{SceneRoot}/Menus/MainMenu.unity";
            if (System.IO.File.Exists(menuPath) && System.IO.File.ReadAllText(menuPath).Contains(AnimatedMenuVersion)) return;
            EditorApplication.delayCall += () =>
            {
                var activeScene = SceneManager.GetActiveScene();
                if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || activeScene.isDirty)
                {
                    Debug.LogWarning("Menu animado pendente: salve a cena e use DevGames > Rebuild Animated Main Menu.");
                    return;
                }

                var previousPath = activeScene.path;
                RebuildAnimatedMainMenu();
                if (!string.IsNullOrEmpty(previousPath) && previousPath != menuPath)
                    EditorSceneManager.OpenScene(previousPath, OpenSceneMode.Single);
            };
        }

        /// <summary>Applies the organic farm revision and emits an unmistakable confirmation.</summary>
        [MenuItem("DevGames/APLICAR FAZENDA VERTICAL V30")]
        public static void ApplyVisualFixV6()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("Pare o Play Mode antes de aplicar a FAZENDA VERTICAL V30.");
                return;
            }

            BuildAll();
            Debug.Log("FAZENDA VERTICAL V30 salva em Farm.unity - abra Bootstrap e pressione Play.");
        }

        [InitializeOnLoadMethod]
        static void ApplyOrganicFarmOnce()
        {
            if (Application.isBatchMode) return;
            var farmScene = $"{SceneRoot}/Levels/Farm.unity";
            if (System.IO.File.Exists(farmScene) && System.IO.File.ReadAllText(farmScene).Contains("FAZENDA_VERTICAL_V30")) return;
            EditorApplication.update -= ApplyOrganicFarmWhenReady;
            EditorApplication.update += ApplyOrganicFarmWhenReady;
        }

        /// <summary>Waits for compilation to finish before rebuilding the generated scenes.</summary>
        static void ApplyOrganicFarmWhenReady()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            EditorApplication.update -= ApplyOrganicFarmWhenReady;
            ApplyVisualFixV6();
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
            CreateMaterial("TractorRed", new Color(0.58f, 0.08f, 0.035f), result);
            CreateMaterial("TireDark", new Color(0.035f, 0.04f, 0.045f), result);
            CreateMaterial("SeatLeather", new Color(0.16f, 0.075f, 0.035f), result);
            CreateMaterial("Goal", new Color(1f, 0.55f, 0.12f), result, true);
            ConfigureGroundMaterial(result["Green"], "grass_ground", new Color(0.74f, 0.86f, 0.70f), 7f);
            ConfigureGroundMaterial(result["Soil"], "dirt", new Color(0.82f, 0.72f, 0.58f), 5f);
            return result;
        }

        /// <summary>Applies a repeating CC0 color and normal texture to a farm ground material.</summary>
        /// <param name="material">URP material used by the generated ground meshes.</param>
        /// <param name="textureName">Poly Haven asset prefix shared by the downloaded maps.</param>
        /// <param name="tint">Subtle color grading that keeps the texture inside the farm palette.</param>
        /// <param name="tiling">Number of repetitions across a Unity primitive UV tile.</param>
        static void ConfigureGroundMaterial(Material material, string textureName, Color tint, float tiling)
        {
            var colorPath = $"{GroundTextureRoot}/{textureName}_diff_1k.jpg";
            var normalPath = $"{GroundTextureRoot}/{textureName}_nor_gl_1k.jpg";
            ConfigureGroundTextureImport(colorPath, false);
            ConfigureGroundTextureImport(normalPath, true);
            var colorMap = AssetDatabase.LoadAssetAtPath<Texture2D>(colorPath);
            var normalMap = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
            if (material == null || colorMap == null) return;

            material.color = tint;
            material.SetTexture("_BaseMap", colorMap);
            material.SetTexture("_MainTex", colorMap);
            material.SetTextureScale("_BaseMap", Vector2.one * tiling);
            material.SetTextureScale("_MainTex", Vector2.one * tiling);
            material.SetFloat("_Smoothness", 0.12f);
            if (normalMap != null)
            {
                material.SetTexture("_BumpMap", normalMap);
                material.SetTextureScale("_BumpMap", Vector2.one * tiling);
                material.SetFloat("_BumpScale", 0.65f);
                material.EnableKeyword("_NORMALMAP");
            }
            EditorUtility.SetDirty(material);
        }

        /// <summary>Configures a ground image for seamless repetition and correct normal-map decoding.</summary>
        /// <param name="path">Project-relative path of the texture.</param>
        /// <param name="normalMap">True when the image contains tangent-space normals.</param>
        static void ConfigureGroundTextureImport(string path, bool normalMap)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;
            var desiredType = normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
            if (importer.textureType == desiredType && importer.wrapMode == TextureWrapMode.Repeat && importer.maxTextureSize == 1024) return;
            importer.textureType = desiredType;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.maxTextureSize = 1024;
            importer.SaveAndReimport();
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

        /// <summary>Creates URP materials from the official hand-painted Fox Dev texture set.</summary>
        static void CreateFoxIsleMaterials()
        {
            if (!AssetDatabase.LoadAssetAtPath<GameObject>(FoxIsle)) return;
            foreach (var name in new[] { "Chimney", "Decor", "Fence", "Grass", "House", "Roof", "Stairs", "Stone", "Windmill" })
            {
                var baseMap = LoadFoxTexture(name, "BaseMap", false);
                var normal = LoadFoxTexture(name, "Normal", true);
                var mask = LoadFoxTexture(name, "MaskMap", false, true);
                if (baseMap == null) continue;

                var path = $"{FoxIsleRoot}/Materials/{name}.mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                {
                    material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
                    AssetDatabase.CreateAsset(material, path);
                }
                material.SetTexture("_BaseMap", baseMap);
                material.SetTexture("_MainTex", baseMap);
                if (normal != null)
                {
                    material.SetTexture("_BumpMap", normal);
                    material.EnableKeyword("_NORMALMAP");
                }
                if (mask != null)
                {
                    material.SetTexture("_MetallicGlossMap", mask);
                    material.SetTexture("_OcclusionMap", mask);
                    material.EnableKeyword("_METALLICSPECGLOSSMAP");
                }
                if (name == "House")
                {
                    var emission = AssetDatabase.LoadAssetAtPath<Texture2D>($"{FoxIsleRoot}/Textures/FloatingIsle_House_Emissive.png");
                    if (emission != null)
                    {
                        material.SetTexture("_EmissionMap", emission);
                        material.SetColor("_EmissionColor", Color.white);
                        material.EnableKeyword("_EMISSION");
                    }
                }
                EditorUtility.SetDirty(material);
            }
        }

        /// <summary>Loads a Fox Dev texture and configures its color-space/import type for URP.</summary>
        /// <param name="part">Texture family, such as Grass or Stone.</param>
        /// <param name="suffix">Map suffix used in the official package.</param>
        /// <param name="normalMap">Whether Unity should import this image as a normal map.</param>
        /// <param name="linear">Whether the image stores data instead of display colors.</param>
        /// <returns>The imported texture, or null when that optional map does not exist.</returns>
        static Texture2D LoadFoxTexture(string part, string suffix, bool normalMap, bool linear = false)
        {
            var path = $"{FoxIsleRoot}/Textures/FloatingIsle_{part}_{suffix}.png";
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return null;
            var desiredType = normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
            if (importer.textureType != desiredType || importer.sRGBTexture == linear)
            {
                importer.textureType = desiredType;
                importer.sRGBTexture = !linear;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
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
            var camera = CreateAnimatedMenuBackdrop(materials);
            var canvas = CreateCanvas("Main Menu Canvas");
            var controller = canvas.gameObject.AddComponent<MenuController>();

            var panel = CreateMenuPanel(canvas.transform);
            CreateText(panel, "Kicker", "UMA AVENTURA NA FAZENDA", 16, new Vector2(0.5f, 0.82f), new Vector2(560f, 36f), TextAnchor.MiddleLeft, new Color(0.95f, 0.71f, 0.36f));
            var title = CreateText(panel, "Title", "O MENINO LOIRO\nE OS DEDOS PERDIDOS", 58, new Vector2(0.5f, 0.66f), new Vector2(560f, 180f), TextAnchor.MiddleLeft, new Color(1f, 0.96f, 0.87f));
            title.fontStyle = FontStyle.Bold;
            var shadow = title.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.18f, 0.07f, 0.04f, 0.75f);
            shadow.effectDistance = new Vector2(3f, -3f);
            CreateText(panel, "Subtitle", "Corra, salte e encontre o que a fazenda esconde.", 22, new Vector2(0.5f, 0.48f), new Vector2(560f, 58f), TextAnchor.MiddleLeft, new Color(0.85f, 0.77f, 0.62f));
            var play = CreateMenuButton(panel, "Play", "JOGAR", new Vector2(0.5f, 0.31f), true);
            var quit = CreateMenuButton(panel, "Quit", "SAIR", new Vector2(0.5f, 0.18f), false);
            UnityEventTools.AddPersistentListener(play.onClick, controller.LoadLevelSelect);
            UnityEventTools.AddPersistentListener(quit.onClick, controller.Quit);
            Object.FindAnyObjectByType<EventSystem>().firstSelectedGameObject = play.gameObject;
            new GameObject(AnimatedMenuVersion);
            SaveScene(scene, $"{SceneRoot}/Menus/MainMenu.unity");
        }

        static Camera CreateAnimatedMenuBackdrop(Dictionary<string, Material> materials)
        {
            RenderSettings.ambientLight = new Color(0.42f, 0.35f, 0.29f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.32f, 0.42f, 0.43f);
            RenderSettings.fogDensity = 0.012f;
            CreateSun();

            var camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.fieldOfView = 46f;
            camera.transform.SetPositionAndRotation(new Vector3(0f, 5.6f, -16f), Quaternion.Euler(8f, 0f, 0f));
            camera.backgroundColor = new Color(0.13f, 0.25f, 0.29f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.gameObject.AddComponent<AudioListener>();

            CreateBlock("Campo do Menu", new Vector3(0f, -0.55f, 8f), new Vector3(32f, 1f, 24f), materials["Green"], "Ground");
            CreateModel(FarmBuildings + "BigBarn.fbx", "Celeiro ao Fundo", null, new Vector3(5.2f, 0f, 10f), Quaternion.Euler(0f, -18f, 0f), 8.5f, true);
            CreateModel(FarmBuildings + "Fence.fbx", "Cerca", null, new Vector3(-4f, 0f, 5.6f), Quaternion.Euler(0f, 90f, 0f), 1.25f, true);
            CreateModel(Nature + "CommonTree_3.fbx", "Arvore", null, new Vector3(10.5f, 0f, 12f), Quaternion.Euler(0f, -20f, 0f), 7f, true);

            var characterRoot = new GameObject("Menino do Menu");
            var character = CreateModel(CharacterModel, "Modelo", characterRoot.transform, Vector3.zero, Quaternion.Euler(0f, 90f, 0f), 2.15f, false);
            if (character != null)
            {
                // Unity's "fake null" defeats ??, so a missing Animator must be checked with ==.
                var animator = character.GetComponent<Animator>();
                if (animator == null) animator = character.AddComponent<Animator>();
                animator.avatar = AssetDatabase.LoadAllAssetsAtPath(CharacterModel).OfType<Avatar>().FirstOrDefault(avatar => avatar.isValid && avatar.isHuman);
                animator.runtimeAnimatorController = CreateMenuCharacterController();
                animator.applyRootMotion = false;
                camera.gameObject.AddComponent<MenuSceneAnimator>().Configure(characterRoot.transform, animator, camera.transform,
                    new Vector3(-9f, 0f, 2.8f), new Vector3(9f, 0f, 2.8f));
            }
            else Object.DestroyImmediate(characterRoot);

            CreateMenuBirdFlock();
            return camera;
        }

        static AnimatorController CreateMenuCharacterController()
        {
            const string path = Root + "/Settings/MenuCharacter.controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            var machine = controller.layers[0].stateMachine;
            var walk = machine.states.FirstOrDefault(item => item.state.name == "Walk").state ?? machine.AddState("Walk");
            var jump = machine.states.FirstOrDefault(item => item.state.name == "Jump").state ?? machine.AddState("Jump");
            walk.motion = FindAnimation("Armature|Walk_Loop");
            jump.motion = FindAnimation("Armature|Jump_Loop");
            machine.defaultState = walk;
            return controller;
        }

        static void CreateMenuBirdFlock()
        {
            var flock = new GameObject("Passaros do Menu");
            for (var i = 0; i < 5; i++)
            {
                var bird = new GameObject($"Passaro {i + 1}");
                bird.transform.SetParent(flock.transform);
                bird.AddComponent<BirdFlight>().Configure(new Vector3(0f, 8.5f + i * 0.35f, 11f + i), 5.5f + i,
                    8f + i * 1.1f, i * 1.15f);
                CreateModel(BirdModel, "Modelo", bird.transform, Vector3.zero, Quaternion.Euler(0f, 90f, 0f), 0.55f + i * 0.06f, false);
            }
        }

        static Transform CreateMenuPanel(Transform parent)
        {
            var panel = new GameObject("Menu Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0.43f, 1f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0.07f, 0.075f, 0.07f, 0.82f);
            return panel.transform;
        }

        static Button CreateMenuButton(Transform parent, string name, string label, Vector2 anchor, bool primary)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var rect = (RectTransform)buttonObject.transform;
            rect.anchorMin = rect.anchorMax = anchor;
            rect.sizeDelta = new Vector2(560f, 76f);
            var image = buttonObject.GetComponent<Image>();
            image.color = primary ? new Color(0.48f, 0.14f, 0.09f, 0.98f) : new Color(0.16f, 0.16f, 0.14f, 0.88f);
            var button = buttonObject.GetComponent<Button>();
            var colors = button.colors;
            colors.normalColor = image.color;
            colors.highlightedColor = primary ? new Color(0.59f, 0.20f, 0.13f) : new Color(0.24f, 0.23f, 0.19f);
            colors.pressedColor = new Color(0.34f, 0.09f, 0.06f);
            colors.selectedColor = colors.highlightedColor;
            colors.fadeDuration = 0.18f;
            button.colors = colors;
            var text = CreateText(buttonObject.transform, "Label", label, 22, new Vector2(0.5f, 0.5f), rect.sizeDelta, TextAnchor.MiddleLeft, Color.white);
            var labelRect = (RectTransform)text.transform;
            labelRect.sizeDelta = new Vector2(492f, 76f);
            labelRect.anchoredPosition = new Vector2(17f, 0f);
            text.fontStyle = FontStyle.Bold;
            return button;
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
            player.AddComponent<AudioSource>();
            player.AddComponent<PlayerFootsteps>();

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
            player.AddComponent<PlayerVisualGrounding>().Configure(player.GetComponent<PlayerMotor>(), animator, character.transform);
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
        static readonly Vector2 FinalCenter = new(0f, 116f);
        const float BarnHeight = 13f;
        const float SiloBase = 17f, SiloHeight = 16f;
        const float MillBase = 38f, MillHeight = 24f;

        // Finale: the Fox Dev cottage isle, bigger than the others; the player lands at its door.
        const float FinalBase = 66f;
        const float FinalIslandScale = 24f;
        const float FinalLanding = 9.5f;
        const float IslandScale = 18f;

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
            exit = AddFloatingStation(route, "Moinho", exit, MillCenter, MillBase, MillHeight, 4.6f, -45f, FinalCenter);
            AddBridge(route, "Casa", exit, FinalCenter, FinalBase, FinalLanding);
            return route;
        }

        static Vector3 OnCircle(Vector2 center, float radius, float degrees, float y) =>
            new(center.x + Mathf.Sin(degrees * Mathf.Deg2Rad) * radius, y, center.y + Mathf.Cos(degrees * Mathf.Deg2Rad) * radius);

        static float AngleFrom(Vector2 center, Vector3 point) =>
            Mathf.Atan2(point.x - center.x, point.z - center.y) * Mathf.Rad2Deg;

        /// <summary>Adds a rising line of floating props from a standing spot onto an island; returns the landing.</summary>
        static Vector3 AddBridge(List<(string name, Vector3[] points)> route, string name, Vector3 start,
            Vector2 center, float islandTop, float landingRadius)
        {
            var landing = OnCircle(center, landingRadius, AngleFrom(center, start), islandTop);
            var jumps = Mathf.CeilToInt(Vector2.Distance(new Vector2(start.x, start.z), new Vector2(landing.x, landing.z)) / BridgeSpacing);
            var bridge = new List<Vector3> { start };
            for (var k = 1; k < jumps; k++) bridge.Add(Vector3.Lerp(start, landing, (float)k / jumps));
            bridge.Add(landing);
            route.Add(($"Ponte {name}", bridge.ToArray()));
            return landing;
        }

        /// <summary>
        /// Adds a floating bridge onto the building's island and a spiral from the island to its deck.
        /// Returns the deck spot facing the next building, where the following bridge starts.
        /// </summary>
        static Vector3 AddFloatingStation(List<(string name, Vector3[] points)> route, string name, Vector3 start,
            Vector2 center, float baseY, float height, float spiralRadius, float angleStep, Vector2? next)
        {
            var landing = AddBridge(route, name, start, center, baseY, IslandLanding);
            var arrival = AngleFrom(center, landing);
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
            new GameObject("FAZENDA_VERTICAL_V30");
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
            CreateIsland("Casa", FinalCenter, FinalBase, Last(route[5].points), materials, cottage: true);

            // Each deck carries a beacon over the spot where the next bridge starts.
            CreateStation("Celeiro", new Vector3(BarnCenter.x, StationTop(0f, BarnHeight), BarnCenter.y), new Vector3(5f, DeckThickness, 15f), route[1].points[0], materials);
            CreateStation("Silo", new Vector3(SiloCenter.x, StationTop(SiloBase, SiloHeight), SiloCenter.y), new Vector3(5f, DeckThickness, 5f), route[3].points[0], materials);
            CreateStation("Moinho", new Vector3(MillCenter.x, StationTop(MillBase, MillHeight), MillCenter.y), new Vector3(4.2f, DeckThickness, 4.2f), route[5].points[0], materials);
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
        /// Floating island. It uses the hand-painted Fox Dev isle when the pack is installed (base
        /// only, or with its cottage for the finale) and falls back to simple shapes otherwise.
        /// An invisible flat disc is the walkable surface, so a fall from the spiral lands back on
        /// the island and the player retries from there instead of from the field.
        /// </summary>
        static void CreateIsland(string name, Vector2 center, float top, Vector3 landing,
            Dictionary<string, Material> materials, bool cottage = false)
        {
            var root = new GameObject($"Ilha {name}").transform;
            var radius = cottage ? FinalIslandScale * 0.445f : IslandRadius;
            var arrival = AngleFrom(center, landing);
            var grass = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            grass.name = "Gramado";
            grass.layer = LayerMask.NameToLayer("Ground");
            grass.transform.SetParent(root, false);
            grass.transform.position = new Vector3(center.x, top - 0.25f, center.y);
            grass.transform.localScale = new Vector3(radius * 2f, 0.25f, radius * 2f);
            grass.GetComponent<Renderer>().sharedMaterial = materials["Green"];
            Object.DestroyImmediate(grass.GetComponent<Collider>());
            grass.AddComponent<MeshCollider>();

            if (PlaceFoxIsle(root, center, top, arrival, cottage ? FinalIslandScale : IslandScale, cottage))
                SetVisible(grass, false);
            else
            {
                CreateVisual(PrimitiveType.Sphere, "Terra", root, Vector3.zero, new Vector3(radius * 1.95f, 5f, radius * 1.95f), materials["Soil"])
                    .transform.position = new Vector3(center.x, top - 3f, center.y);
                CreateVisual(PrimitiveType.Sphere, "Rocha", root, Vector3.zero, new Vector3(radius * 1.1f, 6f, radius * 1.1f), materials["Soil"])
                    .transform.position = new Vector3(center.x, top - 6f, center.y);
                var rocks = new[] { "Rock1.fbx", "Rock2.fbx", "Rock3.fbx", "Rocks.fbx" };
                for (var i = 0; i < 6; i++)
                {
                    // Models are placed by their base, so the base goes below the rock's own height.
                    var height = 3f + i % 3;
                    var hanging = OnCircle(center, 3.5f + i % 3 * 1.5f, i * 60f + top, top - 0.6f - i % 2 * 2f - height);
                    CreateModel(IslandPack + rocks[i % rocks.Length], $"Rocha Pendurada {i + 1}", root, root.InverseTransformPoint(hanging),
                        Quaternion.Euler(180f, i * 47f, 0f), height, true);
                }
            }

            // Clouds drift below the island, under the bridges.
            for (var i = 0; i < 3; i++)
                CreateModel(IslandPack + "Cloud.fbx", $"Nuvem {i + 1}", root,
                    root.InverseTransformPoint(OnCircle(center, radius + 6f + i * 2f, i * 120f + top * 3f, top - 9f - i * 1.5f)),
                    Quaternion.Euler(0f, i * 70f, 0f), 2.5f + i * 0.5f, true);
            if (cottage) return;

            // Decor sits on the rim, away from where the bridge arrives, outside the spiral's footprint.
            var decor = new[] { FarmProps + "Hay.fbx", Nature + "Bush_Common_Flowers.fbx", FarmProps + "Barrel.fbx",
                Nature + "Flower_3_Group.fbx", FarmProps + "Bags.fbx", Nature + "Grass_Common_Tall.fbx", Nature + "Bush_Common.fbx" };
            var heights = new[] { 1.1f, 0.6f, 1.1f, 0.3f, 0.8f, 0.5f, 0.6f };
            for (var i = 0; i < decor.Length; i++)
            {
                var point = OnCircle(center, radius * 0.85f, arrival + 70f + i * 37f, top);
                CreateModel(decor[i], $"Enfeite {i + 1}", root, root.InverseTransformPoint(point),
                    Quaternion.Euler(0f, i * 53f, 0f), heights[i], true);
            }
        }

        /// <summary>
        /// Places the Fox Dev isle so its grass meets the route's island height. The grass is
        /// slightly domed, so its height is sampled on the ring the player walks and averaged.
        /// The cottage faces the arriving bridge, with its walls made solid.
        /// </summary>
        /// <returns>False when the pack is not installed (it is not redistributable).</returns>
        static bool PlaceFoxIsle(Transform root, Vector2 center, float surfaceY, float yaw, float scale, bool cottage)
        {
            var isle = cottage ? InstantiateFoxParts(root) : InstantiateFoxParts(root, "RockyBase", "GeoSphere001");
            if (isle == null) return false;
            isle.name = cottage ? "Ilha da Casa (Fox Dev)" : "Ilha (Fox Dev)";
            var rock = isle.transform.Find("RockyBase");
            var grass = isle.transform.Find("GeoSphere001");
            if (rock == null || grass == null)
            {
                Debug.LogWarning("Floating Isle sem RockyBase/GeoSphere001; usando ilha simples.");
                Object.DestroyImmediate(isle);
                return false;
            }

            isle.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(0f, yaw, 0f));
            isle.transform.localScale = Vector3.one * scale;
            var rockCenter = rock.GetComponent<Renderer>().bounds.center;
            isle.transform.position = new Vector3(center.x - rockCenter.x, 0f, center.y - rockCenter.z);

            var probe = grass.gameObject.AddComponent<MeshCollider>();
            Physics.SyncTransforms();
            float sum = 0f, hits = 0f;
            for (var angle = 0f; angle < 360f; angle += 30f)
                if (probe.Raycast(new Ray(OnCircle(center, scale * 0.35f, angle, 500f), Vector3.down), out var hit, 1000f))
                {
                    sum += hit.point.y;
                    hits++;
                }
            Object.DestroyImmediate(probe);
            var grassY = hits > 0f ? sum / hits : grass.GetComponent<Renderer>().bounds.max.y;
            isle.transform.position += Vector3.up * (surfaceY - grassY);

            var house = isle.transform.Find("House");
            if (cottage && house != null)
            {
                house.gameObject.layer = LayerMask.NameToLayer("CameraBlocker");
                house.gameObject.AddComponent<BoxCollider>();
            }
            return true;
        }

        // The field is itself a big floating island; past its edge is the void.
        static readonly Rect FieldIsland = new(-32f, -18f, 72f, 68f);

        static void CreateFarmTerrain(Dictionary<string, Material> materials)
        {
            var root = new GameObject("Ilha da Fazenda").transform;
            var field = FieldIsland;
            CreateBlock("Pasto", new Vector3(field.center.x, -0.6f, field.center.y), new Vector3(field.width, 1.2f, field.height), materials["Green"], "Ground")
                .transform.SetParent(root, true);
            // Dirt surfaces are visual-only meshes with curved borders, a hair above the grass.
            CreateCurvedDirtPath(root, materials["Soil"]);
            CreateRoundedGroundPatch("Terreiro do Celeiro", root, new Vector3(11f, 0.018f, 22.5f), new Vector2(25f, 9f), 3.2f, materials["Soil"]);

            // Fence on the rim plus a low invisible wall: walking never drops you off, a jump or a fall can.
            var corners = new[]
            {
                new Vector3(field.xMin, 0f, field.yMin), new Vector3(field.xMax, 0f, field.yMin),
                new Vector3(field.xMax, 0f, field.yMax), new Vector3(field.xMin, 0f, field.yMax)
            };
            for (var i = 0; i < 4; i++)
            {
                var from = corners[i] + (corners[(i + 2) % 4] - corners[i]).normalized * 1.2f;
                var to = corners[(i + 1) % 4] + (corners[(i + 3) % 4] - corners[(i + 1) % 4]).normalized * 1.2f;
                FenceLine(root, $"Cerca da Borda {i + 1}", from, (to - from).normalized, Vector3.Distance(from, to));
                var wall = CreateBlock($"Limite {i + 1}", (from + to) * 0.5f + Vector3.up * 0.55f,
                    new Vector3(Mathf.Abs(to.x - from.x) + 0.3f, 1.1f, Mathf.Abs(to.z - from.z) + 0.3f), null, "Ground");
                SetVisible(wall, false);
                wall.transform.SetParent(root, true);
            }

            // Cliff: a ring of rocks under the rim, then three big rocks forming the island's body.
            var cliff = new GameObject("Penhasco").transform;
            cliff.SetParent(root, false);
            var step = 0;
            for (var x = field.xMin + 6f; x <= field.xMax - 6f; x += 15f, step++)
            {
                CreateUnderRock(cliff, new Vector3(x, -0.3f, field.yMin + 6f), 16f, step * 67f);
                CreateUnderRock(cliff, new Vector3(x, -0.3f, field.yMax - 6f), 16f, step * 41f);
            }
            for (var z = field.yMin + 21f; z <= field.yMax - 21f; z += 15f, step++)
            {
                CreateUnderRock(cliff, new Vector3(field.xMin + 6f, -0.3f, z), 16f, step * 53f);
                CreateUnderRock(cliff, new Vector3(field.xMax - 6f, -0.3f, z), 16f, step * 29f);
            }
            for (var i = 0; i < 2; i++)
                CreateUnderRock(cliff, new Vector3(field.center.x, -1.5f, field.center.y - 14f + i * 28f), 55f, i * 150f);
        }

        /// <summary>Creates a gently winding farm road with smooth sides instead of a rectangular block.</summary>
        /// <param name="parent">Farm terrain hierarchy that receives the visual road.</param>
        /// <param name="material">Tiled dirt material used on the road surface.</param>
        static void CreateCurvedDirtPath(Transform parent, Material material)
        {
            var controls = new[]
            {
                new Vector3(0.2f, 0.014f, -17f),
                new Vector3(-0.3f, 0.014f, -10f),
                new Vector3(0.35f, 0.014f, -2f),
                new Vector3(-0.45f, 0.014f, 6f),
                new Vector3(0.45f, 0.014f, 14f),
                new Vector3(2.2f, 0.014f, 20f),
                new Vector3(8f, 0.014f, 22.2f)
            };
            const int samplesPerSegment = 7;
            const float width = 6.2f;
            var centers = new List<Vector3>();
            for (var segment = 0; segment < controls.Length - 1; segment++)
                for (var sample = 0; sample < samplesPerSegment; sample++)
                {
                    var t = sample / (float)samplesPerSegment;
                    var a = controls[Mathf.Max(0, segment - 1)];
                    var b = controls[segment];
                    var c = controls[segment + 1];
                    var d = controls[Mathf.Min(controls.Length - 1, segment + 2)];
                    centers.Add(CatmullRom(a, b, c, d, t));
                }
            centers.Add(controls[controls.Length - 1]);

            var vertices = new Vector3[centers.Count * 2];
            var uv = new Vector2[vertices.Length];
            var triangles = new int[(centers.Count - 1) * 6];
            var traveled = 0f;
            for (var i = 0; i < centers.Count; i++)
            {
                if (i > 0) traveled += Vector3.Distance(centers[i - 1], centers[i]);
                var previous = centers[Mathf.Max(0, i - 1)];
                var next = centers[Mathf.Min(centers.Count - 1, i + 1)];
                var side = Vector3.Cross(Vector3.up, (next - previous).normalized);
                var edgeVariation = Mathf.Sin(i * 1.73f) * 0.22f + Mathf.Sin(i * 0.47f) * 0.14f;
                var halfWidth = width * 0.5f + edgeVariation;
                vertices[i * 2] = centers[i] - side * halfWidth;
                vertices[i * 2 + 1] = centers[i] + side * halfWidth;
                uv[i * 2] = new Vector2(0f, traveled / 36f);
                uv[i * 2 + 1] = new Vector2(1f, traveled / 36f);
                if (i >= centers.Count - 1) continue;
                var index = i * 6;
                var vertex = i * 2;
                triangles[index] = vertex;
                triangles[index + 1] = vertex + 2;
                triangles[index + 2] = vertex + 1;
                triangles[index + 3] = vertex + 1;
                triangles[index + 4] = vertex + 2;
                triangles[index + 5] = vertex + 3;
            }
            CreateGroundMesh("Estrada de Terra Curva", parent, vertices, uv, triangles, material);
        }

        /// <summary>Evaluates a smooth Catmull-Rom curve between the middle control points.</summary>
        /// <param name="a">Control point before the segment.</param>
        /// <param name="b">Segment start.</param>
        /// <param name="c">Segment end.</param>
        /// <param name="d">Control point after the segment.</param>
        /// <param name="t">Normalized position from zero to one.</param>
        /// <returns>A smoothly interpolated world position.</returns>
        static Vector3 CatmullRom(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
        {
            var t2 = t * t;
            var t3 = t2 * t;
            return 0.5f * ((2f * b) + (-a + c) * t + (2f * a - 5f * b + 4f * c - d) * t2 + (-a + 3f * b - 3f * c + d) * t3);
        }

        /// <summary>Creates a flat rectangular dirt area with circular, subdivided corners.</summary>
        /// <param name="name">Readable hierarchy name.</param>
        /// <param name="parent">Terrain hierarchy receiving the patch.</param>
        /// <param name="center">World center slightly above the grass.</param>
        /// <param name="size">Width and depth of the patch.</param>
        /// <param name="radius">Corner radius, clamped to half the smallest dimension.</param>
        /// <param name="material">Tiled dirt material.</param>
        static void CreateRoundedGroundPatch(string name, Transform parent, Vector3 center, Vector2 size, float radius, Material material)
        {
            const int cornerSegments = 7;
            radius = Mathf.Min(radius, Mathf.Min(size.x, size.y) * 0.5f);
            var perimeter = new List<Vector3>();
            var cornerCenters = new[]
            {
                new Vector2(size.x * 0.5f - radius, size.y * 0.5f - radius),
                new Vector2(-size.x * 0.5f + radius, size.y * 0.5f - radius),
                new Vector2(-size.x * 0.5f + radius, -size.y * 0.5f + radius),
                new Vector2(size.x * 0.5f - radius, -size.y * 0.5f + radius)
            };
            for (var corner = 0; corner < 4; corner++)
                for (var step = 0; step <= cornerSegments; step++)
                {
                    var angle = (corner * 90f + step * 90f / cornerSegments) * Mathf.Deg2Rad;
                    var point = cornerCenters[corner] + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                    perimeter.Add(center + new Vector3(point.x, 0f, point.y));
                }

            var vertices = new Vector3[perimeter.Count + 1];
            var uv = new Vector2[vertices.Length];
            var triangles = new int[perimeter.Count * 3];
            vertices[0] = center;
            uv[0] = new Vector2(0.5f, 0.5f);
            for (var i = 0; i < perimeter.Count; i++)
            {
                vertices[i + 1] = perimeter[i];
                uv[i + 1] = new Vector2((perimeter[i].x - center.x) / size.x + 0.5f, (perimeter[i].z - center.z) / size.y + 0.5f);
                triangles[i * 3] = 0;
                // Clockwise order in the X/Z plane makes the surface normal point upward.
                triangles[i * 3 + 1] = (i + 1) % perimeter.Count + 1;
                triangles[i * 3 + 2] = i + 1;
            }
            CreateGroundMesh(name, parent, vertices, uv, triangles, material);
        }

        /// <summary>Creates a visual-only mesh from prepared vertices, UVs and triangle indices.</summary>
        /// <param name="name">Name shown in the scene hierarchy.</param>
        /// <param name="parent">Parent transform for organization.</param>
        /// <param name="vertices">World-space surface vertices.</param>
        /// <param name="uv">Texture coordinates corresponding to every vertex.</param>
        /// <param name="triangles">Triangle index buffer.</param>
        /// <param name="material">Material rendered on the mesh.</param>
        static void CreateGroundMesh(string name, Transform parent, Vector3[] vertices, Vector2[] uv, int[] triangles, Material material)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, true);
            var mesh = new Mesh { name = name + " Mesh" };
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        /// <summary>Hangs a rock under the island with its top at <paramref name="top"/>; visual only.</summary>
        static void CreateUnderRock(Transform parent, Vector3 top, float size, float yaw)
        {
            var rock = InstantiateFoxParts(parent, "RockyBase");
            if (rock == null)
            {
                // Without the Fox Dev pack: an upside-down Whitebird rock, base placed below its height.
                CreateModel(IslandPack + "Rocks.fbx", "Rocha", parent, parent.InverseTransformPoint(top - Vector3.up * size * 0.8f),
                    Quaternion.Euler(180f, yaw, 0f), size * 0.8f, true);
                return;
            }
            rock.name = "Rocha (Fox Dev)";
            rock.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(0f, yaw, 0f));
            rock.transform.localScale = Vector3.one * size;
            var bounds = rock.GetComponentInChildren<Renderer>().bounds;
            rock.transform.position = top - new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
        }

        /// <summary>
        /// Instantiates the Fox Dev isle keeping only the named parts. Returns null when the pack
        /// is not installed (its license forbids redistribution, so it stays out of Git).
        /// </summary>
        static GameObject InstantiateFoxParts(Transform parent, params string[] keep)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(FoxIsle);
            if (asset == null) return null;
            var isle = Object.Instantiate(asset, parent);
            if (keep.Length > 0)
                foreach (Transform part in isle.transform.Cast<Transform>().ToArray())
                    if (!keep.Contains(part.name)) Object.DestroyImmediate(part.gameObject);
            foreach (var child in isle.GetComponentsInChildren<Transform>(true))
                GameObjectUtility.SetStaticEditorFlags(child.gameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic);
            ApplyFoxIsleMaterials(isle);
            return isle;
        }

        /// <summary>Matches each separable island mesh to its restored hand-painted material.</summary>
        /// <param name="isle">Instantiated Fox Dev model containing the island pieces.</param>
        static void ApplyFoxIsleMaterials(GameObject isle)
        {
            foreach (var renderer in isle.GetComponentsInChildren<Renderer>(true))
            {
                var label = (renderer.gameObject.name + " " + renderer.sharedMaterial?.name).ToLowerInvariant();
                var materialName = label.Contains("rocky") || label.Contains("stone") ? "Stone" :
                    label.Contains("geo") || label.Contains("grass") ? "Grass" :
                    label.Contains("chimney") ? "Chimney" :
                    label.Contains("fence") ? "Fence" :
                    label.Contains("roof") ? "Roof" :
                    label.Contains("stair") ? "Stairs" :
                    label.Contains("wind") ? "Windmill" :
                    label.Contains("house") ? "House" : "Decor";
                var material = AssetDatabase.LoadAssetAtPath<Material>($"{FoxIsleRoot}/Materials/{materialName}.mat");
                if (material == null) continue;
                var slots = renderer.sharedMaterials;
                for (var i = 0; i < slots.Length; i++) slots[i] = material;
                renderer.sharedMaterials = slots;
            }
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
        static Transform FitToBox(GameObject visual, Vector3 center, Quaternion rotation, Vector3 size)
        {
            var renderers = visual.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return visual.transform;
            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);

            // A world-aligned parent avoids skewing FBX roots that carry their own axis rotation.
            var pivot = new GameObject(visual.name + " Ajuste").transform;
            pivot.position = bounds.center;
            visual.transform.SetParent(pivot, true);
            pivot.localScale = new Vector3(size.x / Mathf.Max(bounds.size.x, 0.01f),
                size.y / Mathf.Max(bounds.size.y, 0.01f), size.z / Mathf.Max(bounds.size.z, 0.01f));
            pivot.SetPositionAndRotation(center, rotation);
            return pivot;
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

            CreateGroundFarm(materials);
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
            new(-10f, 27f, 4f, 12f),     // harvest by the field gate
            new(22f, 0f, 16f, 22f),      // animal pen
            new(-26f, -14f, 15f, 15f),   // machine shed
            new(11f, -12f, 10f, 10f),    // tractor
            new(-25f, 10f, 10f, 10f),    // chicken coop
            new(14f, 8f, 7f, 7f),        // well
            new(-14.5f, 1.5f, 5f, 5.5f), // picnic table
            new(-30.5f, -0.5f, 7f, 7f),  // small windmill
            new(7f, -15f, 8f, 8f),       // workshop
            new(24f, -14f, 10f, 10f),    // parked truck
            new(28f, 30f, 11f, 12f),     // pond
            new(28f, 22.5f, 10f, 5.5f),  // big hay bales behind the pen
            new(-10.5f, 1.5f, 3.5f, 3.5f) // campfire
        };

        /// <summary>
        /// The starting island, laid out in readable zones: entrance, vegetable field on the left,
        /// animal pen on the right and storage beside the barn.
        /// </summary>
        static void CreateGroundFarm(IReadOnlyDictionary<string, Material> materials)
        {
            var entrance = new GameObject("Zona - Entrada").transform;
            CreateModel(FarmBuildings + "OpenBarn.fbx", "Galpao de Maquinas", entrance, OnGround(-19f, -7f), Quaternion.Euler(0f, 90f, 0f), 6.5f, true);
            var tractor = CreateModel(TractorModel, "Trator", entrance, OnGround(16f, -7f), Quaternion.Euler(0f, -25f, 0f), 2.7f, true);
            ApplyTractorMaterials(tractor, materials);
            CreateModel(FarmProps + "Cart.fbx", "Carroca", entrance, OnGround(-12f, -3f), Quaternion.Euler(0f, 30f, 0f), 1.6f, true);
            CreateModel(FarmBuildings + "ChickenCoop.fbx", "Galinheiro", entrance, OnGround(-20f, 15f), Quaternion.Euler(0f, 90f, 0f), 3.2f, true);
            CreateModel(FarmBuildings + "Well.fbx", "Poco", entrance, OnGround(17.5f, 11.5f), Quaternion.identity, 2.8f, true);
            PropRow(entrance, "Feno do Galpao", "Hay.fbx", new Vector3(-23f, 0f, -1f), Vector3.right * 2.4f, 3, 1.1f);
            PropRow(entrance, "Barris do Galpao", "Barrel.fbx", new Vector3(-14f, 0f, -12f), Vector3.right * 1.3f, 3, 1.1f);
            // Fence lines flank the entrance road and open where it meets the barn yard.
            FenceLine(entrance, "Cerca da Estrada E", OnGround(-4.5f, -12f), Vector3.forward, 18f);
            FenceLine(entrance, "Cerca da Estrada D", OnGround(4.5f, -12f), Vector3.forward, 18f);
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

            CreateFarmDetails(new GameObject("Zona - Detalhes").transform);
            CreateFarmProps(new GameObject("Zona - Objetos da Fazenda").transform);
            CreateNature(new GameObject("Zona - Natureza").transform);
        }

        /// <summary>Colors the tractor parts because its source FBX does not include usable material assets.</summary>
        /// <param name="tractor">Instantiated tractor whose child renderers will receive farm materials.</param>
        /// <param name="materials">Project materials indexed by their descriptive names.</param>
        static void ApplyTractorMaterials(GameObject tractor, IReadOnlyDictionary<string, Material> materials)
        {
            if (tractor == null) return;

            foreach (var renderer in tractor.GetComponentsInChildren<Renderer>(true))
            {
                var partName = renderer.gameObject.name.ToLowerInvariant();
                var selected = materials["TractorRed"];

                if (partName.Contains("tyre") || partName.Contains("tire"))
                    selected = materials["TireDark"];
                else if (partName.Contains("seat"))
                    selected = materials["SeatLeather"];
                else if (partName.Contains("wheel") || partName.Contains("hub") || partName.Contains("axle") ||
                         partName.Contains("exhaust") || partName.Contains("hitch"))
                    selected = materials["Metal"];

                var assigned = renderer.sharedMaterials;
                for (var i = 0; i < assigned.Length; i++) assigned[i] = selected;
                renderer.sharedMaterials = assigned;
            }
        }

        // Ultimate Crops: "_4" models are the grown plants; "_Crop" models are the harvested produce.
        // Each entry: plant model, height, planting columns across the bed (x offsets), row spacing.
        static readonly (string model, float height, float[] columns, float spacing)[] CropBeds =
        {
            ("Corn_4.fbx", 1.9f, new[] { -0.55f, 0.55f }, 0.9f),
            ("Wheat_4.fbx", 1f, new[] { -0.8f, -0.27f, 0.27f, 0.8f }, 0.4f),
            ("Pumpkin_4.fbx", 0.55f, new[] { 0f }, 1.3f),
            ("Carrot_4.fbx", 0.55f, new[] { -0.7f, 0f, 0.7f }, 0.6f),
            ("Lettuce_4.fbx", 0.35f, new[] { -0.55f, 0.55f }, 0.8f),
        };

        /// <summary>
        /// Fenced kitchen garden: a central path from the gate splits it into two halves of four
        /// raised beds, tall crops at the back and low ones by the path; harvest waits by the gate.
        /// </summary>
        static void CreateVegetableField(Transform root)
        {
            var soil = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialRoot}/Soil.mat");
            var wood = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialRoot}/Wood.mat");
            CreateVisual(PrimitiveType.Cube, "Caminho da Horta", root, new Vector3(-19.2f, 0.01f, 33f), new Vector3(16.4f, 0.02f, 1.8f), soil);

            var random = new System.Random(11);
            var bedX = new[] { -25f, -21.6f, -18.2f, -14.8f };
            var halves = new[] { (center: 26.9f, length: 9.4f, crops: new[] { 0, 2, 1, 3 }), (center: 40.1f, length: 11.4f, crops: new[] { 0, 1, 3, 4 }) };
            var bed = 0;
            foreach (var half in halves)
                for (var i = 0; i < bedX.Length; i++)
                {
                    var crop = CropBeds[half.crops[i]];
                    var center = new Vector3(bedX[i], 0f, half.center);
                    var name = $"Canteiro {++bed}";
                    CreateVisual(PrimitiveType.Cube, $"{name} Terra", root, center + Vector3.up * 0.1f, new Vector3(2.4f, 0.2f, half.length), soil);
                    for (var side = -1; side <= 1; side += 2)
                    {
                        CreateVisual(PrimitiveType.Cube, $"{name} Borda", root, center + new Vector3(side * 1.26f, 0.13f, 0f), new Vector3(0.12f, 0.26f, half.length + 0.12f), wood);
                        CreateVisual(PrimitiveType.Cube, $"{name} Borda", root, center + new Vector3(0f, 0.13f, side * (half.length * 0.5f + 0.06f)), new Vector3(2.64f, 0.26f, 0.12f), wood);
                    }

                    var rows = Mathf.FloorToInt((half.length - 0.6f) / crop.spacing) + 1;
                    var first = -(rows - 1) * crop.spacing * 0.5f;
                    for (var row = 0; row < rows; row++)
                        foreach (var column in crop.columns)
                            CreateModel(Crops + crop.model, $"{name} Planta", root,
                                center + new Vector3(column + Jitter(random, 0.06f), 0.2f, first + row * crop.spacing + Jitter(random, 0.06f)),
                                Quaternion.Euler(0f, random.Next(360), 0f), crop.height * (0.9f + (float)random.NextDouble() * 0.2f), true);
                }

            // The path ends at a water barrel by the back fence.
            CreateModel(FarmProps + "Barrel.fbx", "Barril de Agua", root, OnGround(-26.6f, 33f), Quaternion.identity, 1f, true);

            // Harvest outside the gate, clear of the stepping stones at z 33.
            CreateModel(FarmProps + "Crate.fbx", "Caixa de Cenouras", root, OnGround(-8.6f, 28.6f), Quaternion.Euler(0f, 12f, 0f), 0.9f, true);
            for (var i = 0; i < 3; i++)
            {
                CreateModel(Crops + "Carrot_Crop.fbx", $"Cenoura Colhida {i + 1}", root, new Vector3(-8.9f + i * 0.3f, 0.9f, 28.6f),
                    Quaternion.Euler(90f, i * 25f, 0f), 0.14f, true);
                CreateModel(Crops + "Pumpkin_Crop.fbx", $"Abobora Colhida {i + 1}", root, OnGround(-7.4f + i % 2 * 0.6f, 29.8f + i * 0.55f),
                    Quaternion.Euler(0f, i * 70f, 0f), 0.5f, true);
                CreateModel(Crops + "Wheat_Crop.fbx", $"Feixe de Trigo {i + 1}", root, new Vector3(-7.6f, i * 0.12f, 35.6f),
                    Quaternion.Euler(0f, 80f + i * 15f, 0f), 0.12f, true);
            }

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
            CreateVisual(PrimitiveType.Cube, "Chao do Curral", root, new Vector3(30f, 0.01f, 11f), new Vector3(14f, 0.02f, 20f),
                AssetDatabase.LoadAssetAtPath<Material>($"{MaterialRoot}/Soil.mat"));
            // Gate on the side facing the road (x = 23, z 12-16).
            FenceLine(root, "Cerca do Curral Oeste A", new Vector3(23f, 0f, 1f), Vector3.forward, 11f);
            FenceLine(root, "Cerca do Curral Oeste B", new Vector3(23f, 0f, 16f), Vector3.forward, 5f);
            FenceLine(root, "Cerca do Curral Leste", new Vector3(37f, 0f, 1f), Vector3.forward, 20f);
            FenceLine(root, "Cerca do Curral Sul", new Vector3(23f, 0f, 1f), Vector3.right, 14f);
            FenceLine(root, "Cerca do Curral Norte", new Vector3(23f, 0f, 21f), Vector3.right, 14f);
            PropRow(root, "Feno do Curral", "Hay.fbx", new Vector3(35.5f, 0f, 4f), Vector3.forward * 2.4f, 3, 1.1f);
            PropRow(root, "Bebedouro", "Barrel.fbx", new Vector3(25.5f, 0f, 19.5f), Vector3.right * 1.3f, 3, 1f);

            // Each animal's wander radius keeps it inside the 14 x 20 m pen.
            CreateWanderingAnimal("Vaca 1", "Cow.fbx", OnGround(28f, 7f), 1.7f, 3.5f, 0.55f);
            CreateWanderingAnimal("Vaca 2", "Cow.fbx", OnGround(32f, 15f), 1.6f, 3.5f, 0.5f);
            CreateWanderingAnimal("Cavalo", "Horse.fbx", OnGround(30f, 11f), 2.1f, 4.5f, 0.7f);
            CreateWanderingAnimal("Ovelha 1", "Sheep.fbx", OnGround(26.5f, 16f), 1.2f, 2.5f, 0.45f);
            CreateWanderingAnimal("Ovelha 2", "Sheep.fbx", OnGround(34f, 18f), 1.1f, 2.5f, 0.5f);
            CreateWanderingAnimal("Porco", "Pig.fbx", OnGround(34f, 5f), 1f, 2.5f, 0.42f);
        }

        /// <summary>Small touches that make the yard feel lived in: flower beds, a sign, a picnic spot.</summary>
        static void CreateFarmDetails(Transform root)
        {
            var random = new System.Random(23);
            var flowers = new[] { "Flower_4_Group.fbx", "Flower_3_Group.fbx", "Flower_4_Single.fbx", "Flower_3_Single.fbx" };

            // Flower beds outside the road fences, from the spawn to the barn yard.
            for (var z = -11f; z <= 5.5f; z += 0.9f)
                for (var side = -1; side <= 1; side += 2)
                    PlaceNature(root, "Canteiro de Flores da Estrada", flowers[random.Next(flowers.Length)],
                        new Vector2(side * (5.6f + Jitter(random, 0.25f)), z + Jitter(random, 0.2f)), 0.22f, 0.35f, random);
            // A ring of flowers around the well.
            for (var i = 0; i < 10; i++)
                PlaceNature(root, "Flores do Poco", flowers[i % flowers.Length],
                    new Vector2(17.5f + Mathf.Sin(i * 36f * Mathf.Deg2Rad) * 2f, 11.5f + Mathf.Cos(i * 36f * Mathf.Deg2Rad) * 2f), 0.2f, 0.3f, random);

            CreateModel(IslandPack + "Sighn.fbx", "Placa da Fazenda", root, OnGround(-6.2f, 7.5f), Quaternion.Euler(0f, 160f, 0f), 1.7f, true);
            CreateModel(IslandPack + "Table.fbx", "Mesa de Piquenique", root, OnGround(-12f, 4f), Quaternion.Euler(0f, 90f, 0f), 0.8f, true);
            CreateModel(IslandPack + "Chair.fbx", "Cadeira 1", root, OnGround(-12f, 2.6f), Quaternion.identity, 0.9f, true);
            CreateModel(IslandPack + "Chair.fbx", "Cadeira 2", root, OnGround(-12f, 5.4f), Quaternion.Euler(0f, 180f, 0f), 0.9f, true);
            CreateModel(IslandPack + "Weel.fbx", "Roda de Carroca", root, OnGround(20f, -9.5f), Quaternion.Euler(0f, 30f, 12f), 1.1f, true);
            CreateModel(FarmBuildings + "Windmill.fbx", "Moinho Pequeno", root, OnGround(-27f, 3f), Quaternion.Euler(0f, 120f, 0f), 7f, true);
        }

        /// <summary>
        /// Everyday farm objects from the styloo Cozy Farm and Kenney Survival Kit packs (both CC0),
        /// grouped where they would be used: workshop, parked truck, pond, hay, garden, coop and well.
        /// </summary>
        static void CreateFarmProps(Transform root)
        {
            void Place(string path, string name, float x, float z, float yaw, float height, float y = 0f, float tilt = 0f) =>
                CreateModel(path, name, root, new Vector3(x, y, z), Quaternion.Euler(0f, yaw, tilt), height, true);

            // Workshop by the tractor: bench, anvil, grinder, tools leaning on it, logs and planks.
            Place(SurvivalKit + "workbench.fbx", "Bancada", 10f, -12.5f, 180f, 0.95f);
            Place(SurvivalKit + "workbench-anvil.fbx", "Bigorna", 12.2f, -12.2f, 160f, 0.8f);
            Place(SurvivalKit + "workbench-grind.fbx", "Rebolo", 8f, -12.4f, 200f, 0.9f);
            Place(SurvivalKit + "tool-shovel.fbx", "Pa", 9.1f, -13.5f, 180f, 1.15f, tilt: 12f);
            Place(SurvivalKit + "tool-hoe.fbx", "Enxada", 9.6f, -13.6f, 180f, 1.15f, tilt: -10f);
            Place(SurvivalKit + "tool-axe.fbx", "Machado", 11f, -13.5f, 180f, 0.9f, tilt: 14f);
            Place(SurvivalKit + "tool-pickaxe.fbx", "Picareta", 11.5f, -13.6f, 180f, 0.95f, tilt: -12f);
            for (var i = 0; i < 3; i++) Place(SurvivalKit + "tree-log.fbx", $"Tora {i + 1}", 8.2f + i * 0.6f, -9.6f, 90f, 0.55f);
            for (var i = 0; i < 2; i++) Place(SurvivalKit + "tree-log.fbx", $"Tora de Cima {i + 1}", 8.5f + i * 0.6f, -9.6f, 90f, 0.55f, y: 0.5f);
            Place(SurvivalKit + "resource-planks.fbx", "Tabuas", 12.5f, -9.8f, 30f, 0.3f);
            Place(SurvivalKit + "resource-wood.fbx", "Lenha", 13.3f, -10.6f, 70f, 0.25f);
            Place(SurvivalKit + "barrel.fbx", "Barril da Oficina", 13.8f, -13.2f, 0f, 1f);

            // Truck parked in the south-east corner, with a fuel can.
            Place(CozyFarm + "truck.fbx", "Caminhonete", 29f, -9f, -35f, 2.3f);
            Place(CozyFarm + "gas.fbx", "Galao", 26.4f, -11.2f, 20f, 0.5f);
            Place(CozyFarm + "woodenbox.fbx", "Caixote da Caminhonete", 31.6f, -12.4f, 15f, 0.6f);

            // Pond in the quiet north-east corner, with a bench and a fishing rod.
            Place(CozyFarm + "pondscene.fbx", "Laguinho", 33.5f, 36.5f, 20f, 2.2f);
            Place(CozyFarm + "benchlittle.fbx", "Banco do Lago", 30f, 32f, 210f, 0.9f);
            Place(CozyFarm + "fishingrod.fbx", "Vara de Pesca", 31f, 33.5f, 200f, 2f, tilt: 8f);

            // Big round bales behind the pen.
            Place(CozyFarm + "haystackround.fbx", "Fardo Redondo 1", 31f, 25f, 10f, 1.8f);
            Place(CozyFarm + "haystackround.fbx", "Fardo Redondo 2", 35.5f, 25.3f, -15f, 1.8f);
            Place(CozyFarm + "haystackthick.fbx", "Monte de Feno", 29.2f, 27.6f, 40f, 1.6f);

            // Garden: watering cans on the path, a basket of produce at the harvest crate.
            Place(CozyFarm + "wateringcanwflower.fbx", "Regador", -12.6f, 33.7f, 120f, 0.45f);
            Place(CozyFarm + "wateringcan_001.fbx", "Regador 2", -25.4f, 32.4f, -40f, 0.45f);
            for (var i = 0; i < 3; i++)
            {
                Place(CozyFarm + "tomato.fbx", $"Tomate {i + 1}", -7.2f + i * 0.35f, 31.6f, i * 50f, 0.3f);
                Place(CozyFarm + "apple.fbx", $"Maca {i + 1}", -8.4f + i * 0.3f, 31.2f, i * 70f, 0.25f);
            }
            Place(CozyFarm + "woodenbox.fbx", "Caixote da Horta", -9.4f, 30.9f, 8f, 0.6f);

            // Coop eggs, buckets at the well and pen trough, mailbox at the entrance, campfire by the picnic.
            for (var i = 0; i < 3; i++) Place(CozyFarm + "egg.fbx", $"Ovo {i + 1}", -16.3f + i * 0.18f, 13.2f, i * 40f, 0.08f);
            Place(CozyFarm + "bucketwooden.fbx", "Balde do Poco", 19.4f, 10.2f, 0f, 0.35f);
            Place(CozyFarm + "bucketwooden.fbx", "Balde do Curral", 29.5f, 19.4f, 30f, 0.35f);
            Place(CozyFarm + "mailbox.fbx", "Caixa de Correio", 6f, -14f, 180f, 1.2f);
            Place(SurvivalKit + "campfire-pit.fbx", "Fogueira", -8.7f, 3.2f, 0f, 0.35f);
            Place(SurvivalKit + "tree-trunk.fbx", "Toco de Sentar", -9.8f, 1.9f, 30f, 0.45f);
            Place(CozyFarm + "flowerstack.fbx", "Vasos de Flores", -9.6f, 6.6f, 90f, 0.9f);
            Place(CozyFarm + "woodenmixer.fbx", "Batedeira de Manteiga", 21.2f, 45.4f, 90f, 1.2f);
            Place(CozyFarm + "woodenbox_001.fbx", "Pilha de Caixotes", -14f, -13.6f, 0f, 1.5f);
        }

        /// <summary>Trees frame the field; grass, flowers, bushes and stones fill the free ground.</summary>
        static void CreateNature(Transform root)
        {
            var random = new System.Random(7);
            var trees = new[] { "CommonTree_1.fbx", "CommonTree_2.fbx", "CommonTree_3.fbx", "CommonTree_4.fbx", "CommonTree_5.fbx", "Pine_1.fbx", "Pine_2.fbx", "Pine_3.fbx" };
            // Trees line the island rim (inside the fence), skipping the busy zones.
            var t = 0;
            var rim = new Rect(FieldIsland.xMin + 3.5f, FieldIsland.yMin + 3.5f, FieldIsland.width - 7f, FieldIsland.height - 7f);
            for (var s = 0f; s < 2f * (rim.width + rim.height); s += 6.5f)
            {
                var point = PointOnRectEdge(rim, s) + new Vector2(Jitter(random, 1.2f), Jitter(random, 1.2f));
                if (BusyGround.Any(zone => zone.Contains(point))) continue;
                PlaceNature(root, $"Arvore {++t}", trees[t % trees.Length], point, 6f, 9f, random);
            }

            var inner = new Rect(FieldIsland.xMin + 4f, FieldIsland.yMin + 4f, FieldIsland.width - 8f, FieldIsland.height - 8f);
            Scatter(root, "Arbusto", new[] { "Bush_Common.fbx", "Bush_Common_Flowers.fbx", "Plant_1.fbx", "Plant_7.fbx" }, 22, inner, 0.35f, 0.7f, random);
            Scatter(root, "Flores", new[] { "Flower_3_Group.fbx", "Flower_4_Group.fbx", "Flower_3_Single.fbx", "Flower_4_Single.fbx", "Clover_1.fbx", "Clover_2.fbx" }, 130, inner, 0.15f, 0.35f, random);
            Scatter(root, "Petalas", new[] { "Petal_1.fbx", "Petal_2.fbx", "Petal_3.fbx", "Petal_4.fbx", "Petal_5.fbx" }, 40, inner, 0.04f, 0.08f, random);
            Scatter(root, "Cogumelo", new[] { "Mushroom_Common.fbx", "Mushroom_Laetiporus.fbx" }, 14, inner, 0.2f, 0.35f, random);
            Scatter(root, "Capim", new[] { "Grass_Common_Tall.fbx", "Grass_Common_Short.fbx", "Grass_Wispy_Tall.fbx", "Grass_Wispy_Short.fbx", "Fern_1.fbx" }, 80, inner, 0.4f, 0.9f, random);
            Scatter(root, "Pedra", new[] { "Rock_Medium_1.fbx", "Rock_Medium_2.fbx", "Rock_Medium_3.fbx", "Pebble_Round_2.fbx", "Mushroom_Common.fbx" }, 12, inner, 0.3f, 1f, random);

            // Stepping stones from the road to the field gate and to the pen gate.
            for (var i = 0; i < 4; i++)
            {
                PlaceNature(root, $"Caminho da Horta {i + 1}", "RockPath_Round_Small_1.fbx", new Vector2(-5.5f - i * 1.4f, 33f), 0.1f, 0.1f, random);
                PlaceNature(root, $"Caminho do Curral {i + 1}", "RockPath_Round_Small_2.fbx", new Vector2(5.5f + i * 4.3f, 14f), 0.1f, 0.1f, random);
            }
        }

        /// <summary>Walks a rectangle's perimeter: distance along the edge to a point on it.</summary>
        static Vector2 PointOnRectEdge(Rect rect, float distance)
        {
            if (distance < rect.width) return new Vector2(rect.xMin + distance, rect.yMin);
            distance -= rect.width;
            if (distance < rect.height) return new Vector2(rect.xMax, rect.yMin + distance);
            distance -= rect.height;
            if (distance < rect.width) return new Vector2(rect.xMax - distance, rect.yMax);
            return new Vector2(rect.xMin, rect.yMax - (distance - rect.width));
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

        /// <summary>
        /// Fence from start along direction. The Quaternius fence is ~6 m long, so the line is split
        /// into whole segments, each stretched a little to end exactly at the corner.
        /// </summary>
        static void FenceLine(Transform root, string name, Vector3 start, Vector3 direction, float length)
        {
            var count = Mathf.Max(1, Mathf.RoundToInt(length / 6f));
            var segment = length / count;
            var rotation = Quaternion.FromToRotation(Vector3.right, direction);
            for (var i = 0; i < count; i++)
            {
                var center = start + direction * (segment * (i + 0.5f));
                var fence = CreateModel(FarmBuildings + (i % 2 == 0 ? "Fence.fbx" : "Fence2.fbx"), $"{name} {i + 1}", null,
                    center, Quaternion.identity, 1.2f, true);
                if (fence != null)
                    FitToBox(fence, center + Vector3.up * 0.6f, rotation, new Vector3(segment, 1.2f, 0.2f)).SetParent(root, true);
            }
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
            // The clips key the model root's own position, rotation and scale, so placement and
            // size live on a holder and the animation only ever moves the model inside it.
            var holder = new GameObject(name);
            holder.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, position.x * 37f + position.z * 11f, 0f));
            var animal = CreateModel(Animals + file, "Modelo", holder.transform, Vector3.zero, Quaternion.identity, height, false);
            if (animal == null)
            {
                Object.DestroyImmediate(holder);
                return;
            }
            var imported = AssetDatabase.LoadAssetAtPath<GameObject>(Animals + file).transform;
            holder.transform.localScale = Vector3.one * (animal.transform.localScale.x / Mathf.Abs(imported.localScale.x));
            animal.transform.localPosition = imported.localPosition;
            animal.transform.localRotation = imported.localRotation;
            animal.transform.localScale = imported.localScale;

            var animator = animal.GetComponent<Animator>();
            if (animator == null) animator = animal.AddComponent<Animator>();
            animator.runtimeAnimatorController = AnimalController(file, out var canWalk);
            animator.applyRootMotion = false;
            // Quaternius sheep, pig, llama and pug have no walk cycle: they graze in place
            // instead of sliding around in their idle pose.
            holder.AddComponent<AnimalWander>().Configure(position, canWalk ? radius : 0f, speed);
        }

        /// <summary>
        /// Idle/Walk controller for a Quaternius animal, switched by the "Moving" bool that
        /// AnimalWander sets. The FBX clips are imported once as loops.
        /// </summary>
        static RuntimeAnimatorController AnimalController(string file, out bool canWalk)
        {
            var modelPath = Animals + file;
            if (AssetImporter.GetAtPath(modelPath) is ModelImporter importer)
            {
                var clips = importer.clipAnimations.Length > 0 ? importer.clipAnimations : importer.defaultClipAnimations;
                if (clips.Any(clip => !clip.loopTime && (clip.name.EndsWith("Idle") || clip.name.Contains("Walk"))))
                {
                    foreach (var clip in clips)
                        if (clip.name.EndsWith("Idle") || clip.name.Contains("Walk"))
                            clip.loopTime = true;
                    importer.clipAnimations = clips;
                    importer.SaveAndReimport();
                }
            }

            var animations = AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<AnimationClip>()
                .Where(clip => !clip.name.StartsWith("__preview__")).ToArray();
            var idle = animations.FirstOrDefault(clip => clip.name.EndsWith("Idle"));
            var walk = animations.FirstOrDefault(clip => clip.name.EndsWith("WalkSlow")) ??
                       animations.FirstOrDefault(clip => clip.name.EndsWith("Walk"));
            canWalk = walk != null;

            // One controller per model, shared by every animal of that kind.
            const string folder = Root + "/Settings/Animals";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder(Root + "/Settings", "Animals");
            var path = $"{folder}/{System.IO.Path.GetFileNameWithoutExtension(file)}.controller";
            var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (existing != null) return existing;
            var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter("Moving", AnimatorControllerParameterType.Bool);
            var machine = controller.layers[0].stateMachine;
            var idleState = machine.AddState("Idle");
            idleState.motion = idle;
            machine.defaultState = idleState;
            if (canWalk)
            {
                var walkState = machine.AddState("Walk");
                walkState.motion = walk;
                var toWalk = idleState.AddTransition(walkState);
                toWalk.AddCondition(AnimatorConditionMode.If, 0f, "Moving");
                toWalk.hasExitTime = false;
                toWalk.duration = 0.2f;
                var toIdle = walkState.AddTransition(idleState);
                toIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "Moving");
                toIdle.hasExitTime = false;
                toIdle.duration = 0.25f;
            }
            return controller;
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
            var promptBackground = new GameObject("Prompt Background", typeof(RectTransform), typeof(Image));
            promptBackground.transform.SetParent(canvas.transform, false);
            var promptBackgroundRect = (RectTransform)promptBackground.transform;
            promptBackgroundRect.anchorMin = promptBackgroundRect.anchorMax = new Vector2(0.5f, 0.16f);
            promptBackgroundRect.sizeDelta = new Vector2(850f, 86f);
            promptBackground.GetComponent<Image>().color = new Color(0.025f, 0.02f, 0.015f, 0.78f);
            var prompt = CreateText(promptBackground.transform, "Prompt", string.Empty, 28, new Vector2(0.5f, 0.5f), new Vector2(810f, 70f), TextAnchor.MiddleCenter, Color.white);
            prompt.fontStyle = FontStyle.Bold;
            var promptOutline = prompt.gameObject.AddComponent<Outline>();
            promptOutline.effectColor = new Color(0f, 0f, 0f, 0.9f);
            promptOutline.effectDistance = new Vector2(1f, -1f);
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
            // The finger waits at the cottage door, just past where the last bridge lands.
            var route = CourseRoute();
            var landing = Last(route[route.Count - 1].points);
            var toHouse = (new Vector3(FinalCenter.x, landing.y, FinalCenter.y) - landing).normalized;
            goal.transform.position = landing + toHouse * 1.2f + Vector3.up * 1.1f;
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
            canvas.pixelPerfect = true;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
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
            text.alignByGeometry = true;
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

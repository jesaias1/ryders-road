using System;
using System.Linq;
using Avoidance.Core.Configuration;
using Avoidance.Core.Services;
using Avoidance.Gameplay.Camera;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Timing;
using Avoidance.Gameplay.Visuals;
using Avoidance.Input;
using Avoidance.SaveSystem;
using Avoidance.UI.Touch;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Avoidance.UI
{
    [DisallowMultipleComponent]
    public sealed class DevelopmentModuleSelector : MonoBehaviour
    {
        private static readonly float[] FieldOfViewSteps = { 78f, 82f, 86f, 90f, 94f };

        private void Awake()
        {
            CampaignFlowTrial.Clear();
            var configuration = Resources.Load<GameConfiguration>("FoundationGameConfiguration")
                ?? GameConfiguration.CreateRuntimeDefault();
            CreateEnvironment();
            CreateUi(configuration);
            UnitySceneLevelLoader.NotifyReady(gameObject.scene.name);
        }

        private void CreateEnvironment()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.34f, 0.38f, 0.82f);
            RenderSettings.fogDensity = 0.0038f;
            RenderSettings.ambientLight = new Color(0.62f, 0.66f, 0.86f);

            var cameraObject = new GameObject(
                "Selector Camera",
                typeof(UnityEngine.Camera),
                typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 9.2f, -20f);
            cameraObject.transform.rotation = Quaternion.Euler(24f, 0f, 0f);
            var camera = cameraObject.GetComponent<UnityEngine.Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.025f, 0.15f, 0.28f);
            camera.fieldOfView = 58f;

            var sunObject = new GameObject("Selector Sun", typeof(Light));
            sunObject.transform.rotation = Quaternion.Euler(48f, -34f, 0f);
            var sun = sunObject.GetComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.35f;
            sun.color = new Color(1f, 0.9f, 0.72f);
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.26f;

        }

        private static void CreateDecorativeBlock(
            string objectName,
            Vector3 position,
            Vector3 scale,
            Material material)
        {
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = objectName;
            block.transform.position = position;
            block.transform.localScale = scale;
            block.GetComponent<Renderer>().sharedMaterial = material;
            Destroy(block.GetComponent<Collider>());
        }

        private static void CreateSelectorBenchmark(string id, Vector3 position, float yaw)
        {
            var benchmark = new GameObject(id, typeof(VisualBenchmarkAnchor));
            benchmark.transform.position = position;
            benchmark.GetComponent<VisualBenchmarkAnchor>().Initialize(id, yaw);
        }

        private void CreateUi(GameConfiguration configuration)
        {
            ISaveService save = null; ISettingsService settings = null;
            GameServices.Current?.TryGet<ISaveService>(out save);
            GameServices.Current?.TryGet<ISettingsService>(out settings);
            var progression = save?.Current?.progression ?? new ProgressionData();
            ModuleProgressionData.EnsureArrays(progression);
            var canvasObject = new GameObject("Road Frontend", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = 0.5f;
            CreateTitleBackdrop(canvasObject.transform);
            var safe = CreateUiObject("Safe Area", canvasObject.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, typeof(SafeAreaFitter)).transform;
            var home = CreateUiObject("Home", safe, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var campaign = CreatePanel("Campaign Journey", safe, new Vector2(0.06f, 0.20f), new Vector2(0.94f, 0.80f), new Color(.018f,.047f,.085f,.96f));
            var settingsPanel = CreatePanel("Settings Panel", safe, new Vector2(0.29f, 0.12f), new Vector2(0.71f, 0.88f), new Color(.018f,.047f,.085f,.96f));
            campaign.SetActive(false); settingsPanel.SetActive(false);
            var homeGlass = CreatePanel("Home Glass", home.transform, new Vector2(.25f,.075f), new Vector2(.75f,.40f), new Color(.018f,.047f,.085f,.94f));
            CreatePanel("Home Accent",homeGlass.transform,new Vector2(0,.988f),Vector2.one,BrandPresentation.Gold);
            CreateText("Home Promise", homeGlass.transform, "FIND YOUR FLOW", 22, TextAnchor.MiddleCenter, new Vector2(.06f,.76f), new Vector2(.94f,.96f), Vector2.zero, Vector2.zero, Color.white);
            ProductionButton(homeGlass.transform, "CAMPAIGN", new Vector2(.06f,.34f), new Vector2(.94f,.61f), () => { home.SetActive(false); campaign.SetActive(true); }, true);
            ProductionButton(homeGlass.transform, "THE SPIRAL  /  CHALLENGE", new Vector2(.06f,.075f), new Vector2(.62f,.27f), () =>
            {
                ModuleSelectionState.Select(ModuleSelectionState.SpiralModuleId);
                StartCoroutine(LoadScene(ModuleSelectionState.ModuleRunnerSceneName));
            });
            ProductionButton(homeGlass.transform, "SETTINGS", new Vector2(.65f,.075f), new Vector2(.94f,.27f), () => { home.SetActive(false); settingsPanel.SetActive(true); });
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var development = CreatePanel("Development Tools", safe, new Vector2(.25f,.2f), new Vector2(.75f,.78f), BrandPresentation.PanelNavy);
            development.SetActive(false);
            CreateText("Development Header",development.transform,"DEVELOPMENT  /  MOVEMENT COMPARISONS",24,TextAnchor.MiddleCenter,new Vector2(.04f,.78f),new Vector2(.96f,.96f),Vector2.zero,Vector2.zero,Color.white);
            var trials = CampaignTrialMenu.Create(safe, () => development.SetActive(true), () =>
                StartCoroutine(LoadScene(ModuleSelectionState.ModuleRunnerSceneName)));
            ProductionButton(development.transform, "FLOW LAB  /  MOVEMENT PRACTICE", new Vector2(.08f,.5f), new Vector2(.92f,.7f), () =>
            {
                FlowLabSceneController.RequestLaunch();
                StartCoroutine(LoadScene("MovementLab"));
            });
            ProductionButton(development.transform, "CAMPAIGN FLOW TRIAL", new Vector2(.08f,.26f), new Vector2(.92f,.46f), () =>
            {
                development.SetActive(false); trials.SetActive(true);
            });
            ProductionButton(development.transform,"BACK",new Vector2(.08f,.04f),new Vector2(.35f,.2f),()=>{development.SetActive(false);home.SetActive(true);});
            ProductionButton(home.transform,"DEVELOPMENT",new Vector2(.025f,.02f),new Vector2(.16f,.065f),()=>{home.SetActive(false);development.SetActive(true);});
#endif
            var byId = Resources.LoadAll<ModuleDefinition>("Modules").ToDictionary(module => module.StableModuleId);
            var ids = ModuleSelectionState.GetCampaignModuleIds().Where(byId.ContainsKey).ToArray();
            var modules = ids.Select(id => byId[id]).ToArray();
            var nextId = ModuleProgressionData.GetContinueModuleId(progression, ids);
            var nextWorld = nextId != null && byId.TryGetValue(nextId, out var nextModule)
                ? nextModule.EnvironmentBiomeProfile?.DisplayName : null;
            CreateText("Home Journey", homeGlass.transform,
                nextWorld != null ? "NEXT ROAD  /  " + nextWorld.ToUpperInvariant() : "RETURN TO YOUR ROAD  /  REPLAY FOR MASTERY",
                16, TextAnchor.MiddleCenter, new Vector2(.06f,.63f), new Vector2(.94f,.78f), Vector2.zero, Vector2.zero, BrandPresentation.Cyan);
            CreateModulePanel(campaign.GetComponent<RectTransform>(), modules, ids, progression);
            CreateSettingsPanel(settingsPanel.GetComponent<RectTransform>(), settings);
            ProductionButton(campaign.transform, "BACK", new Vector2(0.025f, 0.025f), new Vector2(0.15f, 0.13f), () => { campaign.SetActive(false); home.SetActive(true); });
            ProductionButton(settingsPanel.transform, "BACK", new Vector2(0.05f, 0.015f), new Vector2(0.3f, 0.08f), () => { settingsPanel.SetActive(false); home.SetActive(true); });
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            CreateText("Development Version", safe, configuration.BuildVersion, 14, TextAnchor.LowerRight, new Vector2(0.5f, 0), new Vector2(0.98f, 0.05f), Vector2.zero, Vector2.zero, BrandPresentation.MutedWhite);
#endif
            if (FindAnyObjectByType<EventSystem>() == null) new GameObject("Event System", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        private void CreateModulePanel(RectTransform panel, ModuleDefinition[] modules, string[] orderedIds, ProgressionData progression)
        {
            var fixedCount = modules.Count(module => ModuleProgressionData.HasValidCompletion(ModuleProgressionData.GetRecord(progression, module.StableModuleId)));
            CreateText("Journey Header", panel, "THE ROAD AHEAD", 30, TextAnchor.MiddleLeft, new Vector2(0.025f, 0.82f), new Vector2(0.64f, 0.98f), Vector2.zero, Vector2.zero, Color.white);
            CreateText("Journey Progress", panel, $"{fixedCount} / {modules.Length} ROADS RESTORED", 20, TextAnchor.MiddleRight, new Vector2(0.64f, 0.84f), new Vector2(0.975f, 0.96f), Vector2.zero, Vector2.zero, BrandPresentation.Cyan);
            var pages = Mathf.Max(1, Mathf.CeilToInt(modules.Length / 3f));
            var page = 0;
            var cards = CreateUiObject("Journey Cards", panel, new Vector2(0.02f, 0.19f), new Vector2(0.98f, 0.81f), Vector2.zero, Vector2.zero);
            void RenderPage()
            {
                foreach (Transform child in cards.transform) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
                for (var column = 0; column < 3; column++)
                {
                    var index = page * 3 + column; if (index >= modules.Length) break;
                    var module = modules[index];
                    var unlocked = ModuleProgressionData.IsUnlocked(progression, orderedIds, module.StableModuleId);
                    var record = ModuleProgressionData.GetRecord(progression, module.StableModuleId);
                    var card = CreatePanel("Journey " + module.StableModuleId, cards.transform, new Vector2(column / 3f + 0.007f, 0), new Vector2((column + 1) / 3f - 0.007f, 1), new Color(.04f,.13f,.20f,.98f));
                    var world = module.EnvironmentBiomeProfile != null ? module.EnvironmentBiomeProfile.DisplayName : "Sky Road";
                    var title = module.DisplayName;
                    var separator = title.IndexOf(" - ", StringComparison.Ordinal); if (separator >= 0) title = title.Substring(separator + 3);
                    CreateText("Journey World", card.transform, $"{index + 1:00}   /   {world.ToUpperInvariant()}", 18, TextAnchor.MiddleLeft, new Vector2(0.07f, 0.79f), new Vector2(0.93f, 0.95f), Vector2.zero, Vector2.zero, BrandPresentation.Cyan);
                    CreateText("Journey Title", card.transform, title.ToUpperInvariant(), 28, TextAnchor.MiddleLeft, new Vector2(0.07f, 0.55f), new Vector2(0.93f, 0.79f), Vector2.zero, Vector2.zero, Color.white);
                    CreateText("Journey Record", card.transform, ModuleSummary(module, record, unlocked), 19, TextAnchor.UpperLeft, new Vector2(0.07f, 0.24f), new Vector2(0.93f, 0.52f), Vector2.zero, Vector2.zero, BrandPresentation.MutedWhite);
                    var label = unlocked ? (ModuleProgressionData.HasValidCompletion(record) ? "RUN AGAIN" : "ENTER ROAD") : "LOCKED";
                    var button = ProductionButton(card.transform, label, new Vector2(0.07f, 0.06f), new Vector2(0.93f, 0.24f), () =>
                    {
                        if (!unlocked) return;
                        ModuleSelectionState.Select(module.StableModuleId);
                        StartCoroutine(LoadScene(ModuleSelectionState.ModuleRunnerSceneName));
                    }, false);
                    button.interactable = unlocked;

                }
            }
            RenderPage();
            var continueId = ModuleProgressionData.GetContinueModuleId(progression, orderedIds);
            if (continueId != null)
                ProductionButton(panel, "CONTINUE", new Vector2(0.7f, 0.025f), new Vector2(0.975f, 0.15f), () =>
                {
                    if (!ModuleProgressionData.IsUnlocked(progression, orderedIds, continueId)) return;
                    ModuleSelectionState.Select(continueId);
                    StartCoroutine(LoadScene(ModuleSelectionState.ModuleRunnerSceneName));
                }, true);
            CreateText("Journey Rule", panel, continueId != null ? "Bronze opens the next road. Replay any open module." : "CAMPAIGN COMPLETE — replay for mastery.", 17, TextAnchor.MiddleCenter, new Vector2(0.16f, 0.02f), new Vector2(0.49f, 0.15f), Vector2.zero, Vector2.zero, BrandPresentation.MutedWhite);
            if (pages > 1) ProductionButton(panel, "MORE ROADS", new Vector2(0.50f, 0.025f), new Vector2(0.68f, 0.15f), () => { page = (page + 1) % pages; RenderPage(); });
        }

        private static Button ProductionButton(Transform parent, string label, Vector2 min, Vector2 max, UnityEngine.Events.UnityAction action, bool primary = false)
        {
            var obj = CreateUiObject(label + " Button", parent, min, max, Vector2.zero, Vector2.zero, typeof(Image), typeof(Button));
            obj.GetComponent<Image>().color = primary ? BrandPresentation.Gold : new Color(0.04f, 0.22f, 0.33f, 0.94f);
            var button = obj.GetComponent<Button>();
            button.onClick.AddListener(() => { UiAudio.Play(); action?.Invoke(); });
            var text = CreateText(label, obj.transform, label, 22, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(8, 0), new Vector2(-8, 0), primary ? BrandPresentation.DeepNavy : Color.white);
            text.resizeTextForBestFit = true; text.resizeTextMinSize = 13; text.resizeTextMaxSize = 22; text.raycastTarget = false;
            return button;
        }

        private static void CreateSettingsPanel(RectTransform panel, ISettingsService settings)
        {
            CreateText(
                "Settings Header",
                panel,
                "SETTINGS",
                30,
                TextAnchor.UpperLeft,
                new Vector2(0f, 0.88f),
                Vector2.one,
                new Vector2(28f, -28f),
                new Vector2(-28f, -18f),
                Color.white);

            var layout = Resources.Load<TouchControlLayout>("Touch_Default")
                ?? TouchControlLayout.CreateRuntimeDefault();
            Text controlText = null;
            Text steeringText = null;
            Text lookText = null;
            Text fovText = null;
            Text graphicsText = null;
            Text hapticsText = null;
            Text audioText = null;

            void Refresh()
            {
                var control = TouchInputCoordinator.LoadPreferredControlProfile(
                    layout.DefaultControlProfile);
                var steering = TouchInputCoordinator.LoadPreferredMovementSensitivity(
                    layout.DefaultMovementSensitivity);
                var fov = ResolveStoredFieldOfView();
                if (controlText != null)
                {
                    controlText.text = IsEasyControl(control) ? "CONTROL  EASY" : "CONTROL  CLASSIC";
                }

                if (steeringText != null)
                {
                    steeringText.text = "STEERING  " + steering.ToString().ToUpperInvariant();
                }

                if (lookText != null)
                {
                    lookText.text = "LOOK  " + LookLabel(settings);
                }

                if (fovText != null)
                {
                    fovText.text = "FOV  " + fov.ToString("0");
                }

                if (graphicsText != null)
                {
                    var names = QualitySettings.names;
                    var quality = names.Length == 0
                        ? "MOBILE"
                        : names[Mathf.Clamp(QualitySettings.GetQualityLevel(), 0, names.Length - 1)];
                    graphicsText.text = "GRAPHICS  " + quality.ToUpperInvariant();
                }

                if (hapticsText != null)
                {
                    hapticsText.text = PlayerPrefs.GetInt(TouchInputCoordinator.HapticsPreferenceKey, 1) != 0
                        ? "HAPTICS  ON"
                        : "HAPTICS  OFF";
                }

                if (audioText != null)
                {
                    var volume = Mathf.RoundToInt((settings?.Current.masterVolume ?? AudioListener.volume) * 100f);
                    audioText.text = "AUDIO  " + volume + "%";
                }
            }

            controlText = CreateButton(
                panel,
                "CONTROL",
                new Vector2(28f, -120f),
                new Vector2(330f, 50f),
                () =>
                {
                    var control = TouchInputCoordinator.LoadPreferredControlProfile(
                        layout.DefaultControlProfile);
                    TouchInputCoordinator.StorePreferredControlProfile(
                        IsEasyControl(control)
                            ? TouchControlProfileKind.LeftMoveRightLookTapJump
                            : TouchControlProfileKind.FlowSteerAutoBalanced);
                    Refresh();
                },
                BrandPresentation.DeepNavy,
                18);
            steeringText = CreateButton(
                panel,
                "STEERING",
                new Vector2(28f, -184f),
                new Vector2(330f, 50f),
                () =>
                {
                    var current = TouchInputCoordinator.LoadPreferredMovementSensitivity(
                        layout.DefaultMovementSensitivity);
                    TouchInputCoordinator.StorePreferredMovementSensitivity(
                        TouchControlRuntimeProfile.Next(current));
                    Refresh();
                },
                BrandPresentation.DeepNavy,
                18);
            lookText = CreateButton(
                panel,
                "LOOK",
                new Vector2(28f, -248f),
                new Vector2(330f, 50f),
                () =>
                {
                    if (settings != null)
                    {
                        settings.Current.lookSensitivity = NextLookSensitivity(settings.Current.lookSensitivity);
                        settings.Save();
                    }

                    Refresh();
                },
                BrandPresentation.DeepNavy,
                18);
            fovText = CreateButton(
                panel,
                "FOV",
                new Vector2(28f, -312f),
                new Vector2(330f, 50f),
                () =>
                {
                    PlayerPrefs.SetFloat(
                        FirstPersonCameraRig.FieldOfViewPreferenceKey,
                        NextFieldOfView(ResolveStoredFieldOfView()));
                    PlayerPrefs.Save();
                    Refresh();
                },
                BrandPresentation.DeepNavy,
                18);
            graphicsText = CreateButton(
                panel,
                "GRAPHICS",
                new Vector2(28f, -376f),
                new Vector2(330f, 50f),
                () =>
                {
                    var names = QualitySettings.names;
                    if (names.Length > 0)
                    {
                        QualitySettings.SetQualityLevel(
                            (QualitySettings.GetQualityLevel() + 1) % names.Length,
                            applyExpensiveChanges: true);
                    }

                    Refresh();
                },
                BrandPresentation.DeepNavy,
                18);
            hapticsText = CreateButton(
                panel,
                "HAPTICS",
                new Vector2(28f, -440f),
                new Vector2(330f, 50f),
                () =>
                {
                    var enabled = PlayerPrefs.GetInt(TouchInputCoordinator.HapticsPreferenceKey, 1) == 0;
                    PlayerPrefs.SetInt(TouchInputCoordinator.HapticsPreferenceKey, enabled ? 1 : 0);
                    PlayerPrefs.Save();
                    Refresh();
                },
                BrandPresentation.DeepNavy,
                18);
            audioText = CreateButton(
                panel,
                "AUDIO",
                new Vector2(28f, -504f),
                new Vector2(330f, 50f),
                () =>
                {
                    var current = settings?.Current.masterVolume ?? AudioListener.volume;
                    var next = current >= 0.95f ? 0.65f : current >= 0.8f ? 1f : 0.85f;
                    AudioListener.volume = next;
                    if (settings != null)
                    {
                        settings.Current.masterVolume = next;
                        settings.Save();
                    }

                    Refresh();
                },
                BrandPresentation.DeepNavy,
                18);
            CreateText(
                "Settings Note",
                panel,
                "Easy uses smart parkour camera. Classic keeps right-thumb manual look.",
                17,
                TextAnchor.UpperLeft,
                new Vector2(0f, 0f),
                new Vector2(1f, 0.24f),
                new Vector2(28f, 28f),
                new Vector2(-28f, -8f),
                BrandPresentation.MutedWhite);
            Refresh();
        }

        private static string ModuleSummary(
            ModuleDefinition module,
            ModuleProgressRecord record,
            bool unlocked)
        {
            if (!unlocked) return "Complete the previous module\nto open this road.";
            if (!ModuleProgressionData.HasValidCompletion(record)) return "OPEN TO EXPLORE\nPersonal best  --:--.---";
            var data = GameServices.Current != null && GameServices.Current.TryGet<ISaveService>(out var service)
                ? service.Current.progression : null;
            var current = ModuleProgressionData.GetVersionedBest(data, module.StableModuleId,
                module.ContentVersion, 1, module.RankThresholds.ThresholdVersion);
            if (current == null) return "ROAD RESTORED  /  HISTORY KEPT\nCurrent route PB  --:--.---";
            return current.highestRank.ToUpperInvariant() + "  /  CURRENT ROUTE\nPB  " + RunTimerFormatting.Format(current.bestTimeSeconds);
        }

        private static void CreateSummary(
            Transform parent,
            ModuleDefinition[] modules,
            ProgressionData progression)
        {
            var bronze = 0;
            var silver = 0;
            var gold = 0;
            var diamond = 0;
            foreach (var module in modules)
            {
                var record = ModuleProgressionData.GetRecord(progression, module.StableModuleId);
                if (!ModuleProgressionData.HasValidCompletion(record))
                {
                    continue;
                }

                switch (record.highestRank)
                {
                    case "Diamond":
                        diamond++;
                        break;
                    case "Gold":
                        gold++;
                        break;
                    case "Silver":
                        silver++;
                        break;
                    default:
                        bronze++;
                        break;
                }
            }

            CreateText(
                "Project Summary",
                parent,
                $"{modules.Length} CAMPAIGN LEVELS   DIAMOND {diamond}   GOLD {gold}   SILVER {silver}   BRONZE {bronze}",
                17,
                TextAnchor.UpperLeft,
                new Vector2(0f, 0.8f),
                Vector2.one,
                new Vector2(28f, -68f),
                new Vector2(-28f, -48f),
                BrandPresentation.MutedWhite);
        }

        private System.Collections.IEnumerator LoadScene(string sceneName)
        {
            if (GameServices.Current != null
                && GameServices.Current.TryGet<ILevelLoader>(out var loader))
            {
                yield return loader.LoadAsync(sceneName);
                yield break;
            }

            yield return new UnitySceneLevelLoader().LoadAsync(sceneName);
        }

        private static GameObject CreatePanel(
            string objectName,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Color color)
        {
            var panel = CreateUiObject(
                objectName,
                parent,
                anchorMin,
                anchorMax,
                Vector2.zero,
                Vector2.zero,
                typeof(Image));
            panel.GetComponent<Image>().color = color;
            return panel;
        }

        private static void CreateTitleBackdrop(Transform parent)
        {
            var texture = Resources.Load<Texture2D>("Loading/RydersRoad_LoadingPoster");
            if (texture == null)
            {
                return;
            }

            var backdropObject = CreateUiObject(
                "Title Sky Panorama",
                parent,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero,
                typeof(RawImage));
            var image = backdropObject.GetComponent<RawImage>();
            image.texture = texture;
            image.color = Color.white;
            image.raycastTarget = false;
            image.uvRect = new Rect(0f, 0f, 1f, 1f);
            var fit = backdropObject.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fit.aspectRatio = texture.width / (float)texture.height;

            var shadeObject = CreateUiObject(
                "Title Sky Readability Shade",
                parent,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero,
                typeof(Image));
            var shade = shadeObject.GetComponent<Image>();
            shade.color = new Color(0.02f, 0.08f, 0.14f, 0.12f);
            shade.raycastTarget = false;
        }

        private static void CreateLogo(
            Transform parent,
            string objectName,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            var sprite = BrandPresentation.LoadLogoSprite();
            if (sprite == null)
            {
                CreateText(
                    objectName,
                    parent,
                    BrandPresentation.PlayerFacingTitle,
                    58,
                    TextAnchor.MiddleLeft,
                    anchorMin,
                    anchorMax,
                    offsetMin,
                    offsetMax,
                    Color.white);
                return;
            }

            var imageObject = CreateUiObject(
                objectName,
                parent,
                anchorMin,
                anchorMax,
                Vector2.zero,
                Vector2.zero,
                typeof(Image));
            var rect = imageObject.GetComponent<RectTransform>();
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            var image = imageObject.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.color = Color.white;
            image.raycastTarget = false;
        }

        private static GameObject CreateUiObject(
            string objectName,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            params Type[] extraComponents)
        {
            var types = new System.Collections.Generic.List<Type> { typeof(RectTransform) };
            types.AddRange(extraComponents);
            var gameObject = new GameObject(objectName, types.ToArray());
            gameObject.transform.SetParent(parent, false);
            var rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            return gameObject;
        }

        private static Text CreateText(
            string objectName,
            Transform parent,
            string value,
            int fontSize,
            TextAnchor alignment,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax,
            Color color)
        {
            var textObject = CreateUiObject(
                objectName,
                parent,
                anchorMin,
                anchorMax,
                Vector2.zero,
                Vector2.zero,
                typeof(Text));
            var rect = textObject.GetComponent<RectTransform>();
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            var text = textObject.GetComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static Text CreateButton(
            Transform parent,
            string label,
            Vector2 anchoredPosition,
            Vector2 size,
            UnityEngine.Events.UnityAction action,
            Color color,
            int fontSize)
        {
            var buttonObject = CreateUiObject(
                label + " Button",
                parent,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                anchoredPosition,
                size,
                typeof(Image),
                typeof(Button));
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0f, 1f);
            var image = buttonObject.GetComponent<Image>();
            image.color = color;
            var button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(() => { UiAudio.Play(); action?.Invoke(); });
            var text = CreateText(
                label,
                buttonObject.transform,
                label,
                fontSize,
                TextAnchor.MiddleCenter,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero,
                Color.white);
            text.raycastTarget = false;
            return text;
        }

        private static bool IsEasyControl(TouchControlProfileKind control)
        {
            return control == TouchControlProfileKind.FlowSteerAutoBalanced
                || control == TouchControlProfileKind.FlowSteerAutoDirect
                || control == TouchControlProfileKind.FlowSteerAutoFlow;
        }

        private static string LookLabel(ISettingsService settings)
        {
            var value = settings?.Current.lookSensitivity ?? 0.5f;
            if (value <= 0.25f)
            {
                return "LOW";
            }

            if (value <= 0.55f)
            {
                return "MEDIUM";
            }

            if (value <= 0.82f)
            {
                return "FAST";
            }

            return "VERY FAST";
        }

        private static float NextLookSensitivity(float current)
        {
            if (current <= 0.25f)
            {
                return 0.45f;
            }

            if (current <= 0.55f)
            {
                return 0.7f;
            }

            if (current <= 0.82f)
            {
                return 0.95f;
            }

            return 0.15f;
        }

        private static float ResolveStoredFieldOfView()
        {
            return Mathf.Clamp(
                PlayerPrefs.GetFloat(
                    FirstPersonCameraRig.FieldOfViewPreferenceKey,
                    FirstPersonCameraRig.DefaultPreferredFieldOfView),
                FirstPersonCameraRig.MinimumPreferredFieldOfView,
                FirstPersonCameraRig.MaximumPreferredFieldOfView);
        }

        private static float NextFieldOfView(float current)
        {
            for (var index = 0; index < FieldOfViewSteps.Length; index++)
            {
                if (current < FieldOfViewSteps[index] - 0.1f)
                {
                    return FieldOfViewSteps[index];
                }
            }

            return FieldOfViewSteps[0];
        }
    }
}

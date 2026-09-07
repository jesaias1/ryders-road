using Avoidance.Core.Services;
using Avoidance.Gameplay.Visuals;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Avoidance.UI
{
    [DisallowMultipleComponent]
    public sealed class FoundationTestSceneController : MonoBehaviour
    {
        private static readonly Color SkyColor = new Color(0.18f, 0.56f, 0.9f);
        private static readonly Color PlatformColor = new Color(0.16f, 0.8f, 0.52f);

        private void Awake()
        {
            EnsureCamera();
            CreateLighting();
            CreateEnvironment();
            CreateSafeAreaDemonstration();
        }

        private static void EnsureCamera()
        {
            if (Camera.main != null)
            {
                return;
            }

            var cameraObject = new GameObject("Foundation Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetPositionAndRotation(new Vector3(0f, 3.2f, -7f), Quaternion.Euler(14f, 0f, 0f));
            var camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = SkyColor;
        }

        private static void CreateLighting()
        {
            if (FindAnyObjectByType<Light>() != null)
            {
                return;
            }

            var lightObject = new GameObject("Foundation Sun", typeof(Light));
            lightObject.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
            var light = lightObject.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.25f;
        }

        private static void CreateEnvironment()
        {
            var platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = "Floating Foundation Platform";
            platform.transform.position = Vector3.zero;
            platform.transform.localScale = new Vector3(8f, 0.75f, 8f);
            platform.GetComponent<Renderer>().sharedMaterial =
                VisualMaterialUtility.CreateRuntimeMaterial(
                    "RB Foundation Platform",
                    PlatformColor);

            var voidMarker = GameObject.CreatePrimitive(PrimitiveType.Quad);
            voidMarker.name = "Visible Void Marker";
            voidMarker.transform.SetPositionAndRotation(new Vector3(0f, -8f, 5f), Quaternion.Euler(90f, 0f, 0f));
            voidMarker.transform.localScale = Vector3.one * 30f;
            voidMarker.GetComponent<Renderer>().sharedMaterial =
                VisualMaterialUtility.CreateRuntimeMaterial(
                    "RB Foundation Void",
                    new Color(0.025f, 0.04f, 0.1f));
        }

        private void CreateSafeAreaDemonstration()
        {
            if (FindAnyObjectByType<Canvas>() != null)
            {
                return;
            }

            var canvasObject = new GameObject(
                "Foundation UI",
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var safeArea = new GameObject(
                "Safe Area",
                typeof(RectTransform),
                typeof(Image),
                typeof(SafeAreaFitter));
            safeArea.transform.SetParent(canvasObject.transform, false);
            safeArea.GetComponent<Image>().color = new Color(0.1f, 0.95f, 1f, 0.08f);

            var buttonObject = new GameObject(
                "Reload Through Scene Service",
                typeof(RectTransform),
                typeof(Image),
                typeof(Button));
            buttonObject.transform.SetParent(safeArea.transform, false);
            var buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(1f, 0f);
            buttonRect.anchorMax = new Vector2(1f, 0f);
            buttonRect.pivot = new Vector2(1f, 0f);
            buttonRect.anchoredPosition = new Vector2(-32f, 32f);
            buttonRect.sizeDelta = new Vector2(360f, 96f);
            buttonObject.GetComponent<Image>().color = new Color(0.05f, 0.16f, 0.26f, 0.82f);
            buttonObject.GetComponent<Button>().onClick.AddListener(ReloadFoundationScene);

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(buttonObject.transform, false);
            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            var text = labelObject.GetComponent<Text>();
            text.text = "RELOAD THROUGH SERVICE";
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 28;
            text.color = Color.white;

            if (FindAnyObjectByType<EventSystem>() == null)
            {
                new GameObject("Event System", typeof(EventSystem), typeof(InputSystemUIInputModule));
            }
        }

        private void ReloadFoundationScene()
        {
            if (GameServices.Current != null
                && GameServices.Current.TryGet<ILevelLoader>(out var loader))
            {
                StartCoroutine(loader.LoadAsync("FoundationTest"));
                return;
            }

            SceneManager.LoadScene("FoundationTest");
        }
    }
}

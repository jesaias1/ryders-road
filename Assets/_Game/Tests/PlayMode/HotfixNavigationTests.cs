using System.Collections;
using System.IO;
using System.Linq;
using Avoidance.Bootstrap;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Player;
using Avoidance.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Avoidance.Tests.PlayMode
{
    public sealed class HotfixNavigationTests
    {
        [UnityTest]
        public IEnumerator MenuSpiralButtonActivatesGameplayAndDismissesOverlay()
        {
            yield return Boot();
            Button("THE SPIRAL  /  CHALLENGE Button").onClick.Invoke();
            yield return Gameplay("module.004.the-spiral");
        }
        [UnityTest]
        public IEnumerator MenuCampaignButtonActivatesGameplayAndDismissesOverlay()
        {
            yield return Boot();
            Button("CAMPAIGN Button").onClick.Invoke();
            var card = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)
                .Single(x => x.name == "Journey module.001.first-steps");
            card.GetComponentsInChildren<Button>().Single(x => x.interactable).onClick.Invoke();
            yield return Gameplay("module.001.first-steps");
        }
        [UnityTest]
        public IEnumerator PatchResultsRetryAndNextUsePersistentTransition()
        {
            yield return Boot();
            Button("CAMPAIGN Button").onClick.Invoke();
            var card = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Single(x => x.name == "Journey module.001.first-steps");
            card.GetComponentsInChildren<Button>().Single(x => x.interactable).onClick.Invoke();
            yield return Gameplay("module.001.first-steps");
            Assert.That(Object.FindAnyObjectByType<PatchBlock>().TryComplete(Object.FindAnyObjectByType<ParkourMotor>()), Is.True);
            yield return new WaitForSecondsRealtime(.3f);
            Button("RETRY Button").onClick.Invoke();
            yield return Gameplay("module.001.first-steps");
            Assert.That(Object.FindAnyObjectByType<PatchBlock>().TryComplete(Object.FindAnyObjectByType<ParkourMotor>()), Is.True);
            yield return new WaitForSecondsRealtime(.3f);
            Button("NEXT MODULE Button").onClick.Invoke();
            yield return Gameplay("module.002.moving-parts");
        }
        [UnityTest]
        public IEnumerator UnavailableVideoDoesNotBlockActualSpiralButton()
        {
            yield return Boot();
            Assert.That(Object.FindAnyObjectByType<LoadingPresentation>().GetComponent<VideoPlayer>(), Is.Null);
            Button("THE SPIRAL  /  CHALLENGE Button").onClick.Invoke();
            yield return Gameplay("module.004.the-spiral");
        }
        [UnityTest]
        public IEnumerator MissingSceneFailsVisiblyAndReturnsToMenu()
        {
            yield return Boot();
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("\\[RoadTransition\\].*Scene is not included"));
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("\\[RoadLoading\\] Transition failed"));
            yield return new UnitySceneLevelLoader().LoadAsync("MissingScene092");
            Assert.That(UnitySceneLevelLoader.ActiveRequest.State, Is.EqualTo(SceneLoadState.Failed));
            Assert.That(Object.FindAnyObjectByType<LoadingPresentation>().GetComponent<CanvasGroup>().alpha, Is.EqualTo(1));
            Button("RETURN TO MENU Button").onClick.Invoke();
            var deadline = Time.realtimeSinceStartup + 8;
            while (!UnitySceneLevelLoader.ActiveRequest.IsTerminal && Time.realtimeSinceStartup < deadline) yield return null;
            yield return new WaitForSecondsRealtime(.3f);
            Assert.That(UnitySceneLevelLoader.ActiveRequest.State, Is.EqualTo(SceneLoadState.Ready));
            Assert.That(Object.FindAnyObjectByType<LoadingPresentation>().GetComponent<CanvasGroup>().blocksRaycasts, Is.False);
        }
        [UnityTest]
        public IEnumerator FullViewportMenuCampaignVideoAndModule003()
        {
            yield return Boot();
            yield return CaptureProduct("menu");
            Button("CAMPAIGN Button").onClick.Invoke(); yield return null;
            yield return CaptureProduct("campaign");
            Assert.That(Object.FindAnyObjectByType<LoadingPresentation>().GetComponent<VideoPlayer>(), Is.Null);
            Assert.That(Resources.Load<Texture2D>("Branding/Jesaias_Emblem"), Is.Not.Null);
            var card = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Single(x => x.name == "Journey module.001.first-steps");
            card.GetComponentsInChildren<Button>().Single(x => x.interactable).onClick.Invoke();
            CaptureNow("loading");
            yield return Gameplay("module.001.first-steps");
            foreach(var destination in new[]{"module.002.moving-parts","module.003.flow-error"})
            {
                Assert.That(Object.FindAnyObjectByType<PatchBlock>().TryComplete(Object.FindAnyObjectByType<ParkourMotor>()),Is.True);
                yield return new WaitForSecondsRealtime(.3f);Button("NEXT MODULE Button").onClick.Invoke();yield return Gameplay(destination);
            }
            yield return CaptureProduct("module003");
        }
        [UnityTest]
        public IEnumerator ModuleReadinessTimeoutShowsRecoveryInsteadOfEndlessPoster()
        {
            yield return Boot();
            var profile = Resources.Load<LoadingTransitionProfile>("LoadingTransitionProfile");
            var previous = profile.SceneTimeoutSeconds; profile.SceneTimeoutSeconds = 1;
            System.Action mismatch = () => ModuleSelectionState.Select("module.004.the-spiral");
            UnitySceneLevelLoader.LoadingStarted += mismatch;
            try
            {
                ModuleSelectionState.Select("module.001.first-steps");
                LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("\\[RoadTransition\\].*initialization did not report ready"));
                LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("\\[RoadLoading\\] Transition failed"));
                yield return new UnitySceneLevelLoader().LoadAsync("ModuleRunner");
                Assert.That(UnitySceneLevelLoader.ActiveRequest.State, Is.EqualTo(SceneLoadState.Failed));
                Assert.That(UnitySceneLevelLoader.ActiveRequest.ActivationComplete, Is.True);
                Assert.That(Button("RETURN TO MENU Button").isActiveAndEnabled, Is.True);
            }
            finally { profile.SceneTimeoutSeconds = previous; UnitySceneLevelLoader.LoadingStarted -= mismatch; }
        }
        [UnityTest]
        public IEnumerator ThrowingPresentationListenerCannotBlockCampaignLoad()
        {
            yield return Boot();
            System.Action broken = () => throw new System.InvalidOperationException("Injected presentation failure");
            UnitySceneLevelLoader.LoadingStarted += broken;
            try
            {
                Button("CAMPAIGN Button").onClick.Invoke();
                var card = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Single(x => x.name == "Journey module.001.first-steps");
                LogAssert.Expect(LogType.Warning, "[RoadTransition] Injected presentation failure");
                card.GetComponentsInChildren<Button>().Single(x => x.interactable).onClick.Invoke();
                yield return Gameplay("module.001.first-steps");
            }
            finally { UnitySceneLevelLoader.LoadingStarted -= broken; }
        }
        private static IEnumerator CaptureProduct(string name)
        {
            yield return null;
            CaptureNow(name);
        }
        private static void CaptureNow(string name)
        {
            Directory.CreateDirectory("Logs/Phase092VisualQA");
            var camera = Camera.main;
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(canvas => canvas.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
            var render = new RenderTexture(1560, 720, 24);
            var texture = new Texture2D(1560, 720, TextureFormat.RGB24, false);
            var previous = RenderTexture.active;
            var aspect = camera.aspect;
            try
            {
                camera.targetTexture = render; camera.aspect = 1560f / 720;
                foreach (var canvas in canvases) { canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1f; }
                Canvas.ForceUpdateCanvases();
                Assert.That(camera.rect, Is.EqualTo(new Rect(0, 0, 1, 1)), "Full gameplay camera viewport");
                foreach (var canvas in canvases)
                foreach (var fit in canvas.GetComponentsInChildren<AspectRatioFitter>())
                {
                    if (fit.GetComponent<RawImage>() == null) continue;
                    Assert.That(fit.aspectMode, Is.EqualTo(AspectRatioFitter.AspectMode.EnvelopeParent));
                    var root = (RectTransform)canvas.transform;
                    var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(root, fit.transform);
                    Assert.That(bounds.min.x, Is.LessThanOrEqualTo(root.rect.xMin + 1));
                    Assert.That(bounds.max.x, Is.GreaterThanOrEqualTo(root.rect.xMax - 1));
                    Assert.That(bounds.min.y, Is.LessThanOrEqualTo(root.rect.yMin + 1));
                    Assert.That(bounds.max.y, Is.GreaterThanOrEqualTo(root.rect.yMax - 1));
                }
                camera.Render(); RenderTexture.active = render;
                texture.ReadPixels(new Rect(0, 0, 1560, 720), 0, 0); texture.Apply();
                File.WriteAllBytes("Logs/Phase092VisualQA/" + name + ".png", texture.EncodeToPNG());
            }
            finally
            {
                foreach (var canvas in canvases) { canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null; }
                camera.targetTexture = null; camera.aspect = aspect; RenderTexture.active = previous;
                Object.Destroy(render); Object.Destroy(texture);
            }
        }

        private static Button Button(string name) => Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(x => x.name == name);
        private static IEnumerator Boot()
        {
            var bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
            if (bootstrap != null) { Object.Destroy(bootstrap.gameObject); yield return null; }
            yield return SceneManager.LoadSceneAsync("Bootstrap");
            var deadline = Time.realtimeSinceStartup + 10;
            while (Object.FindAnyObjectByType<DevelopmentModuleSelector>() == null && Time.realtimeSinceStartup < deadline) yield return null;
            yield return new WaitForSecondsRealtime(0.8f);
            Assert.That(Object.FindAnyObjectByType<DevelopmentModuleSelector>(), Is.Not.Null);
        }
        private static IEnumerator Gameplay(string id)
        {
            var deadline = Time.realtimeSinceStartup + 8;
            var overlay = Object.FindAnyObjectByType<LoadingPresentation>();
            while (Time.realtimeSinceStartup < deadline)
            {
                var controller = Object.FindAnyObjectByType<ModuleSceneController>();
                if (controller != null && controller.ActiveModule.StableModuleId == id && overlay.GetComponent<CanvasGroup>().alpha == 0) break;
                yield return null;
            }
            var module = Object.FindAnyObjectByType<ModuleSceneController>();
            var state = $"scene={SceneManager.GetActiveScene().name} module={module?.ActiveModule?.StableModuleId} overlay={overlay.GetComponent<CanvasGroup>().alpha}";
            Directory.CreateDirectory("Logs/Phase092VisualQA");
            File.AppendAllText("Logs/Phase092VisualQA/navigation-trace.txt", id + " | " + state + "\n");
            Assert.That(module, Is.Not.Null, state);
            Assert.That(module.ActiveModule.StableModuleId, Is.EqualTo(id), state);
            Assert.That(overlay.GetComponent<CanvasGroup>().alpha, Is.Zero, state);
            Assert.That(overlay.GetComponent<CanvasGroup>().blocksRaycasts, Is.False, state);
            Assert.That(Object.FindAnyObjectByType<PlayerRuntimeCoordinator>().enabled, Is.True, state);
        }
    }
}

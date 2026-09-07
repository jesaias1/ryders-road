using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Avoidance.Gameplay.Levels
{
    // Only owns scene transitions; no movement, gameplay, saves or presentation dependency.
    public sealed class SceneTransitionHost : MonoBehaviour
    {
        private static SceneTransitionHost _instance;
        public static SceneTransitionHost Instance
        {
            get
            {
                if (_instance == null) _instance = new GameObject("Scene Transition Host").AddComponent<SceneTransitionHost>();
                return _instance;
            }
        }
        private AsyncOperation _operation;
        private LoadingTransitionProfile _profile;
        private void Awake()
        {
            _instance = this; DontDestroyOnLoad(gameObject);
            _profile = Resources.Load<LoadingTransitionProfile>("LoadingTransitionProfile");
        }
        public SceneLoadRequest Begin(string sceneName)
        {
            if (UnitySceneLevelLoader.ActiveRequest != null && !UnitySceneLevelLoader.ActiveRequest.IsTerminal)
                return UnitySceneLevelLoader.ActiveRequest; // Double taps cannot launch competing loads.
            if (_operation != null && !_operation.isDone)
            {
                UnitySceneLevelLoader.PublishFailure("The previous scene operation has not finished. Please restart the app.");
                return new SceneLoadRequest { SceneName = sceneName, State = SceneLoadState.Failed, Error = "Previous operation pending" };
            }
            var request = new SceneLoadRequest { SceneName = sceneName, ModuleId = sceneName == ModuleSelectionState.ModuleRunnerSceneName ? ModuleSelectionState.SelectedModuleId : null };
            UnitySceneLevelLoader.ActiveRequest = request;
            StartCoroutine(Load(request));
            return request;
        }
        private IEnumerator Load(SceneLoadRequest request)
        {
            UnitySceneLevelLoader.PublishStarted();
            yield return null; // Present a frame without waiting for any media callback.
            if (!Application.CanStreamedLevelBeLoaded(request.SceneName))
            {
                Fail(request, "Scene is not included in this build"); yield break;
            }
            request.State = SceneLoadState.Loading;
            string startError = null;
            try { _operation = SceneManager.LoadSceneAsync(request.SceneName, LoadSceneMode.Single); }
            catch (System.Exception exception) { startError = exception.Message; }
            if (startError != null || _operation == null) { Fail(request, startError ?? "No async scene operation"); yield break; }
            // Default automatic activation stays enabled. Media never owns allowSceneActivation.
            var deadline = Time.realtimeSinceStartup + (_profile != null ? _profile.SceneTimeoutSeconds : 20);
            while (!_operation.isDone)
            {
                request.Progress = _operation.progress;
                if (Time.realtimeSinceStartup >= deadline) { Fail(request, "Scene activation timeout"); yield break; }
                yield return null;
            }
            request.Progress = 1; request.ActivationComplete = true; request.State = SceneLoadState.AwaitingReady;
            var needsReady = request.SceneName == ModuleSelectionState.ModuleRunnerSceneName || request.SceneName == ModuleSelectionState.ModuleSelectorSceneName;
            if (!needsReady) request.ModuleReady = true;
            while (!request.ModuleReady)
            {
                if (Time.realtimeSinceStartup >= deadline) { Fail(request, "Scene activated but module initialization did not report ready"); yield break; }
                yield return null;
            }
            yield return null; // Camera/UI gets a normal frame after initialization.
            request.State = SceneLoadState.Ready;
            UnitySceneLevelLoader.PublishFinished();
        }
        private static void Fail(SceneLoadRequest request, string reason)
        {
            request.Error = reason; request.State = SceneLoadState.Failed;
            UnitySceneLevelLoader.Warn(request.ToString());
            UnitySceneLevelLoader.PublishFailure(reason);
            UnitySceneLevelLoader.PublishFinished();
        }
        private void OnDestroy() { if (_instance == this) _instance = null; }
    }
}

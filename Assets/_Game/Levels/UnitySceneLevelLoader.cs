using System.Collections;
using Avoidance.Core.Services;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace Avoidance.Gameplay.Levels
{
    public sealed class UnitySceneLevelLoader : ILevelLoader
    {
        public static event System.Action LoadingStarted;
        public static event System.Action LoadingFinished;
        public static event System.Action<string> LoadingFailed;
        public static SceneLoadRequest ActiveRequest { get; internal set; }
        public string ActiveSceneName => SceneManager.GetActiveScene().name;

        public IEnumerator LoadAsync(string sceneName)
        {
            // The caller can be destroyed by scene activation. It only observes the request;
            // the persistent host owns activation, readiness, failure and completion.
            var request = SceneTransitionHost.Instance.Begin(sceneName);
            while (!request.IsTerminal) yield return null;
        }

        public static void NotifyReady(string sceneName, string moduleId = null)
        {
            var request = ActiveRequest;
            if (request == null || request.IsTerminal || request.SceneName != sceneName) return;
            if (sceneName == ModuleSelectionState.ModuleRunnerSceneName && request.ModuleId != moduleId) return;
            request.ModuleReady = true;
        }
        internal static void PublishStarted() => Publish(LoadingStarted);
        internal static void PublishFinished() => Publish(LoadingFinished);
        internal static void PublishFailure(string message)
        {
            if (LoadingFailed == null) return;
            foreach (System.Action<string> listener in LoadingFailed.GetInvocationList())
                try { listener(message); } catch (System.Exception exception) { Warn(exception.Message); }
        }
        private static void Publish(System.Action action)
        {
            if (action == null) return;
            foreach (System.Action listener in action.GetInvocationList())
                try { listener(); } catch (System.Exception exception) { Warn(exception.Message); }
        }
        internal static void Warn(string message)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning("[RoadTransition] " + message);
#endif
        }
    }
}

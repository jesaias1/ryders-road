using System.Collections;
using Avoidance.Core.Services;
using UnityEngine.SceneManagement;

namespace Avoidance.Gameplay.Levels
{
    public sealed class UnitySceneLevelLoader : ILevelLoader
    {
        public string ActiveSceneName => SceneManager.GetActiveScene().name;

        public IEnumerator LoadAsync(string sceneName)
        {
            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            while (operation != null && !operation.isDone)
            {
                yield return null;
            }
        }
    }
}

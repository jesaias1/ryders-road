using System.Collections;
using Avoidance.Bootstrap;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Avoidance.Tests.PlayMode
{
    public sealed class BootstrapPlayModeTests
    {
        [UnityTest]
        public IEnumerator Bootstrap_LoadsFoundationTestScene()
        {
            SceneManager.LoadScene("Bootstrap");
            var timeout = Time.realtimeSinceStartup + 10f;

            while (SceneManager.GetActiveScene().name != "FoundationTest"
                && Time.realtimeSinceStartup < timeout)
            {
                yield return null;
            }

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("FoundationTest"));
            var bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
            Assert.That(bootstrap, Is.Not.Null);
            Assert.That(bootstrap.IsInitialized, Is.True);
        }
    }
}

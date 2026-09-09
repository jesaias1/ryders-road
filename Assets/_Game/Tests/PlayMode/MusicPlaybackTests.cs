using System.Collections;
using Avoidance.Core.Services;
using Avoidance.Gameplay.Audio;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Avoidance.Tests.PlayMode
{
    public sealed class MusicPlaybackTests
    {
        [UnityTest] public IEnumerator StreamingLoopVolumeRetryAndInterruptedTransitions()
        {
            var previous=GameServices.Current;var services=new ServiceContainer();
            var settings=new PlayerPrefsSettingsService();settings.Load();services.Register<ISettingsService>(settings);
            GameServices.Publish(services);
            try
            {
                MusicDirector.SetContext("menu");yield return new WaitForSecondsRealtime(2.2f);
                var director=MusicDirector.Current;var menu=director.CurrentClip;
                Assert.That(menu,Is.Not.Null);Assert.That(menu.loadType,Is.EqualTo(AudioClipLoadType.Streaming));
                var sources=director.GetComponents<AudioSource>();
                var active=System.Array.Find(sources,s=>s.clip==menu);
                Assert.That(active.loop,Is.True);Assert.That(active.isPlaying,Is.True);
                float time=active.time;MusicDirector.SetContext("menu");yield return null;
                Assert.That(active.time,Is.GreaterThanOrEqualTo(time),"Retry cannot restart the song");
                settings.Current.musicVolume=0;yield return null;
                foreach(var source in sources)Assert.That(source.volume,Is.Zero);
                settings.Current.musicVolume=.65f;yield return null;
                Assert.That(active.volume,Is.GreaterThan(.1f));
                active.time=menu.length-.15f;yield return new WaitForSecondsRealtime(.4f);
                Assert.That(active.isPlaying,Is.True);Assert.That(active.time,Is.LessThan(1),"Actual loop crossed its boundary");
                MusicDirector.SetContext("default");yield return new WaitForSecondsRealtime(.3f);
                MusicDirector.SetContext("practice");yield return new WaitForSecondsRealtime(.2f);
                MusicDirector.SetContext("menu");yield return new WaitForSecondsRealtime(2.2f);
                Assert.That(director.CurrentClip,Is.EqualTo(menu));
                Assert.That(System.Array.FindAll(sources,s=>s.isPlaying).Length,Is.EqualTo(1));
                director.SendMessage("OnApplicationPause",true);yield return null;
                Assert.That(System.Array.Exists(sources,s=>s.isPlaying),Is.False);
                director.SendMessage("OnApplicationPause",false);yield return null;
                Assert.That(System.Array.Exists(sources,s=>s.isPlaying),Is.True);
            }
            finally
            {
                if(MusicDirector.Current!=null)Object.Destroy(MusicDirector.Current.gameObject);
                if(previous!=null)GameServices.Publish(previous);else GameServices.Clear(services);
            }
        }
    }
}

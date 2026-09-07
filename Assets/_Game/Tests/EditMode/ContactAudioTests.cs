using Avoidance.Gameplay.Audio;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Avoidance.Tests.EditMode
{
    public sealed class ContactAudioTests
    {
        [TestCase(GameplayAudioCue.Jump)]
        [TestCase(GameplayAudioCue.Landing)]
        [TestCase(GameplayAudioCue.HardLanding)]
        [TestCase(GameplayAudioCue.CrumbleWarning)]
        [TestCase(GameplayAudioCue.CrumbleCollapse)]
        [TestCase(GameplayAudioCue.Ui)]
        public void ContactCuesHaveShortMonoFoleyWithHeadroom(GameplayAudioCue cue)
        {
            var profile = Resources.Load<GameplayAudioProfile>("GameplayAudioProfile");
            Assert.That(profile.TryGet(cue, out var clip, out var volume), Is.True);
            Assert.That(clip.channels, Is.EqualTo(1));
            Assert.That(clip.length, Is.InRange(.005f, 1.1f));
            Assert.That(volume, Is.InRange(.1f, .5f));
            clip.LoadAudioData();
            var samples = new float[clip.samples];
            Assert.That(clip.GetData(samples, 0), Is.True);
            float peak = 0;
            foreach(float sample in samples) peak = Mathf.Max(peak, Mathf.Abs(sample));
            Assert.That(peak * volume, Is.InRange(.001f, .5f));
        }

        [Test]
        public void LoopChangesClipAndMissingCueOrProfileStopsPreviousSound()
        {
            var root = new GameObject("Loop contract");
            var profile = ScriptableObject.CreateInstance<GameplayAudioProfile>();
            var first = AudioClip.Create("first", 100, 1, 44100, false);
            var second = AudioClip.Create("second", 100, 1, 44100, false);
            try
            {
                var serialized = new SerializedObject(profile);
                var entries = serialized.FindProperty("_entries");entries.arraySize = 2;
                for(int i = 0; i < 2; i++)
                {
                    var entry = entries.GetArrayElementAtIndex(i);
                    entry.FindPropertyRelative("Cue").intValue = (int)(i == 0 ? GameplayAudioCue.Wind : GameplayAudioCue.Water);
                    entry.FindPropertyRelative("Clip").objectReferenceValue = i == 0 ? first : second;
                    entry.FindPropertyRelative("Volume").floatValue = .3f;
                }
                serialized.ApplyModifiedPropertiesWithoutUndo();
                var feedback = root.AddComponent<MovementFeedback>();feedback.Configure(profile);
                feedback.SetLoop(GameplayAudioCue.Wind, 1);
                var loop = System.Array.Find(root.GetComponents<AudioSource>(), source => source.loop);
                Assert.That(loop.clip, Is.SameAs(first));
                feedback.SetLoop(GameplayAudioCue.Water, 1, true);
                Assert.That(loop.clip, Is.SameAs(second));Assert.That(loop.spatialBlend, Is.EqualTo(1));
                int missingIntents = 0;
                feedback.CueRequested += cue => { if(cue == GameplayAudioCue.BiomeAmbience) missingIntents++; };
                feedback.SetLoop(GameplayAudioCue.BiomeAmbience, 1);
                feedback.SetLoop(GameplayAudioCue.BiomeAmbience, 1);
                Assert.That(missingIntents, Is.EqualTo(1));
                Assert.That(loop.isPlaying, Is.False);Assert.That(loop.volume, Is.Zero);
                feedback.SetLoop(GameplayAudioCue.Wind, 1);
                feedback.Configure(null);
                Assert.That(loop.isPlaying, Is.False);Assert.That(loop.volume, Is.Zero);
            }
            finally { Object.DestroyImmediate(root);Object.DestroyImmediate(profile);Object.DestroyImmediate(first);Object.DestroyImmediate(second); }
        }
    }
}

using Avoidance.Gameplay.Levels;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

namespace Avoidance.Tests.EditMode
{
    public sealed class Loading092Tests
    {
        [Test]
        public void PackagedMediaAndBoundedTransitionProfileExist()
        {
            var clip = Resources.Load<VideoClip>("Loading/RydersRoad_Loading");
            Assert.That(clip, Is.Not.Null);
            Assert.That(clip.width, Is.EqualTo(1280)); Assert.That(clip.height, Is.EqualTo(720));
            Assert.That(clip.frameCount, Is.GreaterThan(240));
            Assert.That(Resources.Load<Texture2D>("Loading/RydersRoad_LoadingPoster"), Is.Not.Null);
            var profile = Resources.Load<LoadingTransitionProfile>("LoadingTransitionProfile");
            Assert.That(profile, Is.Not.Null);
            Assert.That(profile.SceneTimeoutSeconds, Is.InRange(5, 30));
            Assert.That(profile.MediaTimeoutSeconds, Is.InRange(.1f, 3));
            Assert.That(profile.RevealSeconds, Is.InRange(.01f, .3f));
        }
        [Test]
        public void ColdStartUsesApprovedNativePosterBeforeManagedLoading()
        {
            Assert.That(PlayerSettings.SplashScreen.show, Is.True);
            Assert.That(PlayerSettings.SplashScreen.showUnityLogo, Is.False);
            Assert.That(PlayerSettings.SplashScreen.background, Is.Not.Null);
            Assert.That(AssetDatabase.GetAssetPath(PlayerSettings.SplashScreen.background),
                Is.EqualTo("Assets/Branding/Android/RydersRoad_Startup.jpg"));
            Assert.That(PlayerSettings.SplashScreen.blurBackgroundImage, Is.False);
            var importer = (TextureImporter)AssetImporter.GetAtPath("Assets/Branding/Android/RydersRoad_Startup.jpg");
            Assert.That(importer.isReadable, Is.True, "Android native splash export needs CPU texture access");
            Assert.That(importer.textureCompression, Is.EqualTo(TextureImporterCompression.Uncompressed));
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            Assert.That(settings.FindProperty("androidSplashScreen").objectReferenceValue, Is.Not.Null);
        }
        [Test]
        public void FullscreenAllowsBothLandscapesAndRendersBehindSafeArea()
        {
            Assert.That(PlayerSettings.defaultInterfaceOrientation, Is.EqualTo(UIOrientation.AutoRotation));
            Assert.That(PlayerSettings.allowedAutorotateToLandscapeLeft, Is.True);
            Assert.That(PlayerSettings.allowedAutorotateToLandscapeRight, Is.True);
            Assert.That(PlayerSettings.allowedAutorotateToPortrait, Is.False);
            Assert.That(PlayerSettings.allowedAutorotateToPortraitUpsideDown, Is.False);
            Assert.That(PlayerSettings.Android.renderOutsideSafeArea, Is.True);
            Assert.That(PlayerSettings.fullScreenMode, Is.EqualTo(FullScreenMode.FullScreenWindow));
        }
    }
}

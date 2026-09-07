using System;
using Avoidance.Core.Services;
using UnityEngine;

namespace Avoidance.Core.Utilities
{
    public sealed class AndroidDisplayService : IPlatformDisplayService
    {
        private bool _immersiveFailureLogged;

        public bool ImmersiveRequested { get; private set; }
        public bool HasFocus { get; private set; } = true;
        public int AndroidApiLevel { get; private set; } = -1;
        public Rect SafeArea => Screen.safeArea;

        public string DisplaySummary =>
            $"{Screen.width}x{Screen.height} safe {SafeArea.x:0},{SafeArea.y:0} {SafeArea.width:0}x{SafeArea.height:0} fullscreen {Screen.fullScreen} api {AndroidApiLevel}";

        public void RequestImmersiveMode()
        {
            ImmersiveRequested = true;
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            Screen.fullScreen = true;

#if UNITY_ANDROID && !UNITY_EDITOR
            using var version = new AndroidJavaClass("android.os.Build$VERSION");
            AndroidApiLevel = version.GetStatic<int>("SDK_INT");

            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            if (activity == null)
            {
                return;
            }

            activity.Call(
                "runOnUiThread",
                new AndroidJavaRunnable(() =>
                {
                    try
                    {
                        using (activity)
                        {
                            using var window = activity.Call<AndroidJavaObject>("getWindow");
                            if (window == null)
                            {
                                return;
                            }

                            ApplyWindowFlags(window);
                            TryApplyModernInsets(window);
                            ApplyLegacyImmersive(window);
                        }
                    }
                    catch (Exception exception) when (
                        exception is AndroidJavaException
                        || exception is NullReferenceException
                        || exception is InvalidOperationException)
                    {
                        LogImmersiveFailure(exception);
                    }
                }));
#else
            AndroidApiLevel = -1;
#endif
        }

        public void SetFocusState(bool focused)
        {
            HasFocus = focused;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private void LogImmersiveFailure(Exception exception)
        {
            if (_immersiveFailureLogged)
            {
                return;
            }

            _immersiveFailureLogged = true;
            Debug.LogWarning(
                $"Android immersive mode request was skipped; Unity will retry on focus/resume. {exception.Message}");
        }

        private void ApplyWindowFlags(AndroidJavaObject window)
        {
            const int flagFullscreen = 1024;
            const int flagTranslucentStatus = 67108864;
            const int flagTranslucentNavigation = 134217728;
            const int flagDrawsSystemBarBackgrounds = int.MinValue;
            window.Call("clearFlags", flagTranslucentStatus | flagTranslucentNavigation);
            window.Call("addFlags", flagFullscreen | flagDrawsSystemBarBackgrounds);

            if (AndroidApiLevel >= 21)
            {
                const int transparent = 0;
                window.Call("setStatusBarColor", transparent);
                window.Call("setNavigationBarColor", transparent);
            }

            if (AndroidApiLevel < 28)
            {
                return;
            }

            using var attributes = window.Call<AndroidJavaObject>("getAttributes");
            if (attributes == null)
            {
                return;
            }

            attributes.Set("layoutInDisplayCutoutMode", 1);
            window.Call("setAttributes", attributes);
        }

        private bool TryApplyModernInsets(AndroidJavaObject window)
        {
            if (AndroidApiLevel < 30)
            {
                return false;
            }

            try
            {
                window.Call("setDecorFitsSystemWindows", false);
                using var controller = window.Call<AndroidJavaObject>("getInsetsController");
                if (controller == null)
                {
                    return false;
                }

                using var type = new AndroidJavaClass("android.view.WindowInsets$Type");
                var bars = type.CallStatic<int>("statusBars")
                    | type.CallStatic<int>("navigationBars");
                controller.Call("hide", bars);
                controller.Call("setSystemBarsBehavior", 2);
                return true;
            }
            catch (Exception exception) when (
                exception is AndroidJavaException
                || exception is NullReferenceException
                || exception is InvalidOperationException)
            {
                Debug.LogWarning($"Modern Android immersive mode failed; using legacy fallback. {exception.Message}");
                return false;
            }
        }

        private static void ApplyLegacyImmersive(AndroidJavaObject window)
        {
            using var decorView = window.Call<AndroidJavaObject>("getDecorView");
            if (decorView == null)
            {
                return;
            }

            const int systemUiFlagHideNavigation = 2;
            const int systemUiFlagFullscreen = 4;
            const int systemUiFlagLayoutStable = 256;
            const int systemUiFlagLayoutHideNavigation = 512;
            const int systemUiFlagLayoutFullscreen = 1024;
            const int systemUiFlagImmersiveSticky = 4096;
            decorView.Call(
                "setSystemUiVisibility",
                systemUiFlagHideNavigation
                | systemUiFlagFullscreen
                | systemUiFlagLayoutStable
                | systemUiFlagLayoutHideNavigation
                | systemUiFlagLayoutFullscreen
                | systemUiFlagImmersiveSticky);
        }
#endif
    }

    public static class AndroidImmersiveMode
    {
        public static void Apply()
        {
            new AndroidDisplayService().RequestImmersiveMode();
        }
    }
}

using System.Collections;
using Avoidance.Gameplay.Levels;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Avoidance.UI
{
    public sealed class LoadingPresentation : MonoBehaviour
    {
        private CanvasGroup _group;
        private RawImage _image;
        private Texture2D _poster;
        private VideoPlayer _video;
        private RenderTexture _target;
        private Coroutine _fade;
        private LoadingTransitionProfile _profile;
        private GameObject _failure;
        private Text _failureText;
        private bool _showing, _failedMedia;
        private float _lastFrameAt;
        private long _lastFrame = -1;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            if (FindAnyObjectByType<LoadingPresentation>() == null)
                new GameObject("Road Loading Transition").AddComponent<LoadingPresentation>();
        }
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            _profile = Resources.Load<LoadingTransitionProfile>("LoadingTransitionProfile");
            var canvas = gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 1000;
            gameObject.AddComponent<GraphicRaycaster>();
            _group = gameObject.AddComponent<CanvasGroup>(); _group.alpha = 0; _group.blocksRaycasts = false;
            var back = new GameObject("Loading Blue", typeof(RectTransform), typeof(Image)); back.transform.SetParent(transform, false);
            Stretch(back.GetComponent<RectTransform>()); back.GetComponent<Image>().color = new Color(0.025f, 0.15f, 0.28f);
            var frame = new GameObject("Supplied Loading Film", typeof(RectTransform), typeof(RawImage), typeof(AspectRatioFitter)); frame.transform.SetParent(transform, false);
            Stretch(frame.GetComponent<RectTransform>());
            var fit = frame.GetComponent<AspectRatioFitter>(); fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent; fit.aspectRatio = 16f / 9f;
            _image = frame.GetComponent<RawImage>(); _poster = Resources.Load<Texture2D>("Loading/RydersRoad_LoadingPoster"); _image.texture = _poster;
            _target = new RenderTexture(1280, 720, 0, RenderTextureFormat.ARGB32) { name = "Road Loading Video Target" }; _target.Create();
            _video = gameObject.AddComponent<VideoPlayer>(); _video.playOnAwake = false; _video.isLooping = true;
            _video.source = VideoSource.VideoClip; _video.clip = Resources.Load<VideoClip>("Loading/RydersRoad_Loading");
            _video.renderMode = VideoRenderMode.RenderTexture; _video.targetTexture = _target;
            _video.audioOutputMode = VideoAudioOutputMode.None; _video.timeUpdateMode = VideoTimeUpdateMode.DSPTime;
            _video.waitForFirstFrame = true;
            _video.prepareCompleted += Ready; _video.errorReceived += VideoError;
            CreateFailurePanel();
            UnitySceneLevelLoader.LoadingStarted += Begin;
            UnitySceneLevelLoader.LoadingFinished += Finish;
            UnitySceneLevelLoader.LoadingFailed += Failed;
            // Cover the first managed frame too, before Bootstrap publishes a load event.
            // Native startup uses the same approved poster through PlayerSettings.
            Begin();
        }
        private void CreateFailurePanel()
        {
            _failure = new GameObject("Loading Recovery", typeof(RectTransform), typeof(Image)); _failure.transform.SetParent(transform, false);
            var rect = _failure.GetComponent<RectTransform>(); rect.anchorMin = new Vector2(.2f, .3f); rect.anchorMax = new Vector2(.8f, .7f); rect.offsetMin = rect.offsetMax = Vector2.zero;
            _failure.GetComponent<Image>().color = new Color(.02f, .08f, .15f, .97f);
            var message = new GameObject("Loading Error", typeof(RectTransform), typeof(Text)); message.transform.SetParent(rect, false); Stretch(message.GetComponent<RectTransform>());
            _failureText = message.GetComponent<Text>(); _failureText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); _failureText.fontSize = 24; _failureText.alignment = TextAnchor.UpperCenter; _failureText.color = Color.white;
            var button = new GameObject("RETURN TO MENU Button", typeof(RectTransform), typeof(Image), typeof(Button)); button.transform.SetParent(rect, false);
            var br = button.GetComponent<RectTransform>(); br.anchorMin = new Vector2(.15f, .08f); br.anchorMax = new Vector2(.85f, .4f); br.offsetMin = br.offsetMax = Vector2.zero;
            button.GetComponent<Image>().color = new Color(.05f, .4f, .5f);
            var label = new GameObject("Label", typeof(RectTransform), typeof(Text)); label.transform.SetParent(br, false); Stretch(label.GetComponent<RectTransform>());
            var text = label.GetComponent<Text>(); text.font = _failureText.font; text.fontSize = 24; text.alignment = TextAnchor.MiddleCenter; text.text = "RETURN TO MENU";
            button.GetComponent<Button>().onClick.AddListener(() => SceneTransitionHost.Instance.Begin(ModuleSelectionState.ModuleSelectorSceneName));
            _failure.SetActive(false);
        }
        private static void Stretch(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; }
        private void Begin()
        {
            if (_fade != null) StopCoroutine(_fade); _fade = null;
            _failure.SetActive(false); _showing = true; _failedMedia = false; _lastFrame = -1; _lastFrameAt = Time.realtimeSinceStartup;
            _group.alpha = 1; _group.blocksRaycasts = true; _image.texture = _poster;
            if (_video.clip == null || !_video.isActiveAndEnabled) { MediaFallback("Video unavailable"); return; }
            if (_video.isPrepared) _video.Play(); else _video.Prepare();
        }
        private void Ready(VideoPlayer player) { if (_showing && !_failedMedia) player.Play(); }
        private void Update()
        {
            if (!_showing) return;
            var request = UnitySceneLevelLoader.ActiveRequest;
            // Terminal state is authoritative even if a presentation callback was missed.
            if (request != null && request.IsTerminal) { if (request.State == SceneLoadState.Failed) Failed(request.Error); else Finish(); return; }
            if (_failedMedia) return;
            if (_video.isPrepared && _video.frame >= 0 && _video.frame != _lastFrame)
            {
                _lastFrame = _video.frame; _lastFrameAt = Time.realtimeSinceStartup; _image.texture = _target;
            }
            if (Time.realtimeSinceStartup - _lastFrameAt > (_profile != null ? _profile.MediaTimeoutSeconds : 2)) MediaFallback("No new decoded video frame before watchdog deadline");
        }
        private void VideoError(VideoPlayer player, string error) { MediaFallback(error); }
        private void MediaFallback(string reason)
        {
            _failedMedia = true; _image.texture = _poster;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning($"[RoadLoading] Poster fallback: {reason}; source={_video.source} prepared={_video.isPrepared} playing={_video.isPlaying} frame={_video.frame}; {UnitySceneLevelLoader.ActiveRequest}");
#endif
            _video.Stop();
        }
        private void Failed(string reason)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning($"[RoadLoading] Transition failed: {reason}; services={Avoidance.Core.Services.GameServices.Current != null} activeScene={UnityEngine.SceneManagement.SceneManager.GetActiveScene().name} overlay={_group.alpha} videoPrepared={_video.isPrepared} videoPlaying={_video.isPlaying} videoFrame={_video.frame}; {UnitySceneLevelLoader.ActiveRequest}");
#endif
            if (_fade != null) StopCoroutine(_fade); _fade = null; _showing = false;
            _group.alpha = 1; _group.blocksRaycasts = true; _image.texture = _poster; _video.Pause();
            _failureText.text = "This road could not open. Try returning to the menu.\nIf loading still fails, restart the app.";
            _failure.SetActive(true);
        }
        private void Finish()
        {
            if (_failure.activeSelf || _fade != null) return;
            _showing = false; _fade = StartCoroutine(Reveal());
        }
        private IEnumerator Reveal()
        {
            var duration = _profile != null ? _profile.RevealSeconds : .18f;
            while (_group.alpha > 0) { _group.alpha = Mathf.MoveTowards(_group.alpha, 0, Time.unscaledDeltaTime / Mathf.Max(.01f, duration)); if (_group.alpha > 0) yield return null; }
            _group.blocksRaycasts = false; _video.Pause(); _image.texture = _poster; _fade = null;
        }
        private void OnDestroy()
        {
            UnitySceneLevelLoader.LoadingStarted -= Begin; UnitySceneLevelLoader.LoadingFinished -= Finish; UnitySceneLevelLoader.LoadingFailed -= Failed;
            if (_video != null) { _video.prepareCompleted -= Ready; _video.errorReceived -= VideoError; _video.targetTexture = null; }
            if (_target != null) { _target.Release(); Destroy(_target); }
        }
    }
}

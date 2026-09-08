using System.Collections;
using Avoidance.Gameplay.Levels;
using UnityEngine;
using UnityEngine.UI;


namespace Avoidance.UI
{
    public sealed class LoadingPresentation : MonoBehaviour
    {
        private CanvasGroup _group;
        private RawImage _image;
        private Texture2D _poster;
        private RectTransform _emblem;
        private RectTransform _track;
        private RectTransform _traveller;
        private Text _loadingLabel;

        private Coroutine _fade;
        private LoadingTransitionProfile _profile;
        private GameObject _failure;
        private Text _failureText;
        private bool _showing;


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
            var back = new GameObject("Loading Background", typeof(RectTransform), typeof(Image)); back.transform.SetParent(transform, false);
            Stretch(back.GetComponent<RectTransform>()); back.GetComponent<Image>().color = new Color(5/255f,14/255f,24/255f);
            var frame = new GameObject("Ryders Road Loading Identity", typeof(RectTransform), typeof(RawImage)); frame.transform.SetParent(transform, false);
            _image = frame.GetComponent<RawImage>(); _poster = Resources.Load<Texture2D>(BrandPresentation.LogoResourcePath); _image.texture = _poster;
            _image.enabled = _poster != null; _image.raycastTarget = false;
            _emblem = frame.GetComponent<RectTransform>();
            _emblem.anchorMin = _emblem.anchorMax = new Vector2(.5f,.5f);
            LayoutEmblem();
            if (_poster == null)
            {
                var fallback = new GameObject("Game fallback", typeof(RectTransform), typeof(Text)); fallback.transform.SetParent(transform,false);
                Stretch(fallback.GetComponent<RectTransform>()); var text=fallback.GetComponent<Text>();
                text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.text=BrandPresentation.PlayerFacingTitle;
                text.alignment=TextAnchor.MiddleCenter; text.fontSize=24; text.color=new Color(.7f,.85f,.86f);
            }
            var track = new GameObject("Loading Track",typeof(RectTransform),typeof(Image));track.transform.SetParent(transform,false);
            _track=track.GetComponent<RectTransform>();_track.anchorMin=_track.anchorMax=new Vector2(.5f,.5f);
            track.GetComponent<Image>().color=new Color(.12f,.23f,.29f);track.GetComponent<Image>().raycastTarget=false;
            var traveller=new GameObject("Loading Traveller",typeof(RectTransform),typeof(Image));traveller.transform.SetParent(_track,false);
            _traveller=traveller.GetComponent<RectTransform>();_traveller.anchorMin=_traveller.anchorMax=new Vector2(0,.5f);
            traveller.GetComponent<Image>().color=BrandPresentation.Gold;traveller.GetComponent<Image>().raycastTarget=false;
            var label=new GameObject("Loading Label",typeof(RectTransform),typeof(Text));label.transform.SetParent(transform,false);
            _loadingLabel=label.GetComponent<Text>();_loadingLabel.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _loadingLabel.fontSize=16;_loadingLabel.alignment=TextAnchor.MiddleCenter;_loadingLabel.color=BrandPresentation.MutedWhite;
            _loadingLabel.text="OPENING YOUR ROAD";_loadingLabel.raycastTarget=false;
            LayoutEmblem();
            CreateFailurePanel();
            UnitySceneLevelLoader.LoadingStarted += Begin;
            UnitySceneLevelLoader.LoadingFinished += Finish;
            UnitySceneLevelLoader.LoadingFailed += Failed;
            // Cover the first managed frame too, before Bootstrap publishes a load event.
            // Studio identity belongs to native startup; managed transitions use the game identity.
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
            _failure.SetActive(false); _showing = true;
            _group.alpha = 1; _group.blocksRaycasts = true; _image.texture = _poster;
        }
        private void LayoutEmblem()
        {
            var safe=Screen.safeArea;
            float width=Mathf.Min(safe.width*.40f,safe.height*.85f);
            _emblem.sizeDelta=new Vector2(width,width*(_poster != null ? _poster.height/(float)_poster.width : .4f));
            _emblem.anchoredPosition=safe.center-new Vector2(Screen.width,Screen.height)*.5f + Vector2.up*safe.height*.05f;
            if(_track==null)return;
            _track.sizeDelta=new Vector2(width*.58f,3);_track.anchoredPosition=_emblem.anchoredPosition-Vector2.up*(_emblem.sizeDelta.y*.5f+28);
            _traveller.sizeDelta=new Vector2(width*.12f,3);
            var label=_loadingLabel.rectTransform;label.anchorMin=label.anchorMax=new Vector2(.5f,.5f);
            label.sizeDelta=new Vector2(width,30);label.anchoredPosition=_track.anchoredPosition-Vector2.up*25;
        }
        private void Update()
        {
            if (!_showing) return;
            var request = UnitySceneLevelLoader.ActiveRequest;
            // Terminal state is authoritative even if a presentation callback was missed.
            if (request != null && request.IsTerminal) { if (request.State == SceneLoadState.Failed) Failed(request.Error); else Finish(); return; }
            LayoutEmblem();
            float period=_profile != null ? _profile.PulsePeriodSeconds : 2.4f;
            float alpha=Mathf.Lerp(.78f,1f,.5f+.5f*Mathf.Cos(Time.unscaledTime*2*Mathf.PI/Mathf.Max(.1f,period)));
            _image.color=new Color(1,1,1,alpha);
            float travel=Mathf.PingPong(Time.unscaledTime/Mathf.Max(.1f,period)*2,1);
            _traveller.anchoredPosition=new Vector2(Mathf.Lerp(_traveller.sizeDelta.x*.5f,_track.sizeDelta.x-_traveller.sizeDelta.x*.5f,travel),0);
            _loadingLabel.text=request != null && request.State==SceneLoadState.AwaitingReady ? "BUILDING YOUR ROAD" : "OPENING YOUR ROAD";
        }
        private void Failed(string reason)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning($"[RoadLoading] Transition failed: {reason}; services={Avoidance.Core.Services.GameServices.Current != null} activeScene={UnityEngine.SceneManagement.SceneManager.GetActiveScene().name} overlay={_group.alpha}; {UnitySceneLevelLoader.ActiveRequest}");
#endif
            if (_fade != null) StopCoroutine(_fade); _fade = null; _showing = false;
            _group.alpha = 1; _group.blocksRaycasts = true; _image.texture = _poster;
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
            _group.blocksRaycasts = false; _image.texture = _poster; _fade = null;
        }
        private void OnDestroy()
        {
            UnitySceneLevelLoader.LoadingStarted -= Begin; UnitySceneLevelLoader.LoadingFinished -= Finish; UnitySceneLevelLoader.LoadingFailed -= Failed;


        }
    }
}

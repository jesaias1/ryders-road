using System;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Player;
using Avoidance.Gameplay.Ranking;
using Avoidance.Gameplay.Respawn;
using Avoidance.Gameplay.Timing;
using Avoidance.Gameplay.Visuals;
using Avoidance.SaveSystem;
using UnityEngine;
using UnityEngine.UI;

namespace Avoidance.UI
{
    [DisallowMultipleComponent]
    public sealed class ModuleHud : MonoBehaviour
    {
        private static readonly ModuleRank[] DisplayRanks =
        {
            ModuleRank.Bronze,
            ModuleRank.Silver,
            ModuleRank.Gold,
            ModuleRank.Diamond
        };

        private Text _status;
        private Text _flowText;
        private float _flowVisible;
        private int _flowCount, _flowTotal;
        
        private Text _completion;
        private Text _splitText;
        private RectTransform _resultsPanel;
        private Text _resultsText;
        private Image _resultsAccent;
        private Image[] _rankBadges = Array.Empty<Image>();
        private Text[] _rankLabels = Array.Empty<Text>();
        private ModuleDefinition _module;
        private PlayerRuntimeCoordinator _player;
        private RestoreController _restore;
        private ModuleAttemptTelemetry _telemetry;
        private ModuleRunSession _run;
        private ModuleProgressRecord _progress;
        private ModuleVisualProfile _visuals;
        private string _buildVersion;
        private Action _retry;
        private Action _nextModule;
        private Action _moduleSelect;
        private float _splitVisibleSeconds;

        public void Initialize(
            Text status,
            Text completion,
            ModuleDefinition module,
            PlayerRuntimeCoordinator player,
            RestoreController restore,
            ModuleAttemptTelemetry telemetry,
            ModuleRunSession run,
            ModuleProgressRecord progress,
            string buildVersion,
            Action retry,
            Action nextModule,
            Action moduleSelect)
        {
            _status = status;
            _completion = completion;
            _module = module;
            _player = player;
            _restore = restore;
            _telemetry = telemetry;
            _run = run;
            _progress = progress;
            _visuals = module.VisualProfile != null
                ? module.VisualProfile
                : ModuleVisualProfile.CreateRuntimeDefault();
            _buildVersion = buildVersion;
            _retry = retry;
            _nextModule = nextModule;
            _moduleSelect = moduleSelect;
            _status.supportRichText = true;
            CreateSplitText();
            _flowText = CreateText("Flow Discovery", _status.transform.parent, "", 22, TextAnchor.UpperCenter, new Vector2(0.36f, 0.71f), new Vector2(0.64f, 0.78f), Vector2.zero, Vector2.zero);
            _flowText.raycastTarget = false; _flowText.enabled = false;
            CreateRankStrip();
            if (CampaignFlowTrial.Active)
                foreach (var badge in _rankBadges) if (badge != null) badge.transform.parent.gameObject.SetActive(false);
            CreateResultsPanel();
        }

        private void Update()
        {
            if (_status == null || _player == null || _module == null || _run == null)
            {
                return;
            }

            if (_flowText != null) { _flowVisible -= Time.unscaledDeltaTime; _flowText.enabled = _flowVisible > 0; }
            var elapsed = _run.Timer.ElapsedSeconds;
            var target = ModuleRankUtility.CurrentTarget(elapsed, _module.RankThresholds);
            var targetSeconds = ModuleRankUtility.ThresholdFor(target, _module.RankThresholds);
            var best = _progress != null && _progress.bestTimeSeconds > 0d
                ? $"PB {RunTimerFormatting.Format(_progress.bestTimeSeconds)}"
                : "PB --:--.---";
            var targetLine = target == ModuleRank.Bronze || targetSeconds <= 0d
                ? "BRONZE AVAILABLE"
                : $"{ModuleRankUtility.Display(target)} {RunTimerFormatting.Format(targetSeconds)}";

            _status.text =
                $"<size=34><color=#F0FBFF>{RunTimerFormatting.Format(elapsed)}</color></size>\n" +
                $"<size=18>{targetLine}   <color=#BCDDE8>{best}</color></size>";
            _status.color = _visuals.RankColor(target);
            if (CampaignFlowTrial.Active)
            {
                _status.text = $"<size=27>{RunTimerFormatting.Format(elapsed)}  ·  {CampaignFlowTrial.Label}</size>\n"
                    + $"<size=18>{_player.Motor.HorizontalSpeed:0.0} m/s  ·  PEAK {_player.Motor.PeakHorizontalSpeed:0.0}  ·  JUMPS {_player.Motor.JumpCount}  ·  SESSION BEST {CampaignFlowTrial.Best(_module):0.00}s</size>";
                _status.color = Color.white;
            }
            RefreshRankStrip(target);

            if (_splitText != null && _splitText.enabled)
            {
                _splitVisibleSeconds -= Time.unscaledDeltaTime;
                if (_splitVisibleSeconds <= 0f)
                {
                    _splitText.enabled = false;
                }
            }
        }

        public void ShowFlowPickup(int count, int total)
        {
            _flowCount = count; _flowTotal = total;
            if (_flowText == null) return;
            _flowText.text = count == total ? "ALL FLOW SHARDS FOUND" : $"FLOW SHARD  {count} / {total}";
            _flowText.color = BrandPresentation.Cyan; _flowVisible = 1.15f;
        }

        public void ShowSplit(RunSplit split)
        {
            if (_splitText == null || split == null)
            {
                return;
            }

            var delta = split.hasPersonalBestComparison
                ? $"\n{(split.pbDeltaSeconds <= 0d ? "-" : "+")}{Math.Abs(split.pbDeltaSeconds):0.00}s"
                : string.Empty;
            _splitText.text = split.hasPersonalBestComparison ? $"{(split.pbDeltaSeconds <= 0 ? "AHEAD OF PB" : "PB SPLIT")}  {delta.Trim()}" : "RESTORE POINT ACTIVE";
            _splitText.enabled = true;
            _splitText.color = _visuals.ColorFor(ModuleMaterialRole.WaterFoam);
            _splitVisibleSeconds = 2.1f;
        }

        public void ShowResults(ModuleRunResult result)
        {
            if (_completion != null)
            {
                _completion.enabled = false;
            }

            if (_resultsPanel == null || _resultsText == null || result == null)
            {
                return;
            }

            var challenge = FindAnyObjectByType<FlowChallenge>();
            var flow = challenge != null && challenge.Total > 0 ? $"\n<size=18><color=#71DBEF>FLOW SHARDS  {challenge.Count} / {challenge.Total}  /  {100 * challenge.Count / challenge.Total}%</color></size>" : "";
            _resultsText.text = FormatResults(result) + flow;
            if (CampaignFlowTrial.Active)
                _resultsText.text = $"<size=24>{CampaignFlowTrial.Label}</size>\n<size=36>TRIAL COMPLETE</size>\n"
                    + $"{RunTimerFormatting.Format(result.CompletionSeconds)}\nSESSION BEST {CampaignFlowTrial.Best(_module):0.00}s"
                    + "\n<size=20>CAMPAIGN RECORDS UNCHANGED\nCompare another mode from the home trial menu</size>" + flow;
            _resultsText.color = _visuals.RankColor(result.Rank);
            if (_resultsAccent != null)
            {
                _resultsAccent.color = _visuals.RankColor(result.Rank);
            }

            _resultsPanel.gameObject.SetActive(true);
            if (_status != null) _status.enabled = false;
            if (_player != null)
            {
                _player.Input?.ResetState(); _player.enabled = false;
                var momentum = _player.GetComponent<Avoidance.Gameplay.Audio.MomentumPresentation>();
                if (momentum != null) momentum.enabled = false;
            }
            if (_restore != null) _restore.enabled = false;
            foreach (var badge in _rankBadges) if (badge != null) badge.transform.parent.gameObject.SetActive(false);
            StartCoroutine(RevealResults());
        }

        private System.Collections.IEnumerator RevealResults()
        {
            var group = _resultsPanel.GetComponent<CanvasGroup>();
            if (group == null) group = _resultsPanel.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0;
            while (group.alpha < 1) { group.alpha = Mathf.MoveTowards(group.alpha, 1, Time.unscaledDeltaTime * 6); yield return null; }
        }

        public static string FormatResults(ModuleRunResult result)
        {
            var valid = ModuleRankUtility.IsValidForPersonalBest(result.Validity);
            var pb = result.PersonalBestSeconds > 0d
                ? "PB " + RunTimerFormatting.Format(result.PersonalBestSeconds) : "PB --";
            if (valid && result.IsNewPersonalBest) pb = "NEW " + pb;
            var next = result.NextRank == ModuleRank.None
                ? "DIAMOND ACHIEVED"
                : $"{result.SecondsFromNextRank:0.00}s TO {ModuleRankUtility.Display(result.NextRank)}";
            return "<size=22><color=#C7E8F3>" + (valid ? "MODULE FIXED" : "PRACTICE COMPLETE") + "</color></size>\n"
                + "<size=42>" + ModuleRankUtility.Display(result.Rank) + "</size>\n"
                + "<size=36><color=#F0FBFF>" + RunTimerFormatting.Format(result.CompletionSeconds) + "</color></size>\n"
                + "<size=23>" + pb + "</size>\n<size=20><color=#C7E8F3>" + next + "</color></size>"
                + (result.NewlyUnlockedModuleId != null ? "\n<size=18><color=#66E3ED>NEXT ROAD UNLOCKED</color></size>" : string.Empty)
                + (valid ? string.Empty : "\nPB / PROGRESSION NOT RECORDED");
        }

        public void ShowModuleFixed()
        {
            if (_completion == null || _module == null || _telemetry == null || (_resultsPanel != null && _resultsPanel.gameObject.activeSelf))
            {
                return;
            }

            _completion.text =
                $"MODULE FIXED\n{_module.DisplayName.ToUpperInvariant()}\n" +
                $"{RunTimerFormatting.Format(_telemetry.CompletionSeconds)}";
            _completion.enabled = true;
        }

        private void CreateSplitText()
        {
            if (_status == null)
            {
                return;
            }

            _splitText = CreateText(
                "Checkpoint Split",
                _status.transform.parent,
                string.Empty,
                30,
                TextAnchor.UpperCenter,
                new Vector2(0.36f, 0.77f),
                new Vector2(0.64f, 0.9f),
                Vector2.zero,
                Vector2.zero);
            _splitText.enabled = false;
            _splitText.color = new Color(0.78f, 0.96f, 1f, 0.94f);
        }

        private void CreateResultsPanel()
        {
            if (_status == null)
            {
                return;
            }

            var panelObject = CreateUiObject(
                "Module Results",
                _status.transform.parent,
                new Vector2(0.32f, 0.2f),
                new Vector2(0.68f, 0.8f),
                Vector2.zero,
                Vector2.zero,
                typeof(Image));
            _resultsPanel = panelObject.GetComponent<RectTransform>();
            panelObject.GetComponent<Image>().color = BrandPresentation.PanelNavy;
            _resultsAccent = CreateUiObject(
                "Results Accent",
                _resultsPanel,
                new Vector2(0f, 0.985f),
                new Vector2(1f, 1f),
                Vector2.zero,
                Vector2.zero,
                typeof(Image)).GetComponent<Image>();
            _resultsAccent.color = _visuals.ColorFor(ModuleMaterialRole.WarmAccent);
            _resultsText = CreateText(
                "Results Text",
                _resultsPanel,
                string.Empty,
                30,
                TextAnchor.UpperCenter,
                new Vector2(0f, 0.24f),
                new Vector2(1f, 1f),
                new Vector2(18f, 18f),
                new Vector2(-18f, -18f));
            CreateButton(_resultsPanel, "RETRY", new Vector2(0.5f, 0.13f), _retry);
            CreateButton(_resultsPanel, ModuleSelectionState.GetNextCampaignModuleId(_module.StableModuleId) != null
                ? "NEXT MODULE" : "CAMPAIGN", new Vector2(0.5f, 0.24f), _nextModule);
            if (ModuleSelectionState.GetNextCampaignModuleId(_module.StableModuleId) != null)
                CreateButton(_resultsPanel, "CAMPAIGN", new Vector2(0.5f, 0.025f), _moduleSelect);
            panelObject.SetActive(false);
        }

        private void CreateRankStrip()
        {
            if (_status == null)
            {
                return;
            }

            var stripObject = CreateUiObject(
                "Rank Strip",
                _status.transform.parent,
                new Vector2(0.7f, 0.934f),
                new Vector2(0.975f, 0.983f),
                Vector2.zero,
                Vector2.zero,
                typeof(Image));
            stripObject.GetComponent<Image>().color = _visuals.HudPanelColor;
            _rankBadges = new Image[DisplayRanks.Length];
            _rankLabels = new Text[DisplayRanks.Length];
            for (var index = 0; index < DisplayRanks.Length; index++)
            {
                var min = index / (float)DisplayRanks.Length;
                var max = (index + 1) / (float)DisplayRanks.Length;
                var badge = CreateUiObject(
                    DisplayRanks[index] + " Badge",
                    stripObject.transform,
                    new Vector2(min, 0f),
                    new Vector2(max, 1f),
                    Vector2.zero,
                    Vector2.zero,
                    typeof(Image));
                var image = badge.GetComponent<Image>();
                image.color = _visuals.RankColor(DisplayRanks[index]);
                _rankBadges[index] = image;
                _rankLabels[index] = CreateText(
                    DisplayRanks[index].ToString(),
                    badge.transform,
                    ModuleRankUtility.Display(DisplayRanks[index]),
                    15,
                    TextAnchor.MiddleCenter,
                    Vector2.zero,
                    Vector2.one,
                    new Vector2(2f, 0f),
                    new Vector2(-2f, 0f));
                _rankLabels[index].color = Color.white;
                _rankLabels[index].raycastTarget = false;
            }
        }

        private void RefreshRankStrip(ModuleRank target)
        {
            if (_rankBadges == null || _rankBadges.Length == 0)
            {
                return;
            }

            for (var index = 0; index < _rankBadges.Length && index < DisplayRanks.Length; index++)
            {
                if (_rankBadges[index] == null)
                {
                    continue;
                }

                var color = _visuals.RankColor(DisplayRanks[index]);
                color.a = DisplayRanks[index] == target ? 0.72f : 0.15f;
                _rankBadges[index].color = color;
                if (_rankLabels[index] != null)
                {
                    _rankLabels[index].color = DisplayRanks[index] == target
                        ? Color.white
                        : new Color(1f, 1f, 1f, 0.7f);
                }
            }
        }

        private static GameObject CreateUiObject(
            string objectName,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            params Type[] extraComponents)
        {
            var types = new System.Collections.Generic.List<Type> { typeof(RectTransform) };
            types.AddRange(extraComponents);
            var gameObject = new GameObject(objectName, types.ToArray());
            gameObject.transform.SetParent(parent, false);
            var rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            return gameObject;
        }

        private static Text CreateText(
            string objectName,
            Transform parent,
            string value,
            int fontSize,
            TextAnchor alignment,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            var textObject = CreateUiObject(
                objectName,
                parent,
                anchorMin,
                anchorMax,
                Vector2.zero,
                Vector2.zero,
                typeof(Text));
            var rect = textObject.GetComponent<RectTransform>();
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            var text = textObject.GetComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static void CreateButton(
            Transform parent,
            string label,
            Vector2 anchor,
            Action action)
        {
            var buttonObject = CreateUiObject(
                label + " Button",
                parent,
                anchor,
                anchor,
                Vector2.zero,
                new Vector2(280f, 48f),
                typeof(Image),
                typeof(Button));
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0.5f, 0.5f);
            buttonObject.GetComponent<Image>().color = label == "NEXT MODULE"
                ? new Color(1f, 0.68f, 0.12f, 0.92f)
                : new Color(0.08f, 0.12f, 0.22f, 0.86f);
            buttonObject.GetComponent<Button>().onClick.AddListener(() => { UiAudio.Play(); action?.Invoke(); });
            var text = CreateText(
                label,
                buttonObject.transform,
                label,
                20,
                TextAnchor.MiddleCenter,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
            text.raycastTarget = false;
            text.color = Color.white;
        }
    }
}

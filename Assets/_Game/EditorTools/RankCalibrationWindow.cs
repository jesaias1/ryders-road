using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Ranking;
using Avoidance.Gameplay.Timing;
using UnityEditor;
using UnityEngine;

namespace Avoidance.EditorTools
{
    public sealed class RankCalibrationWindow : EditorWindow
    {
        private Vector2 _scroll;
        private readonly Dictionary<string, ModuleRankThresholds> _drafts =
            new Dictionary<string, ModuleRankThresholds>(StringComparer.Ordinal);

        [MenuItem("RYDERS BLOCK/Rank Calibration")]
        public static void Open()
        {
            GetWindow<RankCalibrationWindow>("RYDERS BLOCK Ranks");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Phase 3 Rank Calibration", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Thresholds are provisional. Review local completion samples and apply changes manually.",
                MessageType.Info);

            var modules = AssetDatabase.FindAssets(
                    "t:ModuleDefinition",
                    new[] { Phase2ModuleContentFactory.ModuleResourcesDirectory })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<ModuleDefinition>)
                .Where(module => module != null)
                .OrderBy(module => module.StableModuleId)
                .ToArray();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var module in modules)
            {
                DrawModule(module);
                EditorGUILayout.Space(12f);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawModule(ModuleDefinition module)
        {
            var current = module.RankThresholds;
            if (!_drafts.TryGetValue(module.StableModuleId, out var draft))
            {
                draft = new ModuleRankThresholds(
                    current.SilverSeconds,
                    current.GoldSeconds,
                    current.DiamondSeconds,
                    current.CalibrationState,
                    current.ThresholdVersion,
                    current.DesignerNotes);
                _drafts[module.StableModuleId] = draft;
            }

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(module.DisplayName, EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Stable ID", module.StableModuleId);
            EditorGUILayout.LabelField(
                "Estimates",
                $"Clean {module.ExpectedCleanTime:0.###}s | Casual {module.EstimatedCasualTime:0.###}s");

            var samples = LoadSamples(module.StableModuleId);
            EditorGUILayout.LabelField(
                "Local completions",
                samples.CompletionTimes.Count == 0
                    ? "none"
                    : $"count {samples.CompletionTimes.Count}, fastest {samples.CompletionTimes.Min():0.000}s, median {Median(samples.CompletionTimes):0.000}s, average {samples.CompletionTimes.Average():0.000}s");
            EditorGUILayout.LabelField(
                "Local attempts",
                samples.ReportCount == 0
                    ? "none"
                    : $"reports {samples.ReportCount}, attempts {samples.Attempts}, falls {samples.Falls}, average falls/report {samples.AverageFalls:0.00}");

            var silver = EditorGUILayout.FloatField("Silver", draft.SilverSeconds);
            var gold = EditorGUILayout.FloatField("Gold", draft.GoldSeconds);
            var diamond = EditorGUILayout.FloatField("Diamond", draft.DiamondSeconds);
            var state = (RankCalibrationState)EditorGUILayout.EnumPopup(
                "Calibration",
                draft.CalibrationState);
            var version = EditorGUILayout.IntField("Threshold Version", draft.ThresholdVersion);
            var notes = EditorGUILayout.TextField("Designer Notes", draft.DesignerNotes);
            _drafts[module.StableModuleId] = new ModuleRankThresholds(
                silver,
                gold,
                diamond,
                state,
                version,
                notes);

            if (samples.CompletionTimes.Count > 0
                && GUILayout.Button("Suggest From Local Samples"))
            {
                var median = Median(samples.CompletionTimes);
                _drafts[module.StableModuleId] = new ModuleRankThresholds(
                    Mathf.Max((float)median * 1.35f, module.ExpectedCleanTime * 1.25f),
                    Mathf.Max((float)median * 1.12f, module.ExpectedCleanTime),
                    Mathf.Max((float)median * 0.92f, module.ExpectedCleanTime * 0.82f),
                    RankCalibrationState.InTesting,
                    Math.Max(1, draft.ThresholdVersion + 1),
                    "Suggested from local development samples; verify on physical Android.");
            }

            if (GUILayout.Button("Apply Draft To Module Asset"))
            {
                module.ConfigureRankThresholds(_drafts[module.StableModuleId]);
                EditorUtility.SetDirty(module);
                AssetDatabase.SaveAssets();
            }

            EditorGUILayout.EndVertical();
        }

        private sealed class CalibrationSampleSummary
        {
            public List<double> CompletionTimes { get; } = new List<double>();
            public int ReportCount { get; set; }
            public int Attempts { get; set; }
            public int Falls { get; set; }
            public double AverageFalls => ReportCount <= 0 ? 0d : (double)Falls / ReportCount;
        }

        private static CalibrationSampleSummary LoadSamples(string moduleId)
        {
            var samples = new CalibrationSampleSummary();
            var directory = Path.Combine(Application.persistentDataPath, "ModuleReports");
            if (!Directory.Exists(directory))
            {
                return samples;
            }

            foreach (var file in Directory.EnumerateFiles(directory, "*.json"))
            {
                try
                {
                    var report = JsonUtility.FromJson<ModuleAttemptReport>(
                        File.ReadAllText(file));
                    if (report != null
                        && report.moduleId == moduleId)
                    {
                        samples.ReportCount++;
                        samples.Attempts += Math.Max(0, report.attempts);
                        samples.Falls += Math.Max(0, report.falls);
                        if (report.completed && report.completionSeconds > 0f)
                        {
                            samples.CompletionTimes.Add(report.completionSeconds);
                        }
                    }
                }
                catch (IOException)
                {
                }
                catch (ArgumentException)
                {
                }
            }

            return samples;
        }

        private static double Median(List<double> values)
        {
            var ordered = values.OrderBy(value => value).ToArray();
            var middle = ordered.Length / 2;
            return ordered.Length % 2 == 0
                ? (ordered[middle - 1] + ordered[middle]) * 0.5d
                : ordered[middle];
        }
    }
}

using UnityEngine;

namespace Avoidance.UI
{
    public static class SafeAreaLayoutHelper
    {
        public static void CalculateAnchors(
            Rect safeArea,
            Vector2 screenSize,
            out Vector2 anchorMin,
            out Vector2 anchorMax)
        {
            if (screenSize.x <= 0f || screenSize.y <= 0f)
            {
                anchorMin = Vector2.zero;
                anchorMax = Vector2.one;
                return;
            }

            anchorMin = new Vector2(
                Mathf.Clamp01(safeArea.xMin / screenSize.x),
                Mathf.Clamp01(safeArea.yMin / screenSize.y));
            anchorMax = new Vector2(
                Mathf.Clamp01(safeArea.xMax / screenSize.x),
                Mathf.Clamp01(safeArea.yMax / screenSize.y));
        }
    }

    [DisallowMultipleComponent]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            Apply();
        }

        private void Update()
        {
            if (_lastSafeArea != Screen.safeArea
                || _lastScreenSize.x != Screen.width
                || _lastScreenSize.y != Screen.height)
            {
                Apply();
            }
        }

        public void Apply()
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }

            SafeAreaLayoutHelper.CalculateAnchors(
                Screen.safeArea,
                new Vector2(Screen.width, Screen.height),
                out var minimum,
                out var maximum);
            _rectTransform.anchorMin = minimum;
            _rectTransform.anchorMax = maximum;
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;
            _lastSafeArea = Screen.safeArea;
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        }
    }
}

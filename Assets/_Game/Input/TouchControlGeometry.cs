using UnityEngine;

namespace Avoidance.Input
{
    public static class TouchControlGeometry
    {
        public static Rect BuildSafeOriginArea(
            Rect safeRect,
            float controlRadius,
            float horizontalComfortMargin,
            float verticalComfortMargin)
        {
            var radius = Mathf.Max(0f, controlRadius);
            var horizontalMargin = Mathf.Max(0f, horizontalComfortMargin);
            var verticalMargin = Mathf.Max(0f, verticalComfortMargin);
            var xMin = safeRect.xMin + radius + horizontalMargin;
            var xMax = safeRect.xMax - radius - horizontalMargin;
            var yMin = safeRect.yMin + radius + verticalMargin;
            var yMax = safeRect.yMax - radius - verticalMargin;
            if (xMin > xMax)
            {
                xMin = xMax = safeRect.center.x;
            }

            if (yMin > yMax)
            {
                yMin = yMax = safeRect.center.y;
            }

            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        public static Vector2 ClampOrigin(
            Vector2 requestedOrigin,
            Rect safeRect,
            float controlRadius,
            float horizontalComfortMargin,
            float verticalComfortMargin)
        {
            var originArea = BuildSafeOriginArea(
                safeRect,
                controlRadius,
                horizontalComfortMargin,
                verticalComfortMargin);
            return new Vector2(
                Mathf.Clamp(requestedOrigin.x, originArea.xMin, originArea.xMax),
                Mathf.Clamp(requestedOrigin.y, originArea.yMin, originArea.yMax));
        }

        public static bool TravelCircleFits(
            Vector2 origin,
            Rect safeRect,
            float controlRadius,
            float horizontalComfortMargin,
            float verticalComfortMargin)
        {
            return origin.x - controlRadius >= safeRect.xMin + horizontalComfortMargin
                && origin.x + controlRadius <= safeRect.xMax - horizontalComfortMargin
                && origin.y - controlRadius >= safeRect.yMin + verticalComfortMargin
                && origin.y + controlRadius <= safeRect.yMax - verticalComfortMargin;
        }

        public static Vector2 AnchorToLocal(Rect parentRect, Vector2 normalizedAnchor)
        {
            return new Vector2(
                Mathf.Lerp(parentRect.xMin, parentRect.xMax, Mathf.Clamp01(normalizedAnchor.x)),
                Mathf.Lerp(parentRect.yMin, parentRect.yMax, Mathf.Clamp01(normalizedAnchor.y)));
        }
    }
}

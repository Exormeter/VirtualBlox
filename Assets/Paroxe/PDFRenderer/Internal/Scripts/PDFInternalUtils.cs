using UnityEngine;

namespace Paroxe.PdfRenderer.Internal
{
    public static class PDFInternalUtils
    {
        public static float CalculateRectTransformIntersectArea(RectTransform a, RectTransform b)
        {
            Vector3[] worldCorners = new Vector3[4];

            a.GetWorldCorners(worldCorners);
            Vector2 min = worldCorners[0];
            Vector2 max = worldCorners[0];

            for (int i = 1; i < 4; ++i)
            {
                if (worldCorners[i].x < min.x)
                    min.x = worldCorners[i].x;
                if (worldCorners[i].y < min.y)
                    min.y = worldCorners[i].y;
                if (worldCorners[i].x > max.x)
                    max.x = worldCorners[i].x;
                if (worldCorners[i].y > max.y)
                    max.y = worldCorners[i].y;
            }

            Rect ra = new Rect(min.x, min.y, max.x - min.x, max.y - min.y);

            b.GetWorldCorners(worldCorners);

            min = worldCorners[0];
            max = worldCorners[0];

            for (int i = 1; i < 4; ++i)
            {
                if (worldCorners[i].x < min.x)
                    min.x = worldCorners[i].x;
                if (worldCorners[i].y < min.y)
                    min.y = worldCorners[i].y;
                if (worldCorners[i].x > max.x)
                    max.x = worldCorners[i].x;
                if (worldCorners[i].y > max.y)
                    max.y = worldCorners[i].y;
            }

            Rect rb = new Rect(min.x, min.y, max.x - min.x, max.y - min.y);

            float x_overlap = Mathf.Min(ra.xMin + ra.width, rb.xMin + rb.width) - Mathf.Max(ra.xMin, rb.xMin) + 1;
            float y_overlap = Mathf.Min(ra.yMin + ra.height, rb.yMin + rb.height) - Mathf.Max(ra.yMin, rb.yMin) + 1;

            if (x_overlap <= 0.0f || y_overlap <= 0.0f)
                return 0.0f;

            return x_overlap * y_overlap;
        }

        public static float CubicEaseIn(float currentTime, float startingValue, float finalValue, float duration)
        {
            return finalValue * (currentTime /= duration) * currentTime * currentTime + startingValue;
        }
    }
}
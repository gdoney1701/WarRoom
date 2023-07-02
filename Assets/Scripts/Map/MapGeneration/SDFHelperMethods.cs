using UnityEngine;

public static class SDFHelperMethods
{
    public static float SignedDistance(Vector2[] vertices, Vector2 point)
    {
        bool inside = false;
        float minDistSq = float.MaxValue;

        for (int i = 0, j = vertices.Length - 1; i < vertices.Length; j = i++)
        {
            Vector2 a = vertices[i];
            Vector2 b = vertices[j];

            if ((((a.y <= point.y) && (point.y < b.y)) ||
                ((b.y <= point.y) && (point.y < a.y))) &&
                (point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x))
                inside = !inside;
            minDistSq = Mathf.Min(minDistSq, SegmentDistance(a, b, point));
        }

        return (inside ? 1 : -1) * Mathf.Sqrt(minDistSq);
    }
    public static float SegmentDistance(Vector2 a, Vector2 b, Vector2 p)
    {
        float x = a.x;
        float y = a.y;
        float dx = b.x - x;
        float dy = b.y - y;

        if (dx != 0 || dy != 0)
        {
            float t = ((p.x - x) * dx + (p.y - y) * dy) / (dx * dx + dy * dy);

            if (t > 1)
            {
                x = b.x;
                y = b.y;
            }
            else if (t > 0)
            {
                x += dx * t;
                y += dy * t;
            }
        }

        dx = p.x - x;
        dy = p.y - y;

        return dx * dx + dy * dy;
    }
}

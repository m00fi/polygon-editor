using PolygonEditor.Drawing;
using PolygonEditor.Models;

namespace PolygonEditor.Interaction;

public static class HitTester
{
    public const float VertexHitRadius = 8f;
    public const float ControlPointHitRadius = 7f;
    public const float EdgeHitThreshold = 10f;

    public static HitResult HitTest(Models.Scene scene, PointF point)
    {
        // Priorytet: wierzchołek > punkt kontrolny > krawędź > wnętrze.
        foreach (Models.Polygon polygon in scene.Polygons)
        {
            foreach (Vertex vertex in polygon.Vertices)
            {
                if (Distance(point, vertex.Position) <= VertexHitRadius)
                    return HitResult.FromVertex(polygon, vertex);
            }
        }

        foreach (Models.Polygon polygon in scene.Polygons)
        {
            foreach (Edge edge in polygon.Edges)
            {
                if (!edge.IsBezier)
                    continue;
                if (Distance(point, edge.Control1.Position) <= ControlPointHitRadius)
                    return HitResult.FromControlPoint(polygon, edge.Control1);
                if (Distance(point, edge.Control2.Position) <= ControlPointHitRadius)
                    return HitResult.FromControlPoint(polygon, edge.Control2);
            }
        }

        foreach (Models.Polygon polygon in scene.Polygons)
        {
            foreach (Edge edge in polygon.Edges)
            {
                if (DistanceToPolyline(point, EdgeShape.Points(edge)) <= EdgeHitThreshold)
                    return HitResult.FromEdge(polygon, edge);
            }
        }

        for (int i = scene.Polygons.Count - 1; i >= 0; i--)
        {
            Models.Polygon polygon = scene.Polygons[i];
            if (IsPointInPolygon(point, Outline(polygon)))
                return HitResult.FromPolygon(polygon);
        }

        return HitResult.None;
    }

    // Obwód wielokąta z uwzględnieniem krzywych i łuków; ostatni punkt każdej krawędzi to początek następnej.
    private static List<PointF> Outline(Models.Polygon polygon)
    {
        var outline = new List<PointF>();
        foreach (Edge edge in polygon.Edges)
        {
            PointF[] points = EdgeShape.Points(edge);
            for (int i = 0; i < points.Length - 1; i++)
                outline.Add(points[i]);
        }

        return outline;
    }

    private static float Distance(PointF a, PointF b)
    {
        float dx = a.X - b.X;
        float dy = a.Y - b.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    private static float DistanceToPolyline(PointF p, PointF[] points)
    {
        float best = float.MaxValue;
        for (int i = 1; i < points.Length; i++)
            best = MathF.Min(best, DistanceToSegment(p, points[i - 1], points[i]));
        return best;
    }

    private static float DistanceToSegment(PointF p, PointF a, PointF b)
    {
        float dx = b.X - a.X;
        float dy = b.Y - a.Y;
        float lengthSq = dx * dx + dy * dy;
        if (lengthSq < 1e-6f)
            return Distance(p, a);

        float t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / lengthSq;
        t = Math.Clamp(t, 0f, 1f);
        var projection = new PointF(a.X + t * dx, a.Y + t * dy);
        return Distance(p, projection);
    }

    // Reguła even-odd (dopuszcza samoprzecięcia).
    private static bool IsPointInPolygon(PointF point, IReadOnlyList<PointF> outline)
    {
        bool inside = false;
        int n = outline.Count;
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            PointF vi = outline[i];
            PointF vj = outline[j];

            bool intersect = (vi.Y > point.Y) != (vj.Y > point.Y)
                && point.X < (vj.X - vi.X) * (point.Y - vi.Y) / (vj.Y - vi.Y + float.Epsilon) + vi.X;

            if (intersect)
                inside = !inside;
        }

        return inside;
    }
}

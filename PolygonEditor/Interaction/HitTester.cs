using PolygonEditor.Models;

namespace PolygonEditor.Interaction;

/// <summary>Wykrywanie kliknięć w wierzchołki, krawędzie i wnętrze wielokąta.</summary>
public static class HitTester
{
    public const float VertexHitRadius = 8f;
    public const float EdgeHitThreshold = 10f;

    public static HitResult HitTest(Models.Scene scene, PointF point)
    {
        // Priorytet: wierzchołek > krawędź > wnętrze.
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
                if (DistanceToSegment(point, edge.Start.Position, edge.End.Position) <= EdgeHitThreshold)
                    return HitResult.FromEdge(polygon, edge);
            }
        }

        for (int i = scene.Polygons.Count - 1; i >= 0; i--)
        {
            Models.Polygon polygon = scene.Polygons[i];
            if (IsPointInPolygon(point, polygon))
                return HitResult.FromPolygon(polygon);
        }

        return HitResult.None;
    }

    private static float Distance(PointF a, PointF b)
    {
        float dx = a.X - b.X;
        float dy = a.Y - b.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
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

    private static bool IsPointInPolygon(PointF point, Models.Polygon polygon)
    {
        // Even-odd rule.
        bool inside = false;
        IReadOnlyList<Vertex> vertices = polygon.Vertices;
        int n = vertices.Count;
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            PointF vi = vertices[i].Position;
            PointF vj = vertices[j].Position;

            bool intersect = (vi.Y > point.Y) != (vj.Y > point.Y)
                && point.X < (vj.X - vi.X) * (point.Y - vi.Y) / (vj.Y - vi.Y + float.Epsilon) + vi.X;

            if (intersect)
                inside = !inside;
        }

        return inside;
    }
}

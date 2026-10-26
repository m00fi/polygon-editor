using PolygonEditor.Models;

namespace PolygonEditor.Drawing;

// Krawędź jako łamana - te same punkty służą do rysowania i do trafiania myszą.
public static class EdgeShape
{
    public static PointF[] Points(Edge edge) => edge.Kind switch
    {
        EdgeKind.CubicBezier => BezierIncremental.ComputePoints(edge),
        EdgeKind.CircularArc => ArcIncremental.ComputePoints(edge),
        _ => [edge.Start.Position, edge.End.Position]
    };
}

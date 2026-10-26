using PolygonEditor.Models;

namespace PolygonEditor.Drawing;

// Edge as a polyline - the same points are used for drawing and for mouse hit testing.
public static class EdgeShape
{
    public static PointF[] Points(Edge edge) => edge.Kind switch
    {
        EdgeKind.CubicBezier => BezierIncremental.ComputePoints(edge),
        EdgeKind.CircularArc => ArcIncremental.ComputePoints(edge),
        _ => [edge.Start.Position, edge.End.Position]
    };
}

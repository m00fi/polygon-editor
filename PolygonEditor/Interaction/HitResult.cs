using PolygonEditor.Models;

namespace PolygonEditor.Interaction;

public readonly struct HitResult
{
    public HitKind Kind { get; }
    public Models.Polygon? Polygon { get; }
    public Vertex? Vertex { get; }

    // Przy trafieniu w punkt kontrolny to krawędź, do której on należy.
    public Edge? Edge { get; }
    public ControlPoint? ControlPoint { get; }

    private HitResult(HitKind kind, Models.Polygon? polygon, Vertex? vertex, Edge? edge, ControlPoint? controlPoint)
    {
        Kind = kind;
        Polygon = polygon;
        Vertex = vertex;
        Edge = edge;
        ControlPoint = controlPoint;
    }

    public static HitResult None => new(HitKind.None, null, null, null, null);

    public static HitResult FromVertex(Models.Polygon polygon, Vertex vertex) =>
        new(HitKind.Vertex, polygon, vertex, null, null);

    public static HitResult FromControlPoint(Models.Polygon polygon, ControlPoint controlPoint) =>
        new(HitKind.ControlPoint, polygon, null, controlPoint.Edge, controlPoint);

    public static HitResult FromEdge(Models.Polygon polygon, Edge edge) =>
        new(HitKind.Edge, polygon, null, edge, null);

    public static HitResult FromPolygon(Models.Polygon polygon) =>
        new(HitKind.Polygon, polygon, null, null, null);
}

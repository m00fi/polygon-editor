using PolygonEditor.Models;

namespace PolygonEditor.Interaction;

/// <summary>Wynik hit-testu na scenie.</summary>
public readonly struct HitResult
{
    public HitKind Kind { get; }
    public Models.Polygon? Polygon { get; }
    public Vertex? Vertex { get; }
    public Edge? Edge { get; }

    private HitResult(HitKind kind, Models.Polygon? polygon, Vertex? vertex, Edge? edge)
    {
        Kind = kind;
        Polygon = polygon;
        Vertex = vertex;
        Edge = edge;
    }

    public static HitResult None => new(HitKind.None, null, null, null);

    public static HitResult FromVertex(Models.Polygon polygon, Vertex vertex) =>
        new(HitKind.Vertex, polygon, vertex, null);

    public static HitResult FromEdge(Models.Polygon polygon, Edge edge) =>
        new(HitKind.Edge, polygon, null, edge);

    public static HitResult FromPolygon(Models.Polygon polygon) =>
        new(HitKind.Polygon, polygon, null, null);
}

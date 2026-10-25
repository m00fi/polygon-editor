namespace PolygonEditor.Models;
using PolygonEditor.Constraints;

public sealed class Scene
{
    public const Continuity DefaultContinuity = Continuity.G1;

    private readonly List<Polygon> _polygons = [];
    private readonly ConstraintStore _constraintStore = new ConstraintStore();

    public IReadOnlyList<Polygon> Polygons => _polygons;
    public ConstraintStore Constraints => _constraintStore;

    public void Add(Polygon polygon)
    {
        ArgumentNullException.ThrowIfNull(polygon);
        _polygons.Add(polygon);
    }

    public bool Remove(Polygon polygon) => _polygons.Remove(polygon);

    public void Clear()
    {
        _polygons.Clear();
        _constraintStore.Clear();
    }

    // Ograniczenia długości/równoległości dotyczą tylko odcinków, więc przy zmianie rodzaju są usuwane.
    public void SetEdgeKind(Edge edge, EdgeKind kind)
    {
        ArgumentNullException.ThrowIfNull(edge);
        if (edge.Kind == kind)
            return;

        _constraintStore.RemoveAllInvolvingEdge(edge);
        edge.Kind = kind;
        if (kind == EdgeKind.CubicBezier)
            edge.InitializeControlPoints(OutwardSide(edge));

        UpdateDefaultContinuity(edge.Start);
        UpdateDefaultContinuity(edge.End);
    }

    // Przy obiegu zgodnym z zegarem (pole > 0, oś Y w dół) normalna (-dy, dx) wskazuje do wnętrza,
    // więc na zewnątrz jest strona przeciwna.
    private float OutwardSide(Edge edge)
    {
        Polygon? polygon = FindPolygon(edge);
        if (polygon is null)
            return 1f;

        return polygon.SignedArea > 0f ? -1f : 1f;
    }

    public Vertex InsertVertex(Polygon polygon, Edge edge)
    {
        Vertex start = edge.Start;
        Vertex end = edge.End;

        _constraintStore.RemoveAllInvolvingEdge(edge);
        Vertex mid = polygon.InsertVertexAtMidpoint(edge);

        UpdateDefaultContinuity(start);
        UpdateDefaultContinuity(end);
        return mid;
    }

    // Zwraca false, gdy w wielokącie zostałyby mniej niż 3 wierzchołki.
    public bool RemoveVertex(Polygon polygon, Vertex vertex)
    {
        if (!polygon.CanRemoveVertex(vertex) || vertex.Incoming is null || vertex.Outgoing is null)
            return false;

        Vertex prev = vertex.Incoming.Start;
        Vertex next = vertex.Outgoing.End;

        foreach (Edge edge in polygon.EdgesAffectedByVertexRemoval(vertex).ToList())
            _constraintStore.RemoveAllInvolvingEdge(edge);
        polygon.RemoveVertex(vertex);

        UpdateDefaultContinuity(prev);
        UpdateDefaultContinuity(next);
        return true;
    }

    // Ciągłość wybrana przez użytkownika zostaje; inaczej G1 obok krzywej, a G0 między samymi odcinkami.
    private static void UpdateDefaultContinuity(Vertex vertex)
    {
        if (vertex.ContinuityUserSet)
            return;

        vertex.Continuity = vertex.HasAdjacentCurve ? DefaultContinuity : Continuity.G0;
    }

    public Polygon? FindPolygon(Vertex vertex)
    {
        foreach (Polygon polygon in _polygons)
        {
            if (polygon.ContainsVertex(vertex))
                return polygon;
        }

        return null;
    }

    public Polygon? FindPolygon(Edge edge)
    {
        foreach (Polygon polygon in _polygons)
        {
            if (polygon.ContainsEdge(edge))
                return polygon;
        }

        return null;
    }
}

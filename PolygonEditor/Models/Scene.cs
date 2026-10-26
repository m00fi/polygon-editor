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
        else if (kind == EdgeKind.CircularArc)
            edge.ArcSide = OutwardSide(edge);

        UpdateDefaultContinuity(edge.Start);
        UpdateDefaultContinuity(edge.End);

        if (kind == EdgeKind.CircularArc)
        {
            // Łuk nie dopuszcza C1, a G1 może mieć tylko na jednym końcu (wtedy zostaje początek).
            foreach (Vertex end in new[] { edge.Start, edge.End })
            {
                if (end.Continuity == Continuity.C1)
                    end.Continuity = Continuity.G1;
            }

            if (edge.Start.Continuity != Continuity.G0 && edge.End.Continuity != Continuity.G0)
                edge.End.Continuity = Continuity.G0;
        }
    }

    // Ciągłość wybrana przez użytkownika. Łuk może mieć G1 tylko na jednym końcu, więc drugi koniec
    // każdego sąsiedniego łuku przechodzi wtedy na G0. Zwraca wierzchołki zmienione przy okazji.
    public IReadOnlyList<Vertex> SetContinuity(Vertex vertex, Continuity continuity)
    {
        if (continuity == Continuity.C1 && !vertex.AllowsC1)
            throw new InvalidOperationException("C1 is allowed only next to a Bézier curve and never next to an arc.");

        vertex.SetContinuityByUser(continuity);

        var changed = new List<Vertex>();
        if (continuity == Continuity.G0)
            return changed;

        foreach (Edge? edge in new[] { vertex.Incoming, vertex.Outgoing })
        {
            if (edge is null || !edge.IsArc)
                continue;

            Vertex other = edge.Start == vertex ? edge.End : edge.Start;
            if (other.Continuity == Continuity.G0)
                continue;

            other.SetContinuityByUser(Continuity.G0);
            changed.Add(other);
        }

        return changed;
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

    // Ciągłość wybrana przez użytkownika zostaje; inaczej G1 obok Béziera, a G0 w pozostałych przypadkach
    // (polecenie określa domyślną ciągłość tylko dla Béziera - łuk domyślnie ma G0 na obu końcach, czyli półokrąg).
    // Domyślne G1 nie jest ustawiane, gdy sąsiedni łuk ma już G1 na drugim końcu.
    private static void UpdateDefaultContinuity(Vertex vertex)
    {
        if (vertex.ContinuityUserSet)
            return;

        Continuity continuity = vertex.HasAdjacentBezier ? DefaultContinuity : Continuity.G0;
        if (continuity != Continuity.G0 && ArcHasG1AtOtherEnd(vertex))
            continuity = Continuity.G0;

        vertex.Continuity = continuity;
    }

    private static bool ArcHasG1AtOtherEnd(Vertex vertex)
    {
        foreach (Edge? edge in new[] { vertex.Incoming, vertex.Outgoing })
        {
            if (edge is null || !edge.IsArc)
                continue;

            Vertex other = edge.Start == vertex ? edge.End : edge.Start;
            if (other.Continuity != Continuity.G0)
                return true;
        }

        return false;
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

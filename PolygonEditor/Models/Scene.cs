namespace PolygonEditor.Models;
using PolygonEditor.Constraints;

public sealed class Scene
{
    private readonly List<Polygon> _polygons = [];
    private ConstraintStore _constraintStore = new ConstraintStore();

    public IReadOnlyList<Polygon> Polygons => _polygons;
    public ConstraintStore Constraints => _constraintStore;

    public void Add(Polygon polygon)
    {
        ArgumentNullException.ThrowIfNull(polygon);
        _polygons.Add(polygon);
    }

    public void AddConstraintStore(ConstraintStore constraintStore)
    {
        ArgumentNullException.ThrowIfNull(constraintStore);
        _constraintStore = constraintStore;
    }

    public bool Remove(Polygon polygon) => _polygons.Remove(polygon);

    public void Clear()
    {
        _polygons.Clear();
        _constraintStore.Clear();
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

namespace PolygonEditor.Models;

/// <summary>Scena z wielokątami na canvasie.</summary>
public sealed class Scene
{
    private readonly List<Polygon> _polygons = [];

    public IReadOnlyList<Polygon> Polygons => _polygons;

    public void Add(Polygon polygon)
    {
        ArgumentNullException.ThrowIfNull(polygon);
        _polygons.Add(polygon);
    }

    public bool Remove(Polygon polygon) => _polygons.Remove(polygon);

    public void Clear() => _polygons.Clear();

    /// <summary>Szuka wielokąta zawierającego wierzchołek.</summary>
    public Polygon? FindPolygon(Vertex vertex)
    {
        foreach (Polygon polygon in _polygons)
        {
            if (polygon.ContainsVertex(vertex))
                return polygon;
        }

        return null;
    }

    /// <summary>Szuka wielokąta zawierającego krawędź.</summary>
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

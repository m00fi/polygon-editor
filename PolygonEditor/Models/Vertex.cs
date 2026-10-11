namespace PolygonEditor.Models;

/// <summary>Wierzchołek wielokąta.</summary>
public sealed class Vertex
{
    public PointF Position { get; set; }

    /// <summary>Wymagana ciągłość w wierzchołku.</summary>
    public Continuity Continuity { get; set; } = Continuity.G0;

    /// <summary>True, gdy użytkownik sam ustawił ciągłość.</summary>
    public bool ContinuityUserSet { get; set; }

    /// <summary>Krawędź wchodząca.</summary>
    public Edge? Incoming { get; internal set; }

    /// <summary>Krawędź wychodząca.</summary>
    public Edge? Outgoing { get; internal set; }

    public Vertex()
    {
    }

    public Vertex(float x, float y)
    {
        Position = new PointF(x, y);
    }

    public Vertex(PointF position)
    {
        Position = position;
    }

    public void Translate(float dx, float dy)
    {
        Position = new PointF(Position.X + dx, Position.Y + dy);
    }
}

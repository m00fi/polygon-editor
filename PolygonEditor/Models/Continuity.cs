namespace PolygonEditor.Models;

/// <summary>Klasa ciągłości w wierzchołku.</summary>
public enum Continuity
{
    /// <summary>Ciągłość położenia.</summary>
    G0 = 0,

    /// <summary>Ciągłość kierunku stycznej.</summary>
    G1 = 1,

    /// <summary>Ciągłość wektora stycznego (tylko Bézier).</summary>
    C1 = 2
}

namespace PolygonEditor.Models;

// Continuity required at a vertex next to a curve or an arc.
public enum Continuity
{
    // same position (always true, edges share the vertex)
    G0 = 0,

    // same tangent direction
    G1 = 1,

    // same tangent vector, only next to a Bezier curve
    C1 = 2
}

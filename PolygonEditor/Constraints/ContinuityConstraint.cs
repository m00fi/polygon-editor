using PolygonEditor.Models;

namespace PolygonEditor.Constraints;

// G1/C1 continuity at vertex V between incoming edge A and outgoing edge B.
// A "handle" is the point that sets the tangent at V: a Bezier control point or the other end of a line.
// G1: incoming handle, V and outgoing handle lie on one line, with V in the middle.
// C1: also equal tangent vectors. Bezier derivative at the end is 3 * (P1 - P0), for a line it is (End - Start),
// so the scale is 3 on a Bezier side and 1 on a line side.
// Not stored in ConstraintStore, the solver creates these from the vertex settings every time.
public sealed class ContinuityConstraint : IConstraint
{
    public const float PositionTolerance = 0.5f;
    public const float AngleTolerance = 0.005f;

    public Guid Id { get; } = Guid.NewGuid();
    public Vertex Vertex { get; }
    public Edge Incoming { get; }
    public Edge Outgoing { get; }

    public IReadOnlyList<Edge> InvolvedEdges => [Incoming, Outgoing];

    private ContinuityConstraint(Vertex vertex, Edge incoming, Edge outgoing)
    {
        Vertex = vertex;
        Incoming = incoming;
        Outgoing = outgoing;
    }

    public static IEnumerable<ContinuityConstraint> CreateAll(Models.Scene scene)
    {
        foreach (Models.Polygon polygon in scene.Polygons)
        {
            foreach (Vertex vertex in polygon.Vertices)
            {
                if (vertex.Continuity == Continuity.G0 || !vertex.HasAdjacentBezier)
                    continue;
                if (vertex.Incoming is null || vertex.Outgoing is null)
                    continue;

                // next to an arc the arc itself handles continuity (Edge.Arc is built from the neighbour tangent)
                if (vertex.Incoming.IsArc || vertex.Outgoing.IsArc)
                    continue;

                yield return new ContinuityConstraint(vertex, vertex.Incoming, vertex.Outgoing);
            }
        }
    }

    public bool InvolvesEdge(Edge edge) => edge == Incoming || edge == Outgoing;

    private Handle In => Incoming.IsBezier ? new Handle(null, Incoming.Control2) : new Handle(Incoming.Start, null);
    private Handle Out => Outgoing.IsBezier ? new Handle(null, Outgoing.Control1) : new Handle(Outgoing.End, null);

    // for C1: Out = V + (V - In) * ratio
    private float C1Ratio => (Incoming.IsBezier ? 3f : 1f) / (Outgoing.IsBezier ? 3f : 1f);

    public bool IsSatisfied()
    {
        PointF v = Vertex.Position;
        PointF u = Sub(v, In.Position);
        PointF w = Sub(Out.Position, v);
        float uLen = Length(u);
        float wLen = Length(w);
        if (uLen < 1e-3f || wLen < 1e-3f)
            return true;

        if (Vertex.Continuity == Continuity.C1)
        {
            PointF target = Add(v, Scale(u, C1Ratio));
            return Length(Sub(Out.Position, target)) <= PositionTolerance;
        }

        float cross = (u.X * w.Y - u.Y * w.X) / (uLen * wLen);
        float dot = u.X * w.X + u.Y * w.Y;
        return MathF.Abs(cross) <= AngleTolerance && dot > 0f;
    }

    public void Satisfy(SolveContext context)
    {
        Handle inHandle = In;
        Handle outHandle = Out;
        bool inFixed = inHandle.IsFixed(context);
        bool outFixed = outHandle.IsFixed(context);
        if (inFixed && outFixed)
            return;

        // move the handle that is not the anchor; if both are free move the control point,
        // because that does not break other constraints (moving a vertex could break lengths)
        bool moveOut = inFixed
            || (!outFixed && (outHandle.ControlPoint is not null || inHandle.ControlPoint is null));

        PointF v = Vertex.Position;
        if (moveOut)
        {
            PointF u = Sub(v, inHandle.Position);
            if (Length(u) < 1e-3f)
                return;
            outHandle.MoveTo(Vertex.Continuity == Continuity.C1
                ? Add(v, Scale(u, C1Ratio))
                : Add(v, Scale(Normalize(u), KeptLength(Sub(outHandle.Position, v), u))));
        }
        else
        {
            PointF w = Sub(outHandle.Position, v);
            if (Length(w) < 1e-3f)
                return;
            inHandle.MoveTo(Vertex.Continuity == Continuity.C1
                ? Sub(v, Scale(w, 1f / C1Ratio))
                : Sub(v, Scale(Normalize(w), KeptLength(Sub(v, inHandle.Position), w))));
        }
    }

    // for G1 the moved handle keeps its distance from V (if it is zero we take the other one)
    private static float KeptLength(PointF current, PointF other)
    {
        float length = Length(current);
        return length < 1e-3f ? Length(other) : length;
    }

    private readonly record struct Handle(Vertex? Vertex, ControlPoint? ControlPoint)
    {
        public PointF Position => Vertex?.Position ?? ControlPoint!.Position;

        public bool IsFixed(SolveContext context) =>
            Vertex is not null ? context.IsFixed(Vertex) : context.IsFixed(ControlPoint!);

        public void MoveTo(PointF position)
        {
            if (Vertex is not null)
                Vertex.Position = position;
            else
                ControlPoint!.Position = position;
        }
    }

    private static PointF Add(PointF a, PointF b) => new(a.X + b.X, a.Y + b.Y);
    private static PointF Sub(PointF a, PointF b) => new(a.X - b.X, a.Y - b.Y);
    private static PointF Scale(PointF a, float s) => new(a.X * s, a.Y * s);
    private static float Length(PointF a) => MathF.Sqrt(a.X * a.X + a.Y * a.Y);

    private static PointF Normalize(PointF a)
    {
        float length = Length(a);
        return length < 1e-6f ? new PointF(1f, 0f) : Scale(a, 1f / length);
    }
}

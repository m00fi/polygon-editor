using PolygonEditor.Models;

namespace PolygonEditor.Constraints;

// Ciągłość G1/C1 w wierzchołku V między krawędzią wchodzącą A i wychodzącą B.
// Uchwyt to punkt wyznaczający styczną przy V: punkt kontrolny Béziera albo drugi koniec odcinka.
// G1: uchwyt wchodzący, V i uchwyt wychodzący leżą na prostej (V pomiędzy).
// C1: dodatkowo równe wektory styczne; pochodna Béziera to 3·(P1 − P0), a odcinka (End − Start),
// stąd skala 3 dla Béziera i 1 dla odcinka.
// Nie jest trzymane w ConstraintStore - solver tworzy je na bieżąco z ustawień wierzchołków.
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

                // TODO(łuki): styczna łuku wynika z jego geometrii - obsługa w kolejnym kroku,
                // na razie wierzchołki obok łuku są pomijane, żeby solver nie traktował łuku jak cięciwy.
                if (vertex.Incoming.IsArc || vertex.Outgoing.IsArc)
                    continue;

                yield return new ContinuityConstraint(vertex, vertex.Incoming, vertex.Outgoing);
            }
        }
    }

    public bool InvolvesEdge(Edge edge) => edge == Incoming || edge == Outgoing;

    private Handle In => Incoming.IsBezier ? new Handle(null, Incoming.Control2) : new Handle(Incoming.Start, null);
    private Handle Out => Outgoing.IsBezier ? new Handle(null, Outgoing.Control1) : new Handle(Outgoing.End, null);

    // Out = V + (V − In) · ratio przy C1.
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

        // Przesuwamy uchwyt, który nie jest kotwicą; jeśli oba są wolne - punkt kontrolny,
        // bo jego ruch nie psuje innych ograniczeń (wierzchołek mógłby rozjechać długości).
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

    // Przy G1 przesuwany uchwyt zachowuje swoją odległość od V (gdy jest zerowa - bierze drugą).
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

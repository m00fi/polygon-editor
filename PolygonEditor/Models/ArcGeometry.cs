namespace PolygonEditor.Models;

// Łuk okręgu: punkt(θ) = Center + Radius·(cos θ, sin θ) dla θ od StartAngle do StartAngle + SweepAngle.
// Kąty w radianach, oś Y w dół (dodatni Sweep = zgodnie z ruchem wskazówek zegara na ekranie).
public readonly record struct ArcGeometry(PointF Center, float Radius, float StartAngle, float SweepAngle)
{
    // G0 na obu końcach: półokrąg o środku w środku cięciwy i promieniu |cięciwa| / 2.
    // side = +1 wygina łuk w stronę normalnej (-dy, dx), side = -1 w przeciwną (jak Edge.InitializeControlPoints).
    public static ArcGeometry Semicircle(PointF start, PointF end, float side)
    {
        var center = new PointF((start.X + end.X) * 0.5f, (start.Y + end.Y) * 0.5f);
        float dx = end.X - start.X;
        float dy = end.Y - start.Y;
        float radius = MathF.Sqrt(dx * dx + dy * dy) * 0.5f;
        float startAngle = MathF.Atan2(start.Y - center.Y, start.X - center.X);

        // Obrót o +π/2 od kierunku do Start daje kierunek -(−dy, dx), więc dla side = +1 kręcimy w drugą stronę.
        float sweep = side > 0f ? -MathF.PI : MathF.PI;
        return new ArcGeometry(center, radius, startAngle, sweep);
    }

    // G1 na początku: łuk wychodzi ze start w kierunku tangent. Środek leży na prostej przez start
    // prostopadłej do stycznej i jest równo odległy od obu końców: C = start + λ·n, λ = |d|² / (2·n·d).
    // null, gdy styczna jest równoległa do cięciwy (łuk zdegenerowany do odcinka).
    public static ArcGeometry? TangentAtStart(PointF start, PointF end, PointF tangent)
    {
        float tLen = MathF.Sqrt(tangent.X * tangent.X + tangent.Y * tangent.Y);
        float dx = end.X - start.X;
        float dy = end.Y - start.Y;
        float dLen = MathF.Sqrt(dx * dx + dy * dy);
        if (tLen < 1e-4f || dLen < 1e-4f)
            return null;

        float nx = -tangent.Y / tLen;
        float ny = tangent.X / tLen;
        float nd = nx * dx + ny * dy;
        if (MathF.Abs(nd) < 1e-3f * dLen)
            return null;

        float lambda = dLen * dLen / (2f * nd);
        var center = new PointF(start.X + lambda * nx, start.Y + lambda * ny);
        float startAngle = MathF.Atan2(start.Y - center.Y, start.X - center.X);
        float endAngle = MathF.Atan2(end.Y - center.Y, end.X - center.X);

        // Przy rosnącym kącie łuk w punkcie start biegnie w kierunku (−ry, rx); zgodny ze styczną => sweep > 0.
        float rx = start.X - center.X;
        float ry = start.Y - center.Y;
        bool positive = -ry * tangent.X + rx * tangent.Y > 0f;

        float sweep = endAngle - startAngle;
        if (positive)
        {
            while (sweep <= 0f)
                sweep += 2f * MathF.PI;
        }
        else
        {
            while (sweep >= 0f)
                sweep -= 2f * MathF.PI;
        }

        return new ArcGeometry(center, MathF.Abs(lambda), startAngle, sweep);
    }

    // G1 na końcu: łuk dochodzi do end w kierunku tangent - to łuk od end do start ze styczną −tangent, odwrócony.
    public static ArcGeometry? TangentAtEnd(PointF start, PointF end, PointF tangent)
    {
        ArcGeometry? reversed = TangentAtStart(end, start, new PointF(-tangent.X, -tangent.Y));
        if (reversed is not { } r)
            return null;

        return new ArcGeometry(r.Center, r.Radius, r.StartAngle + r.SweepAngle, -r.SweepAngle);
    }

    // Kierunek ruchu wzdłuż łuku w punkcie t.
    public PointF TangentAt(float t)
    {
        float angle = StartAngle + SweepAngle * t;
        float sign = MathF.Sign(SweepAngle);
        return new PointF(-MathF.Sin(angle) * sign, MathF.Cos(angle) * sign);
    }

    public PointF PointAt(float t)
    {
        float angle = StartAngle + SweepAngle * t;
        return new PointF(Center.X + Radius * MathF.Cos(angle), Center.Y + Radius * MathF.Sin(angle));
    }
}

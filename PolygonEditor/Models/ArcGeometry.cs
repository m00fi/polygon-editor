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

    public PointF PointAt(float t)
    {
        float angle = StartAngle + SweepAngle * t;
        return new PointF(Center.X + Radius * MathF.Cos(angle), Center.Y + Radius * MathF.Sin(angle));
    }
}

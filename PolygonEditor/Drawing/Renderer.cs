using System.Drawing.Drawing2D;
using PolygonEditor.Constraints;
using PolygonEditor.Models;

namespace PolygonEditor.Drawing;

/// <summary>Rysuje scenę na Graphics (Bitmap + Bresenham lub DrawLine).</summary>
public sealed class Renderer
{
    public Color BackgroundColor { get; set; } = Color.White;
    public Color EdgeColor { get; set; } = Color.Black;
    public Color VertexFillColor { get; set; } = Color.LightBlue;
    public Color VertexStrokeColor { get; set; } = Color.Black;
    public float VertexRadius { get; set; } = 5f;

    private static readonly Color ParallelMarkColor = Color.Red;
    private static readonly Color LengthLabelColor = Color.Blue;

    public void Render(Graphics target, int width, int height, Models.Scene scene, bool useBresenham)
    {
        if (width <= 0 || height <= 0)
            return;

        using var bitmap = new Bitmap(width, height);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.Clear(BackgroundColor);
            g.SmoothingMode = SmoothingMode.None;

            foreach (Models.Polygon polygon in scene.Polygons)
            {
                foreach (Edge edge in polygon.Edges)
                    DrawEdge(bitmap, g, edge, useBresenham);

                foreach (Vertex vertex in polygon.Vertices)
                    DrawVertex(g, vertex.Position);
            }

            g.SmoothingMode = SmoothingMode.AntiAlias;
            foreach (IConstraint constraint in scene.Constraints.Constraints)
            {
                if (constraint is LengthConstraint length)
                    DrawLength(g, length);
                else if (constraint is ParallelConstraint parallel)
                    DrawParallel(g, parallel);
            }
        }

        target.DrawImageUnscaled(bitmap, 0, 0);
    }

    private void DrawEdge(Bitmap bitmap, Graphics g, Edge edge, bool useBresenham)
    {
        int x0 = (int)MathF.Round(edge.Start.Position.X);
        int y0 = (int)MathF.Round(edge.Start.Position.Y);
        int x1 = (int)MathF.Round(edge.End.Position.X);
        int y1 = (int)MathF.Round(edge.End.Position.Y);

        if (useBresenham)
            Bresenham.DrawLine(bitmap, x0, y0, x1, y1, EdgeColor);
        else
        {
            using var pen = new Pen(EdgeColor, 1f);
            g.DrawLine(pen, x0, y0, x1, y1);
        }
    }

    private void DrawVertex(Graphics g, PointF position)
    {
        float r = VertexRadius;
        var rect = new RectangleF(position.X - r, position.Y - r, r * 2f, r * 2f);
        using var fill = new SolidBrush(VertexFillColor);
        using var stroke = new Pen(VertexStrokeColor, 1.5f);
        g.FillEllipse(fill, rect);
        g.DrawEllipse(stroke, rect);
    }

    private void DrawParallel(Graphics g, ParallelConstraint parallel)
    {
        DrawParallelMarks(g, parallel.Edge1);
        DrawParallelMarks(g, parallel.Edge2);
    }

    private static void DrawParallelMarks(Graphics g, Edge edge)
    {
        if (!TryGetEdgeFrame(edge, out PointF mid, out float ux, out float uy, out float nx, out float ny))
            return;

        const float tickHalf = 7f;
        const float spacing = 2.5f;

        using var pen = new Pen(ParallelMarkColor, 1.6f);

        DrawTick(g, pen, mid, ux, uy, nx, ny, -spacing, tickHalf);
        DrawTick(g, pen, mid, ux, uy, nx, ny, spacing, tickHalf);
    }

    private static void DrawTick(
        Graphics g,
        Pen pen,
        PointF mid,
        float ux,
        float uy,
        float nx,
        float ny,
        float along,
        float tickHalf)
    {
        float cx = mid.X + ux * along;
        float cy = mid.Y + uy * along;
        g.DrawLine(
            pen,
            cx - nx * tickHalf,
            cy - ny * tickHalf,
            cx + nx * tickHalf,
            cy + ny * tickHalf);
    }

    private void DrawLength(Graphics g, LengthConstraint length)
    {
        Edge edge = length.Edge;
        if (!TryGetEdgeFrame(edge, out PointF mid, out float ux, out float uy, out float nx, out float ny))
            return;

        if (ny < 0f)
        {
            nx = -nx;
            ny = -ny;
        }

        const float offset = 14f;
        var anchor = new PointF(mid.X + nx * offset, mid.Y + ny * offset);

        float angleDeg = MathF.Atan2(uy, ux) * (180f / MathF.PI);
        if (angleDeg > 90f || angleDeg < -90f)
            angleDeg += 180f;

        string text = length.TargetLength.ToString("0");
        using var font = new Font("Segoe UI", 10f);
        using var brush = new SolidBrush(LengthLabelColor);
        SizeF size = g.MeasureString(text, font);

        GraphicsState state = g.Save();
        g.TranslateTransform(anchor.X, anchor.Y);
        g.RotateTransform(angleDeg);
        g.DrawString(text, font, brush, -size.Width * 0.5f, -size.Height * 0.5f);
        g.Restore(state);
    }

    private static bool TryGetEdgeFrame(
        Edge edge,
        out PointF mid,
        out float ux,
        out float uy,
        out float nx,
        out float ny)
    {
        mid = edge.Midpoint;
        float dx = edge.End.Position.X - edge.Start.Position.X;
        float dy = edge.End.Position.Y - edge.Start.Position.Y;
        float len = MathF.Sqrt(dx * dx + dy * dy);
        if (len < 1e-4f)
        {
            ux = uy = nx = ny = 0f;
            return false;
        }

        ux = dx / len;
        uy = dy / len;
        nx = -uy;
        ny = ux;
        return true;
    }
}

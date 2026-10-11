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

    public void Render(Graphics target, int width, int height, Models.Scene scene, bool useBresenham)
    {
        if (width <= 0 || height <= 0)
            return;

        using var bitmap = new Bitmap(width, height);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.Clear(BackgroundColor);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

            foreach (Models.Polygon polygon in scene.Polygons)
            {
                foreach (Edge edge in polygon.Edges)
                    DrawEdge(bitmap, g, edge, useBresenham);

                foreach (Vertex vertex in polygon.Vertices)
                    DrawVertex(g, vertex.Position);
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

        // Na razie wszystkie krawędzie jako odcinki (Bezier/łuk w kolejnych fazach).
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
}

namespace PolygonEditor.UI;

/// <summary>Panel canvasu z double buffering.</summary>
public sealed class CanvasPanel : Panel
{
    public CanvasPanel()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Color.White;
        Dock = DockStyle.Fill;
        TabStop = true;
    }
}

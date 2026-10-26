namespace PolygonEditor.UI;

// Canvas panel with double buffering so the scene does not flicker.
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

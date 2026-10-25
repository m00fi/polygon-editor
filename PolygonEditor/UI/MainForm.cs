using PolygonEditor.Constraints;
using PolygonEditor.Drawing;
using PolygonEditor.Interaction;
using PolygonEditor.Models;
using PolygonEditor.Scene;

namespace PolygonEditor.UI;

public partial class MainForm : Form
{
    private readonly Renderer _renderer = new();

    private Models.Scene _scene = DemoSceneFactory.Create();
    private bool _isDragging;
    private HitKind _dragKind = HitKind.None;
    private Vertex? _dragVertex;
    private Models.Polygon? _dragPolygon;
    private PointF _lastMouse;

    private ConstraintSolver _solver = new ConstraintSolver();
    private HitResult _menuHit = HitResult.None;
    private bool _isRemovingParallelConstraint = false;
    private bool _isAddingParallelConstraint = false;

    public MainForm()
    {
        InitializeComponent();
        UseBresenhamAlgorithm = true;
        WireCanvasEvents();
        UpdateStatus("Ready. LMB drag • RMB on edge adds vertex • RMB on vertex deletes • F1 help.");
    }

    /// <summary>true = Bresenham, false = Graphics.DrawLine.</summary>
    public bool UseBresenhamAlgorithm { get; private set; }

    private void WireCanvasEvents()
    {
        canvasPanel.MouseDown += CanvasPanel_MouseDown;
        canvasPanel.MouseMove += CanvasPanel_MouseMove;
        canvasPanel.MouseUp += CanvasPanel_MouseUp;
        canvasPanel.Resize += (_, _) => canvasPanel.Invalidate();
    }

    private void RadioBresenham_CheckedChanged(object? sender, EventArgs e)
    {
        if (radioBresenham.Checked)
        {
            UseBresenhamAlgorithm = true;
            canvasPanel.Invalidate();
            UpdateStatus("Line algorithm: Bresenham (custom).");
        }
    }

    private void RadioLibrary_CheckedChanged(object? sender, EventArgs e)
    {
        if (radioLibrary.Checked)
        {
            UseBresenhamAlgorithm = false;
            canvasPanel.Invalidate();
            UpdateStatus("Line algorithm: library (Graphics.DrawLine).");
        }
    }

    private void MenuResetScene_Click(object? sender, EventArgs e)
    {
        ResetScene();
        UpdateStatus("Scene reset to demo.");
    }

    private void MenuHelp_Click(object? sender, EventArgs e)
    {
        // HelpForm - faza 5.
        MessageBox.Show(
            this,
            "LMB: drag vertex or polygon.\n" +
            "RMB on edge: add vertex at midpoint.\n" +
            "RMB on vertex: delete vertex.\n" +
            "Ctrl+R: reset scene.",
            "Help - Polygon Editor",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void CanvasPanel_Paint(object? sender, PaintEventArgs e)
    {
        _renderer.Render(
            e.Graphics,
            canvasPanel.ClientSize.Width,
            canvasPanel.ClientSize.Height,
            _scene,
            UseBresenhamAlgorithm);
    }

    private void CanvasPanel_MouseDown(object? sender, MouseEventArgs e)
    {
        var point = new PointF(e.X, e.Y);
        var ipoint = new Point(e.X, e.Y);
        HitResult hit = HitTester.HitTest(_scene, point);

        if (IsPickingSecondEdge && (e.Button != MouseButtons.Left || hit.Kind != HitKind.Edge))
        {
            CancelEdgePicking();
            return;
        }

        if (_isRemovingParallelConstraint)
        {
            if (hit.Kind == HitKind.Edge && hit.Polygon is not null && hit.Edge is not null)
            {
                var menuHit = _menuHit;
                if (menuHit.Edge is null)
                    throw new InvalidOperationException("Menu hit edge is null while removing parallel constraint.");

                _isRemovingParallelConstraint = false;
                _scene.Constraints.RemoveParallelConstraint(menuHit.Edge, hit.Edge);
                _solver.Solve(_scene.Constraints, new ConstraintAnchor());
                canvasPanel.Invalidate();
                UpdateStatus("Parallel constraint removed.");
            }
            return;
        }

        if(_isAddingParallelConstraint)
        {
            if (hit.Kind == HitKind.Edge && hit.Polygon is not null && hit.Edge is not null)
            {
                var menuHit = _menuHit;
                if (menuHit.Edge is null)
                    throw new InvalidOperationException("Menu hit edge is null while adding parallel constraint.");

                _isAddingParallelConstraint = false;
                if (!ParallelConstraint.CanCreate(menuHit.Edge, hit.Edge, out string? reason))
                {
                    UpdateStatus($"Parallel constraint not added: {reason}");
                    return;
                }

                _scene.Constraints.AddConstraint(new ParallelConstraint(menuHit.Edge, hit.Edge));
                _solver.Solve(_scene.Constraints, new ConstraintAnchor());
                canvasPanel.Invalidate();
                UpdateStatus("Parallel constraint added.");
            }
            return;
        }



        if (e.Button == MouseButtons.Right)
        {
            HandleRightClick(hit, ipoint);
            return;
        }

        if (e.Button != MouseButtons.Left)
            return;

        _lastMouse = point;
        _isDragging = false;
        _dragKind = HitKind.None;
        _dragVertex = null;
        _dragPolygon = null;


        if (hit.Kind == HitKind.Vertex && hit.Vertex is not null)
        {
            _isDragging = true;
            _dragKind = HitKind.Vertex;
            _dragVertex = hit.Vertex;
            _dragPolygon = hit.Polygon;
            // Wierzchołek od razu podąża za kursorem.
            _dragVertex.Position = point;
            canvasPanel.Invalidate();
            UpdateStatus("Dragging vertex.");
        }
        else if (hit.Kind == HitKind.Polygon && hit.Polygon is not null)
        {
            _isDragging = true;
            _dragKind = HitKind.Polygon;
            _dragPolygon = hit.Polygon;
            UpdateStatus("Dragging polygon.");
        }
        else if (hit.Kind == HitKind.Edge)
        {
            UpdateStatus("Edge hit - RMB adds a vertex at midpoint.");
        }
    }

    private void HandleRightClick(HitResult hit, Point hitPoint)
    {
        _menuHit = hit;
        if (hit.Kind == HitKind.Edge && hit.Polygon is not null && hit.Edge is not null)
        {
            edgeContexMenu.Show(canvasPanel, hitPoint);
            return;
        }

        if (hit.Kind == HitKind.Vertex && hit.Polygon is not null && hit.Vertex is not null)
        {
            vertexContextMenu.Show(canvasPanel, hitPoint);
            return;
        }

        UpdateStatus("RMB: click an edge or a vertex to modify it.");
    }

    private void CanvasPanel_MouseMove(object? sender, MouseEventArgs e)
    {
        var point = new PointF(e.X, e.Y);

        if (!_isDragging)
        {
            HitResult hover = HitTester.HitTest(_scene, point);
            canvasPanel.Cursor = hover.Kind switch
            {
                HitKind.Vertex => Cursors.Hand,
                HitKind.Polygon => Cursors.SizeAll,
                HitKind.Edge => Cursors.Cross,
                _ => Cursors.Default
            };
            return;
        }

        if (_dragKind == HitKind.Vertex && _dragVertex is not null)
        {
            _dragVertex.Position = point;
            _solver.Solve(_scene.Constraints, new ConstraintAnchor { FixedVertex = _dragVertex });
            canvasPanel.Invalidate();
        }
        else if (_dragKind == HitKind.Polygon && _dragPolygon is not null)
        {
            float dx = point.X - _lastMouse.X;
            float dy = point.Y - _lastMouse.Y;
            _dragPolygon.Translate(dx, dy);
            _lastMouse = point;
            canvasPanel.Invalidate();
        }
    }

    private void CanvasPanel_MouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left || !_isDragging)
            return;

        _isDragging = false;
        _dragKind = HitKind.None;
        _dragVertex = null;
        _dragPolygon = null;
        UpdateStatus("Ready.");
    }

    private void ResetScene()
    {
        _isDragging = false;
        _dragKind = HitKind.None;
        _dragVertex = null;
        _dragPolygon = null;
        _isAddingParallelConstraint = false;
        _isRemovingParallelConstraint = false;
        _scene = DemoSceneFactory.Create();
        canvasPanel.Invalidate();
    }

    private void UpdateStatus(string message)
    {
        statusLabel.Text = message;
    }

    private void removeVertex(object sender, EventArgs e)
    {
        var hit = _menuHit;

        if (hit.Kind == HitKind.Vertex && hit.Polygon is not null && hit.Vertex is not null)
        {
            if (hit.Polygon.CanRemoveVertex(hit.Vertex))
            {
                var edgesInvolved = hit.Polygon.EdgesAffectedByVertexRemoval(hit.Vertex);

                foreach (var edge in edgesInvolved)
                    _scene.Constraints.RemoveAllInvolvingEdge(edge);

                hit.Polygon.RemoveVertex(hit.Vertex);
                canvasPanel.Invalidate();
                UpdateStatus("Vertex deleted.");
            }
            else
            {
                UpdateStatus("Cannot delete vertex - polygon must keep at least 3 vertices.");
            }
        }
    }

    private void addVertex(object sender, EventArgs e)
    {
        var hit = _menuHit;

        if (hit.Kind == HitKind.Edge && hit.Polygon is not null && hit.Edge is not null)
        {
            _scene.Constraints.RemoveAllInvolvingEdge(hit.Edge);
            hit.Polygon.InsertVertexAtMidpoint(hit.Edge);
            canvasPanel.Invalidate();
            UpdateStatus("Vertex added at edge midpoint.");
        }
    }

    private void edgeContexMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
    {
        Edge? edge = _menuHit.Edge;
        if (edge is null)
        {
            e.Cancel = true;
            return;
        }
        bool hasLength = _scene.Constraints.HasLength(edge);
        bool hasParallel = _scene.Constraints.HasParallel(edge);
        removeLengthConstraintToolStripMenuItem.Enabled = hasLength;
        removeParallelConstraintToolStripMenuItem.Enabled = hasParallel;
    }

    private static bool TryAskLength(float defaultLength, out float length)
    {
        length = defaultLength;

        using var dialog = new Form
        {
            Text = "Length constraint",
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            ClientSize = new Size(260, 100),
            MaximizeBox = false,
            MinimizeBox = false
        };

        var label = new Label { Text = "Length:", Left = 12, Top = 15, AutoSize = true };
        var box = new TextBox
        {
            Left = 80,
            Top = 12,
            Width = 160,
            Text = defaultLength.ToString("0.##")
        };
        var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Left = 80, Top = 50, Width = 75 };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Left = 165, Top = 50, Width = 75 };

        dialog.Controls.AddRange([label, box, ok, cancel]);
        dialog.AcceptButton = ok;
        dialog.CancelButton = cancel;

        if (dialog.ShowDialog() != DialogResult.OK)
            return false;

        return float.TryParse(box.Text, out length) && length >= 1f;
    }

    private void addLengthConstraint(object sender, EventArgs e)
    {
        var hit = _menuHit;
        float length;
        float defaultLength = hit.Edge?.Length ?? 100f;
        if (hit.Edge != null && TryAskLength(defaultLength, out length))
        {
            _scene.Constraints.AddConstraint(new LengthConstraint(hit.Edge, length));
            _solver.Solve(_scene.Constraints, new ConstraintAnchor());
            canvasPanel.Invalidate();
            UpdateStatus($"Length constraint set to {length:0.##}.");
        }
        else
        {
            UpdateStatus("Length constraint cancelled or invalid.");
        }
    }

    private void removeLength(object sender, EventArgs e)
    {
        var hit = _menuHit;
        if (hit.Edge != null)
        {
            _scene.Constraints.RemoveLengthConstraint(hit.Edge);
            canvasPanel.Invalidate();
            UpdateStatus("Length constraint removed.");
        }
    }

    private void removeParallel(object sender, EventArgs e)
    {
        _isAddingParallelConstraint = false;
        _isRemovingParallelConstraint = true;
        UpdateStatus("Remove parallel: left-click the second edge (Esc or click elsewhere to cancel).");
    }

    private void addParallel(object sender, EventArgs e)
    {
        _isRemovingParallelConstraint = false;
        _isAddingParallelConstraint = true;
        UpdateStatus("Add parallel: left-click the second edge (Esc or click elsewhere to cancel).");
    }

    private bool IsPickingSecondEdge => _isAddingParallelConstraint || _isRemovingParallelConstraint;

    private void CancelEdgePicking()
    {
        if (!IsPickingSecondEdge)
            return;

        _isAddingParallelConstraint = false;
        _isRemovingParallelConstraint = false;
        UpdateStatus("Edge selection cancelled.");
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape && IsPickingSecondEdge)
        {
            CancelEdgePicking();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }
}

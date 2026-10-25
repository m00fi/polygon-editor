using PolygonEditor.Constraints;
using PolygonEditor.Drawing;
using PolygonEditor.Interaction;
using PolygonEditor.Models;
using PolygonEditor.Scene;
using System.Globalization;

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
        UpdateStatus("Ready. LMB: drag vertex/polygon • RMB: edge/vertex menu • F1: help.");
    }

    // true = Bresenham, false = Graphics.DrawLine.
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
        // HelpForm
        MessageBox.Show(
            this,
            "LMB: drag vertex or polygon.\n" +
            "RMB on edge: edge menu (add vertex, length/parallel constraints).\n" +
            "RMB on vertex: vertex menu (delete vertex).\n" +
            "Esc: cancel picking the second edge.\n" +
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
                if (_scene.Constraints.RemoveParallelConstraint(menuHit.Edge, hit.Edge))
                {
                    canvasPanel.Invalidate();
                    UpdateStatus("Parallel constraint removed.");
                }
                else
                {
                    UpdateStatus("These edges are not parallel-constrained to each other.");
                }
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

                // Pierwsza wybrana krawędź stoi w miejscu, druga się obraca.
                bool added = TryAddConstraint(
                    new ParallelConstraint(menuHit.Edge, hit.Edge),
                    new ConstraintAnchor { FixedEdge = menuHit.Edge });
                UpdateStatus(added
                    ? "Parallel constraint added."
                    : "Parallel constraint not added: it conflicts with existing constraints.");
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


        if (hit.Kind == HitKind.Vertex && hit.Vertex is not null && hit.Polygon is not null)
        {
            _isDragging = true;
            _dragKind = HitKind.Vertex;
            _dragVertex = hit.Vertex;
            _dragPolygon = hit.Polygon;
            UpdateStatus("Dragging vertex.");
            // Wierzchołek od razu podąża za kursorem (z uwzględnieniem ograniczeń).
            DragVertexTo(hit.Vertex, hit.Polygon, point);
            canvasPanel.Invalidate();
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
            UpdateStatus("Edge hit - RMB opens the edge menu.");
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

        if (_dragKind == HitKind.Vertex && _dragVertex is not null && _dragPolygon is not null)
        {
            DragVertexTo(_dragVertex, _dragPolygon, point);
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

    // Gdy ograniczeń nie da się spełnić, ruch jest wycofywany i przesuwany jest cały wielokąt
    // (translacja zachowuje długości i kierunki krawędzi).
    private void DragVertexTo(Vertex vertex, Models.Polygon polygon, PointF point)
    {
        SceneSnapshot snapshot = SceneSnapshot.Capture(_scene);
        PointF previous = vertex.Position;

        vertex.Position = point;
        if (_solver.Solve(_scene.Constraints, new ConstraintAnchor { FixedVertex = vertex }))
            return;

        snapshot.Restore();
        polygon.Translate(point.X - previous.X, point.Y - previous.Y);
        UpdateStatus("Constraints cannot be satisfied - moving the whole polygon instead.");
    }

    // Dodaje ograniczenie i rozwiązuje scenę; przy sprzeczności przywraca stan sprzed dodania.
    private bool TryAddConstraint(IConstraint constraint, ConstraintAnchor anchor)
    {
        SceneSnapshot snapshot = SceneSnapshot.Capture(_scene);
        _scene.Constraints.AddConstraint(constraint);

        bool solved = _solver.Solve(_scene.Constraints, anchor);
        if (!solved)
            snapshot.Restore();

        canvasPanel.Invalidate();
        return solved;
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
            Text = defaultLength.ToString("0.##", CultureInfo.InvariantCulture)
        };
        var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Left = 80, Top = 50, Width = 75 };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Left = 165, Top = 50, Width = 75 };

        dialog.Controls.AddRange([label, box, ok, cancel]);
        dialog.AcceptButton = ok;
        dialog.CancelButton = cancel;

        if (dialog.ShowDialog() != DialogResult.OK)
            return false;

        // Akceptujemy zarówno kropkę, jak i przecinek dziesiętny.
        string text = box.Text.Trim().Replace(',', '.');
        return float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out length) && length >= 1f;
    }

    private void addLengthConstraint(object sender, EventArgs e)
    {
        var hit = _menuHit;
        float length;
        float defaultLength = hit.Edge?.Length ?? 100f;
        if (hit.Edge != null && TryAskLength(defaultLength, out length))
        {
            // Start krawędzi stoi w miejscu, End przesuwa się wzdłuż krawędzi.
            bool added = TryAddConstraint(
                new LengthConstraint(hit.Edge, length),
                new ConstraintAnchor { FixedVertex = hit.Edge.Start });
            UpdateStatus(added
                ? $"Length constraint set to {length:0.##}."
                : "Length constraint not set: it conflicts with existing constraints.");
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
        Edge? edge = _menuHit.Edge;
        if (edge is null)
            return;

        // Jedna para - usuwamy od razu, bez wybierania drugiej krawędzi.
        IReadOnlyList<ParallelConstraint> pairs = _scene.Constraints.GetParallelConstraints(edge);
        if (pairs.Count == 1)
        {
            _scene.Constraints.RemoveConstraint(pairs[0]);
            canvasPanel.Invalidate();
            UpdateStatus("Parallel constraint removed.");
            return;
        }

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

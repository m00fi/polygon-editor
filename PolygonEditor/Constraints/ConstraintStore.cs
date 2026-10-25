using System;
using System.Collections.Generic;
using System.Text;
using PolygonEditor.Models;

namespace PolygonEditor.Constraints;

public sealed class ConstraintStore
{
    private List<IConstraint> constraints = [];

    public IReadOnlyList<IConstraint> Constraints => constraints;
    public ConstraintStore()
    {
    }

    public static ConstraintStore Create() => new ConstraintStore();

    public void AddConstraint(IConstraint constraint)
    {
        ArgumentNullException.ThrowIfNull(constraint);
        if (constraint is LengthConstraint length)
        {
            // Usuń poprzedni Length na tej samej krawędzi
            constraints.RemoveAll(c => c is LengthConstraint lc && lc.Edge == length.Edge);
            constraints.Add(length);
            return;
        }
        if (constraint is ParallelConstraint parallel)
        {
            constraints.RemoveAll(c =>
                c is ParallelConstraint pc
                && ((pc.Edge1 == parallel.Edge1 && pc.Edge2 == parallel.Edge2)
                    || (pc.Edge1 == parallel.Edge2 && pc.Edge2 == parallel.Edge1)));
            constraints.Add(parallel);
            return;
        }
        constraints.Add(constraint);
    }
    
    public void RemoveConstraint(IConstraint constraint)
    {
        constraints.Remove(constraint);
    }

    public void RemoveAllInvolvingEdge(Edge edge)
    {
        constraints.RemoveAll(c => c.InvolvesEdge(edge));
    }

    public void RemoveLengthConstraint(Edge edge)
    {
        constraints.RemoveAll(c => c is LengthConstraint lc && lc.Edge == edge);
    }

    // Zwraca true, gdy istniała równoległość między tymi krawędziami.
    public bool RemoveParallelConstraint(Edge edge1, Edge edge2)
    {
        return constraints.RemoveAll(c =>
            c is ParallelConstraint pc
            && ((pc.Edge1 == edge1 && pc.Edge2 == edge2)
                || (pc.Edge1 == edge2 && pc.Edge2 == edge1))) > 0;
    }

    public IReadOnlyList<ParallelConstraint> GetParallelConstraints(Edge edge) =>
        constraints.OfType<ParallelConstraint>().Where(pc => pc.InvolvesEdge(edge)).ToList();

    public void Clear() => constraints.Clear();

    // Przywraca listę ograniczeń zapamiętaną w SceneSnapshot.
    internal void RestoreFrom(IEnumerable<IConstraint> saved)
    {
        constraints = [.. saved];
    }

    public bool HasLength(Edge edge) =>
    constraints.Exists(c => c is LengthConstraint && c.InvolvesEdge(edge));

    public bool HasParallel(Edge edge) =>
        constraints.Exists(c => c is ParallelConstraint && c.InvolvesEdge(edge));
}

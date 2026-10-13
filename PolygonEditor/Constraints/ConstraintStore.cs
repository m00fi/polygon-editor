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
        if(constraints.Exists(c => c.InvolvedEdges == constraint.InvolvedEdges && c.GetType() == constraint.GetType()))
        {
            // Ograniczenie tego samego typu dla tych samych krawędzi już istnieje

            var existingConstraint = constraints.Find(c => c.InvolvedEdges == constraint.InvolvedEdges && c.GetType() == constraint.GetType());
            if(existingConstraint != null)
            {
                RemoveConstraint(existingConstraint);
                AddConstraint(constraint);
            }
        }
        else
        {
            constraints.Add(constraint);
        }
    }
    
    public void RemoveConstraint(IConstraint constraint)
    {
        constraints.Remove(constraint);
    }

    public void RemoveAllInvolvingEdge(Edge edge)
    {
        constraints.RemoveAll(c => c.InvolvesEdge(edge));
    }
}

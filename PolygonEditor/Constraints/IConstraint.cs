using PolygonEditor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PolygonEditor.Constraints
{
    public interface IConstraint
    {
        Guid Id { get; }
        IReadOnlyList<Edge> InvolvedEdges { get; }
        bool InvolvesEdge(Edge edge);

        // one fixing step: moves the vertices that are further from the anchor (by rank)
        void Satisfy(SolveContext context);

        bool IsSatisfied();
    }

    // the thing that has to stay in place while solving (e.g. the dragged vertex)
    public readonly struct ConstraintAnchor
    {
        public Vertex? FixedVertex { get; init; }
        public Edge? FixedEdge { get; init; }
        public ControlPoint? FixedControlPoint { get; init; }
    }
}

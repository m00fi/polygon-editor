using PolygonEditor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PolygonEditor.Constraints
{
    /// <summary>Interfejs reprezentujący ograniczenie w systemie edycji wielokątów.</summary>
    public interface IConstraint
    {
        Guid Id { get; }
        IReadOnlyList<Edge> InvolvedEdges { get; }
        bool InvolvesEdge(Edge edge);
        void Satisfy(ConstraintAnchor anchor);

    }

    public readonly struct ConstraintAnchor
    {
        public Vertex? FixedVertex { get; init; }
        public Edge? FixedEdge { get; init; }
    }
}

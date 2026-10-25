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

        // Jeden krok korekty: przesuwa wierzchołki dalsze od kotwicy (wg rang z kontekstu).
        void Satisfy(SolveContext context);

        bool IsSatisfied();
    }

    // Element trzymany w miejscu podczas rozwiązywania (np. przeciągany wierzchołek).
    public readonly struct ConstraintAnchor
    {
        public Vertex? FixedVertex { get; init; }
        public Edge? FixedEdge { get; init; }
        public ControlPoint? FixedControlPoint { get; init; }
    }
}

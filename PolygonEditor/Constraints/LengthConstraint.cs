using PolygonEditor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PolygonEditor.Constraints
{
    public sealed class LengthConstraint : IConstraint
    {
        public Guid Id { get; } = Guid.NewGuid();
        public Edge Edge { get; }
        public float TargetLength { get; set; }

        public IReadOnlyList<Edge> InvolvedEdges => [Edge];
        public bool InvolvesEdge(Edge edge) => edge == Edge;

        public void Satisfy(ConstraintAnchor anchor)
        {
            // Zachowaj kierunek krawędzi.
            // Jeśli Start jest kotwicą -> przesuń End na odległość TargetLength.
            // Jeśli End jest kotwicą -> przesuń Start.
            // Jeśli żadna nie jest kotwicą -> przesuń End (domyślnie).

            if (anchor.FixedVertex != null)
            {
                var fixedVertex = anchor.FixedVertex;
                var otherVertex = Edge.Start == fixedVertex ? Edge.End : Edge.Start;

                // var direction = (otherVertex.Position - fixedVertex.Position).Normalized();


                //otherVertex.Position = fixedVertex.Position + direction * TargetLength;
            }
        }
    }
}

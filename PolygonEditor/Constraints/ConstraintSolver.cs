using System;
using System.Collections.Generic;
using System.Text;

namespace PolygonEditor.Constraints;

internal class ConstraintSolver
{
    public ConstraintSolver() { }

    public void Solve(ConstraintStore store, ConstraintAnchor anchor)
    {
        foreach (var constraint in store.Constraints)
        {
            constraint.Satisfy(anchor);
        }
    }
}

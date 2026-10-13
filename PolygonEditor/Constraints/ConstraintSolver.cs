using System;
using System.Collections.Generic;
using System.Text;

namespace PolygonEditor.Constraints;

internal class ConstraintSolver
{
    public ConstraintSolver() { }

    public void Solve(ConstraintStore store, ConstraintAnchor anchor)
    {
        for(int i = 0; i < 30; i++)
        {
            foreach (var constraint in store.Constraints)
            {
                constraint.Satisfy(anchor);
            }
        }
    }
}

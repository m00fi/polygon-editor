using System;
using System.Collections.Generic;
using System.Text;

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
        constraints.Add(constraint);
    }


}

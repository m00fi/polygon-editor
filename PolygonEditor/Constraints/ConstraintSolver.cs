namespace PolygonEditor.Constraints;

// Relaxation solver: every iteration each constraint does one small fix,
// we stop when all of them are satisfied or we run out of iterations.
internal sealed class ConstraintSolver
{
    public const int MaxIterations = 100;

    // user constraints (from ConstraintStore) plus continuity built from the vertex settings
    // returns true if everything is satisfied at the end
    public bool Solve(Models.Scene scene, ConstraintAnchor anchor)
    {
        List<IConstraint> constraints = [.. scene.Constraints.Constraints, .. ContinuityConstraint.CreateAll(scene)];
        if (AllSatisfied(constraints))
            return true;

        SolveContext context = SolveContext.Build(constraints, anchor);
        for (int i = 0; i < MaxIterations; i++)
        {
            foreach (IConstraint constraint in constraints)
                constraint.Satisfy(context);

            if (AllSatisfied(constraints))
                return true;
        }

        return false;
    }

    private static bool AllSatisfied(List<IConstraint> constraints)
    {
        foreach (IConstraint constraint in constraints)
        {
            if (!constraint.IsSatisfied())
                return false;
        }

        return true;
    }
}

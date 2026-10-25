namespace PolygonEditor.Constraints;

// Relaksacja: w każdej iteracji każde ograniczenie robi jeden krok korekty,
// aż wszystkie będą spełnione albo skończy się limit iteracji.
internal sealed class ConstraintSolver
{
    public const int MaxIterations = 100;

    // Ograniczenia użytkownika (ConstraintStore) + ciągłość wynikająca z ustawień wierzchołków.
    // Zwraca true, gdy po rozwiązaniu wszystkie są spełnione.
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

namespace PolygonEditor.Constraints;

// Relaksacja: w każdej iteracji każde ograniczenie robi jeden krok korekty,
// aż wszystkie będą spełnione albo skończy się limit iteracji.
internal sealed class ConstraintSolver
{
    public const int MaxIterations = 100;

    // Zwraca true, gdy po rozwiązaniu wszystkie ograniczenia są spełnione.
    public bool Solve(ConstraintStore store, ConstraintAnchor anchor)
    {
        if (AllSatisfied(store))
            return true;

        SolveContext context = SolveContext.Build(store, anchor);
        for (int i = 0; i < MaxIterations; i++)
        {
            foreach (IConstraint constraint in store.Constraints)
                constraint.Satisfy(context);

            if (AllSatisfied(store))
                return true;
        }

        return false;
    }

    private static bool AllSatisfied(ConstraintStore store)
    {
        foreach (IConstraint constraint in store.Constraints)
        {
            if (!constraint.IsSatisfied())
                return false;
        }

        return true;
    }
}

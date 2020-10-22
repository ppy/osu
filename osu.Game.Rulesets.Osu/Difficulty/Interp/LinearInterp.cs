
namespace osu.Game.Rulesets.Osu.Difficulty.Interp
{

    /// <summary>
    /// Define a function f(x) by specifying a list of points X and Values, where Values[i] = f(X[i])
    /// X must be sorted and contain distinct points
    /// Points within the grid are interpolated linearly, points beyond the grid are extrapolated linearly based on the slope at the boundary.
    /// </summary>
    public class LinearInterp
    {
        internal static (int, double) FindInterpCoeffs(double[] values, double target)
        {
            int lowerBoundIndex = values.Length - 2;
            while (lowerBoundIndex > 0 && values[lowerBoundIndex] > target)
                --lowerBoundIndex;

            int upperBoundIndex = lowerBoundIndex + 1;

            double lowerBound = values[lowerBoundIndex];
            double upperBound = values[upperBoundIndex];

            double coeff = (target - lowerBound) / (upperBound - lowerBound);

            return (lowerBoundIndex, coeff);
        }

        public double[] X { get; }
        public double[] Values { get; }

        public LinearInterp(double[] x, double[] values)
        {
            X = x;
            Values = values;
        }

        public double Evaluate(double x)
        {
            (int index, double t) = FindInterpCoeffs(X, x);

            return (1 - t) * Values[index] + t * Values[index + 1];
        }
    }
}

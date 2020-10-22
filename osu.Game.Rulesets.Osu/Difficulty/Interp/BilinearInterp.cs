namespace osu.Game.Rulesets.Osu.Difficulty.Interp
{
    /// <summary>
    /// Define a function f(x,y) by specifying a grid of points X, Y and Values, where Values[i,j] = f(X[i],Y[j])
    /// X and Y must be sorted and contain distinct points
    /// Points within the grid are interpolated linearly, points beyond the grid are extrapolated linearly based on the slope at the boundary.
    /// </summary>
    public class BilinearInterp
    {
        public double[] X { get; }
        public double[] Y { get; }

        public double[,] Values { get; }

        public BilinearInterp(double[] x, double[] y, double[,] values)
        {
            X = x;
            Y = y;
            Values = values;
        }


        public double Evaluate(double x, double y)
        {
            (int xIndex, double xCoeff) = LinearInterp.FindInterpCoeffs(X, x);
            (int yIndex, double yCoeff) = LinearInterp.FindInterpCoeffs(Y, y);

            double v0 = (1 - yCoeff) * Values[xIndex, yIndex] + yCoeff * Values[xIndex, yIndex + 1];
            double v1 = (1 - yCoeff) * Values[xIndex + 1, yIndex] + yCoeff * Values[xIndex + 1, yIndex + 1];

            return (1 - xCoeff) * v0 + xCoeff * v1;
        }
    }
}

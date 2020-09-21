namespace osu.Game.Rulesets.Osu.Difficulty.Interp
{
    /// <summary>
    /// Define a function f(x,y,z) by specifying a grid of points X, Y, Z and Values, where Values[i,j,k] = f(X[i],Y[j],Z[k])
    /// X, Y and Z must be sorted and contain distinct points
    /// Points within the grid are interpolated linearly, points beyond the grid are extrapolated linearly based on the slope at the boundary.
    /// </summary>
    public class TrilinearInterp
    {
        public double[] X { get; }
        public double[] Y { get; }
        public double[] Z { get; }
        public double[,,] Values { get; }


        public TrilinearInterp(double[] x, double[] y, double[] z, double[,,] values)
        {
            X = x;
            Y = y;
            Z = z;
            Values = values;
        }

        public double Evaluate(double x, double y, double z)
        {
            (int xIndex, double xCoeff) = LinearInterp.FindInterpCoeffs(X, x);
            (int yIndex, double yCoeff) = LinearInterp.FindInterpCoeffs(Y, y);
            (int zIndex, double zCoeff) = LinearInterp.FindInterpCoeffs(Z, z);

            double v00 = (1 - zCoeff) * Values[xIndex, yIndex, zIndex] + (zCoeff) * Values[xIndex, yIndex, zIndex + 1];
            double v01 = (1 - zCoeff) * Values[xIndex, yIndex + 1, zIndex] + (zCoeff) * Values[xIndex, yIndex + 1, zIndex + 1];

            double v10 = (1 - zCoeff) * Values[xIndex + 1, yIndex, zIndex] + (zCoeff) * Values[xIndex + 1, yIndex, zIndex + 1];
            double v11 = (1 - zCoeff) * Values[xIndex + 1, yIndex + 1, zIndex] + (zCoeff) * Values[xIndex + 1, yIndex + 1, zIndex + 1];

            double v0 = (1 - yCoeff) * v00 + yCoeff * v01;
            double v1 = (1 - yCoeff) * v10 + yCoeff * v11;

            return (1 - xCoeff) * v0 + xCoeff * v1;
        }
    }
}

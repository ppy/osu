namespace osu.Game.Rulesets.Osu.Difficulty.Interp
{
    public struct HermiteSpline
    {
        public HermiteSpline(double x0, double val0, double d0, double x1, double val1, double d1)
        {
            double scale = 1 / (x1 - x0);
            double scale2 = scale * scale;

            X0 = x0;
            X1 = x1;
            D1 = d1;
            Val1=val1;

            C0 = val0;
            C1 = d0;
            C2 = 3 * (val1 - val0) * scale2 - (2 * d0 + d1) * scale;
            C3 = (2 * (val0 - val1) * scale + d0 + d1) * scale2;
        }
        public double C0, C1, C2, C3, X0, X1, D1, Val1;

        public double Evaluate(double x)
        {
            if (x > X1)
                return (x - X1) * D1 + Val1;
            if (x < X0)
                return (x - X0) * C1 + C0;

            double t = (x - X0);
            double t2 = t * t;
            double t3 = t2 * t;

            //return C0 + t * (C1 + t * (C2 + t * C3));

            return C0 + C1 * t + C2 * t2 + C3 * t3;
        }
    }
}

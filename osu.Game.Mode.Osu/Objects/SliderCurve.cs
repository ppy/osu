using System.Collections.Generic;
using OpenTK;
using System.Linq;
using System.Diagnostics;
using osu.Framework.MathUtils;

namespace osu.Game.Modes.Osu.Objects
{
    public class SliderCurve
    {
        public double Length;

        public List<Vector2> Path;

        public CurveTypes CurveType;

        private List<Vector2> calculatedPath = new List<Vector2>();
        private List<double> cumulativeLength = new List<double>();

        private List<Vector2> calculateSubpath(List<Vector2> subpath)
        {
            switch (CurveType)
            {
                case CurveTypes.Linear:
                    return subpath;
                default:
                    return new BezierApproximator(subpath).CreateBezier();
            }
        }

        public void Calculate()
        {
            calculatedPath.Clear();

            // Sliders may consist of various subpaths separated by two consecutive vertices
            // with the same position. The following loop parses these subpaths and computes
            // their shape independently, consecutively appending them to calculatedPath.
            List<Vector2> subpath = new List<Vector2>();
            for (int i = 0; i < Path.Count; ++i)
            {
                subpath.Add(Path[i]);
                if (i == Path.Count - 1 || Path[i] == Path[i + 1])
                {
                    // If we already constructed a subpath previously, then the new subpath
                    // will have as starting position the end position of the previous subpath.
                    // Hence we can and should remove the previous endpoint to avoid a segment
                    // with 0 length.
                    if (calculatedPath.Count > 0)
                        calculatedPath.RemoveAt(calculatedPath.Count - 1);

                    calculatedPath.AddRange(calculateSubpath(subpath));
                    subpath.Clear();
                }
            }
            
            cumulativeLength.Clear();
            cumulativeLength.Add(Length = 0);
            for (int i = 0; i < calculatedPath.Count - 1; ++i)
            {
                double d = (calculatedPath[i + 1] - calculatedPath[i]).Length;

                Debug.Assert(d >= 0, "Cumulative lengths have to be strictly increasing.");
                cumulativeLength.Add(Length += d);
            }
        }

        public Vector2 PositionAt(double progress)
        {
            progress = MathHelper.Clamp(progress, 0, 1);

            double d = progress * Length;
            int i = cumulativeLength.BinarySearch(d);
            if (i < 0) i = ~i;

            if (i >= calculatedPath.Count)
                return calculatedPath.Last();

            if (i <= 0)
                return calculatedPath.First();

            Vector2 p0 = calculatedPath[i - 1];
            Vector2 p1 = calculatedPath[i];

            double d0 = cumulativeLength[i - 1];
            double d1 = cumulativeLength[i];

            // Avoid division by and almost-zero number in case two points are extremely close to each other.
            if (Precision.AlmostEquals(d0, d1))
                return p0;

            double w = (d - d0) / (d1 - d0);
            return p0 + (p1 - p0) * (float)w;
        }
    }
}
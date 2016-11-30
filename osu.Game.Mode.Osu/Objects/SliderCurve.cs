using System.Collections.Generic;
using OpenTK;

namespace osu.Game.Modes.Osu.Objects
{
    public class SliderCurve
    {
        public double Length;

        public List<Vector2> Path;

        public CurveTypes CurveType;

        private List<Vector2> calculatedPath;

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
            calculatedPath = new List<Vector2>();

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
        }

        public Vector2 PositionAt(double progress)
        {
            progress = MathHelper.Clamp(progress, 0, 1);

            double index = progress * (calculatedPath.Count - 1);
            int flooredIndex = (int)index;

            Vector2 pos = calculatedPath[flooredIndex];
            if (index != flooredIndex)
                pos += (calculatedPath[flooredIndex + 1] - pos) * (float)(index - flooredIndex);

            return pos;
        }
    }
}
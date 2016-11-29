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

        public void Calculate()
        {
            switch (CurveType)
            {
                case CurveTypes.Linear:
                    calculatedPath = Path;
                    break;
                default:
                    var bezier = new BezierApproximator(Path);
                    calculatedPath = bezier.CreateBezier();
                    break;
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
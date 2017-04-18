// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Game.Rulesets.Objects.Types;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Objects
{
    public class CurvedHitObject : HitObject, IHasCurve
    {
        public SliderCurve Curve { get; } = new SliderCurve();

        public int RepeatCount { get; set; } = 1;

        public double EndTime => 0;
        public double Duration => 0;

        public List<Vector2> ControlPoints
        {
            get { return Curve.ControlPoints; }
            set { Curve.ControlPoints = value; }
        }

        public CurveType CurveType
        {
            get { return Curve.CurveType; }
            set { Curve.CurveType = value; }
        }

        public double Distance
        {
            get { return Curve.Distance; }
            set { Curve.Distance = value; }
        }

        public Vector2 PositionAt(double progress) => Curve.PositionAt(ProgressAt(progress));

        public double ProgressAt(double progress)
        {
            var p = progress * RepeatCount % 1;
            if (RepeatAt(progress) % 2 == 1)
                p = 1 - p;
            return p;
        }

        public int RepeatAt(double progress) => (int)(progress * RepeatCount);
    }
}

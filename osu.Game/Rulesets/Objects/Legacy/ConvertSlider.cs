// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Objects.Types;
using System;
using System.Collections.Generic;
using OpenTK;
using osu.Game.Audio;

namespace osu.Game.Rulesets.Objects.Legacy
{
    internal abstract class ConvertSlider : HitObject, IHasCurve
    {
        public List<Vector2> ControlPoints { get; set; }
        public CurveType CurveType { get; set; }
        public double Distance { get; set; }

        public List<List<SampleInfo>> RepeatSamples { get; set; }
        public int RepeatCount { get; set; } = 1;

        public double EndTime { get; set; }
        public double Duration { get; set; }

        public Vector2 PositionAt(double progress)
        {
            throw new NotImplementedException();
        }

        public double ProgressAt(double progress)
        {
            throw new NotImplementedException();
        }

        public int RepeatAt(double progress)
        {
            throw new NotImplementedException();
        }
    }
}

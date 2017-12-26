﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects.Types;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Objects.Legacy.Mania
{
    /// <summary>
    /// A HitObjectParser to parse legacy osu!mania Beatmaps.
    /// </summary>
    public class ConvertHitObjectParser : Legacy.ConvertHitObjectParser
    {
        protected override HitObject CreateHit(Vector2 position, bool newCombo)
        {
            return new ConvertHit
            {
                X = position.X,
                NewCombo = newCombo,
            };
        }

        protected override HitObject CreateSlider(Vector2 position, bool newCombo, List<Vector2> controlPoints, double length, CurveType curveType, int repeatCount, List<List<SampleInfo>> repeatSamples)
        {
            return new ConvertSlider
            {
                X = position.X,
                NewCombo = newCombo,
                ControlPoints = controlPoints,
                Distance = length,
                CurveType = curveType,
                RepeatSamples = repeatSamples,
                RepeatCount = repeatCount
            };
        }

        protected override HitObject CreateSpinner(Vector2 position, double endTime)
        {
            return new ConvertSpinner
            {
                X = position.X,
                EndTime = endTime
            };
        }

        protected override HitObject CreateHold(Vector2 position, bool newCombo, double endTime)
        {
            return new ConvertHold
            {
                X = position.X,
                EndTime = endTime
            };
        }
    }
}

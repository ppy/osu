// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Game.Rulesets.Objects.Types;
using System.Collections.Generic;
using osu.Game.Audio;

namespace osu.Game.Rulesets.Objects.Legacy.Taiko
{
    /// <summary>
    /// A HitObjectParser to parse legacy osu!taiko Beatmaps.
    /// </summary>
    public class ConvertHitObjectParser : Legacy.ConvertHitObjectParser
    {
        protected override HitObject CreateHit(Vector2 position, bool newCombo)
        {
            return new ConvertHit
            {
                NewCombo = newCombo,
            };
        }

        protected override HitObject CreateSlider(Vector2 position, bool newCombo, List<Vector2> controlPoints, double length, CurveType curveType, int repeatCount, List<List<SampleInfo>> repeatSamples)
        {
            return new ConvertSlider
            {
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
                EndTime = endTime
            };
        }

        protected override HitObject CreateHold(Vector2 position, bool newCombo, double endTime)
        {
            return null;
        }
    }
}

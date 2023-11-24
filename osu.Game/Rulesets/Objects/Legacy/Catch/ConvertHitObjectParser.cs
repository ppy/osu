// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osuTK;
using osu.Game.Audio;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Objects.Legacy.Catch
{
    /// <summary>
    /// A HitObjectParser to parse legacy osu!catch Beatmaps.
    /// </summary>
    public class ConvertHitObjectParser : Legacy.ConvertHitObjectParser
    {
        private ConvertHitObject lastObject;

        public ConvertHitObjectParser(double offset, int formatVersion)
            : base(offset, formatVersion)
        {
        }

        protected override HitObject CreateHit(Vector2 position, bool newCombo, int comboOffset)
        {
            return lastObject = new ConvertHit
            {
                Position = position,
                NewCombo = FirstObject || lastObject is ConvertSpinner || newCombo,
                ComboOffset = newCombo ? comboOffset : 0
            };
        }

        protected override HitObject CreateSlider(Vector2 position, bool newCombo, int comboOffset, PathControlPoint[] controlPoints, double? length, int repeatCount,
                                                  IList<IList<HitSampleInfo>> nodeSamples)
        {
            return lastObject = new ConvertSlider
            {
                Position = position,
                NewCombo = FirstObject || lastObject is ConvertSpinner || newCombo,
                ComboOffset = newCombo ? comboOffset : 0,
                Path = new SliderPath(controlPoints, length),
                NodeSamples = nodeSamples,
                RepeatCount = repeatCount
            };
        }

        protected override HitObject CreateSpinner(Vector2 position, bool newCombo, int comboOffset, double duration)
        {
            return lastObject = new ConvertSpinner
            {
                Duration = duration,
                NewCombo = newCombo
                // Spinners cannot have combo offset.
            };
        }

        protected override HitObject CreateHold(Vector2 position, bool newCombo, int comboOffset, double duration)
        {
            return lastObject = null;
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
        public ConvertHitObjectParser(double offset, int formatVersion)
            : base(offset, formatVersion)
        {
        }

        private bool forceNewCombo;

        protected override HitObject CreateHit(Vector2 position, bool newCombo, int comboOffset)
        {
            newCombo |= forceNewCombo;
            forceNewCombo = false;

            return new ConvertHit
            {
                X = position.X,
                NewCombo = newCombo,
            };
        }

        protected override HitObject CreateSlider(Vector2 position, bool newCombo, int comboOffset, PathControlPoint[] controlPoints, double? length, int repeatCount,
                                                  List<IList<HitSampleInfo>> nodeSamples)
        {
            newCombo |= forceNewCombo;
            forceNewCombo = false;

            return new ConvertSlider
            {
                X = position.X,
                NewCombo = FirstObject || newCombo,
                Path = new SliderPath(controlPoints, length),
                NodeSamples = nodeSamples,
                RepeatCount = repeatCount
            };
        }

        protected override HitObject CreateSpinner(Vector2 position, bool newCombo, int comboOffset, double duration)
        {
            return new ConvertSpinner
            {
                Duration = duration
            };
        }

        protected override HitObject CreateHold(Vector2 position, bool newCombo, int comboOffset, double duration)
        {
            return null;
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Game.Rulesets.Objects.Types;
using System.Collections.Generic;
using osu.Game.Audio;

namespace osu.Game.Rulesets.Objects.Legacy.Osu
{
    /// <summary>
    /// A HitObjectParser to parse legacy osu! Beatmaps.
    /// </summary>
    public class ConvertHitObjectParser : Legacy.ConvertHitObjectParser
    {
        public ConvertHitObjectParser(double offset, int formatVersion)
            : base(offset, formatVersion)
        {
        }

        private bool forceNewCombo;
        private int extraComboOffset;

        protected override HitObject CreateHit(Vector2 position, bool newCombo, int comboOffset)
        {
            newCombo |= forceNewCombo;
            comboOffset += extraComboOffset;

            forceNewCombo = false;
            extraComboOffset = 0;

            return new ConvertHit
            {
                Position = position,
                NewCombo = FirstObject || newCombo,
                ComboOffset = comboOffset
            };
        }

        protected override HitObject CreateSlider(Vector2 position, bool newCombo, int comboOffset, Vector2[] controlPoints, double? length, PathType pathType, int repeatCount,
                                                  List<List<HitSampleInfo>> nodeSamples)
        {
            newCombo |= forceNewCombo;
            comboOffset += extraComboOffset;

            forceNewCombo = false;
            extraComboOffset = 0;

            return new ConvertSlider
            {
                Position = position,
                NewCombo = FirstObject || newCombo,
                ComboOffset = comboOffset,
                Path = new SliderPath(pathType, controlPoints, length),
                NodeSamples = nodeSamples,
                RepeatCount = repeatCount
            };
        }

        protected override HitObject CreateSpinner(Vector2 position, bool newCombo, int comboOffset, double endTime)
        {
            // Convert spinners don't create the new combo themselves, but force the next non-spinner hitobject to create a new combo
            // Their combo offset is still added to that next hitobject's combo index
            forceNewCombo |= FormatVersion <= 8 || newCombo;
            extraComboOffset += comboOffset;

            return new ConvertSpinner
            {
                Position = position,
                EndTime = endTime
            };
        }

        protected override HitObject CreateHold(Vector2 position, bool newCombo, int comboOffset, double endTime)
        {
            return null;
        }
    }
}

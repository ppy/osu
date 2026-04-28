// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Extensions;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModDifficultyAdjust : ModDifficultyAdjust
    {
        public override DifficultyBindable OverallDifficulty { get; } = new DifficultyBindable
        {
            Precision = 0.1f,
            MinValue = 0,
            MaxValue = 10,
            // Use larger extended limits for mania to include OD values that occur with EZ or HR enabled
            ExtendedMaxValue = 15,
            ExtendedMinValue = -15,
            ReadCurrentFromDifficulty = diff => diff.OverallDifficulty,
        };

        public override string ExtendedIconInformation
        {
            get
            {
                if (!IsExactlyOneSettingChanged(OverallDifficulty, DrainRate))
                {
                    if (OverallDifficulty.ExtendedMaxValue == OverallDifficulty.Value
                        && DrainRate.ExtendedMaxValue == DrainRate.Value) return "MAX+";
                    if (OverallDifficulty.MaxValue == OverallDifficulty.Value
                        && DrainRate.MaxValue == DrainRate.Value) return "MAX";
                    if (OverallDifficulty.ExtendedMinValue == OverallDifficulty.Value
                        && DrainRate.MinValue == DrainRate.Value) return "MIN+";
                    if (OverallDifficulty.MinValue == OverallDifficulty.Value
                        && DrainRate.MinValue == DrainRate.Value) return "MIN";

                    return string.Empty;
                }

                if (!OverallDifficulty.IsDefault) return format("OD", OverallDifficulty);
                if (!DrainRate.IsDefault) return format("HP", DrainRate);

                return string.Empty;

                string format(string acronym, DifficultyBindable bindable) => $"{acronym}{bindable.Value!.Value.ToStandardFormattedString(1)}";
            }
        }
    }
}

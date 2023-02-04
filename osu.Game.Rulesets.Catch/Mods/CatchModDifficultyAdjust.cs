// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Localisation.Mods;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModDifficultyAdjust : ModDifficultyAdjust, IApplicableToBeatmapProcessor
    {
        [SettingSource(typeof(DifficultyAdjustModStrings), nameof(DifficultyAdjustModStrings.CircleSize), nameof(DifficultyAdjustModStrings.CircleSizeDescription), FIRST_SETTING_ORDER - 1, SettingControlType = typeof(DifficultyAdjustSettingsControl))]
        public DifficultyBindable CircleSize { get; } = new DifficultyBindable
        {
            Precision = 0.1f,
            MinValue = 1,
            MaxValue = 10,
            ExtendedMaxValue = 11,
            ReadCurrentFromDifficulty = diff => diff.CircleSize,
        };

        [SettingSource(typeof(DifficultyAdjustModStrings), nameof(DifficultyAdjustModStrings.ApproachRate), nameof(DifficultyAdjustModStrings.ApproachRateDescription), LAST_SETTING_ORDER + 1, SettingControlType = typeof(DifficultyAdjustSettingsControl))]
        public DifficultyBindable ApproachRate { get; } = new DifficultyBindable
        {
            Precision = 0.1f,
            MinValue = 1,
            MaxValue = 10,
            ExtendedMaxValue = 11,
            ReadCurrentFromDifficulty = diff => diff.ApproachRate,
        };

        [SettingSource(typeof(DifficultyAdjustModStrings), nameof(DifficultyAdjustModStrings.HardRockOffsets), nameof(DifficultyAdjustModStrings.HardRockOffsetsDescription))]
        public BindableBool HardRockOffsets { get; } = new BindableBool();

        public override string SettingDescription
        {
            get
            {
                string circleSize = CircleSize.IsDefault ? string.Empty : $"CS {CircleSize.Value:N1}";
                string approachRate = ApproachRate.IsDefault ? string.Empty : $"AR {ApproachRate.Value:N1}";
                string spicyPatterns = HardRockOffsets.IsDefault ? string.Empty : "Spicy patterns";

                return string.Join(", ", new[]
                {
                    circleSize,
                    base.SettingDescription,
                    approachRate,
                    spicyPatterns,
                }.Where(s => !string.IsNullOrEmpty(s)));
            }
        }

        protected override void ApplySettings(BeatmapDifficulty difficulty)
        {
            base.ApplySettings(difficulty);

            if (CircleSize.Value != null) difficulty.CircleSize = CircleSize.Value.Value;
            if (ApproachRate.Value != null) difficulty.ApproachRate = ApproachRate.Value.Value;
        }

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            var catchProcessor = (CatchBeatmapProcessor)beatmapProcessor;
            catchProcessor.HardRockOffsets = HardRockOffsets.Value;
        }
    }
}

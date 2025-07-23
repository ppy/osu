// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Utils;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModHardRock : ModHardRock, IApplicableToHitObject
    {
        [SettingSource("Flipped axes", "Flip objects on the chosen axes.")]
        public Bindable<MirrorType> Reflection { get; } = new Bindable<MirrorType>(MirrorType.Vertical);

        [SettingSource("Accuracy Multiplier", "The multiplier applied to the beatmap's accuracy (overall difficulty).", SettingControlType = typeof(MultiplierSettingsSlider))]
        public Bindable<double> OverallDifficultyRatio { get; } = new BindableDouble(ADJUST_RATIO)
        {
            MinValue = 1,
            MaxValue = 2,
            Precision = 0.01f,
        };

        [SettingSource("Circle Size Multiplier", "The multiplier applied to the beatmap's circle size (CS).", SettingControlType = typeof(MultiplierSettingsSlider))]
        public Bindable<double> CircleSizeRatio { get; } = new BindableDouble(1.3f)
        {
            MinValue = 1,
            MaxValue = 2,
            Precision = 0.01f,
        };

        [SettingSource("Approach Rate Multiplier", "The multiplier applied to the beatmap's approach rate (AR).", SettingControlType = typeof(MultiplierSettingsSlider))]
        public Bindable<double> ApproachRateRatio { get; } = new BindableDouble(ADJUST_RATIO)
        {
            MinValue = 1,
            MaxValue = 2,
            Precision = 0.01f,
        };

        public override double ScoreMultiplier => UsesDefaultConfiguration ? 1.06 : 1;

        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(ModMirror)).ToArray();

        public void ApplyToHitObject(HitObject hitObject)
        {
            var osuObject = (OsuHitObject)hitObject;

            switch (Reflection.Value)
            {
                case MirrorType.Horizontal:
                    OsuHitObjectGenerationUtils.ReflectHorizontallyAlongPlayfield(osuObject);
                    break;

                case MirrorType.Vertical:
                    OsuHitObjectGenerationUtils.ReflectVerticallyAlongPlayfield(osuObject);
                    break;

                case MirrorType.Both:
                    OsuHitObjectGenerationUtils.ReflectHorizontallyAlongPlayfield(osuObject);
                    OsuHitObjectGenerationUtils.ReflectVerticallyAlongPlayfield(osuObject);
                    break;
            }
        }

        public override void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            base.ApplyToDifficulty(difficulty);

            difficulty.OverallDifficulty = Math.Min(difficulty.OverallDifficulty * (float)OverallDifficultyRatio.Value, AdjustLimit);
            difficulty.CircleSize = Math.Min(difficulty.CircleSize * (float)CircleSizeRatio.Value, AdjustLimit);
            difficulty.ApproachRate = Math.Min(difficulty.ApproachRate * (float)ApproachRateRatio.Value, AdjustLimit);
        }

        public enum MirrorType
        {
            None,
            Horizontal,
            Vertical,
            Both
        }
    }
}

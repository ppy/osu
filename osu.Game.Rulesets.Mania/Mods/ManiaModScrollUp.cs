// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using static osu.Game.Rulesets.Mania.ManiaSettingsSubsection;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModScrollUp : Mod, IApplicableToDrawableRuleset<ManiaHitObject>, IApplicableToScoreProcessor, IApplicableToPlayer
    {
        public override string Name => "Scroll up";
        public override string Acronym => "SU";
        public override LocalisableString Description => @"Key will become faster..., until you miss";
        public override double ScoreMultiplier => 1;
        public override IconUsage? Icon => null;
        public override ModType Type => ModType.Conversion;

        private DrawableManiaRuleset drawableRuleset = null!;

        private readonly BindableDouble scrollTime = new BindableDouble();

        [SettingSource("Max Speed combo", "The combo count at which point the spead reaches max.")]
        public BindableInt MaxComboCount { get; } = new BindableInt(480)
        {
            MinValue = 1,
            MaxValue = 2000,
        };

        [SettingSource("Min Speed", "The speed when combo is 0", SettingControlType = typeof(SettingsSlider<double, ManiaScrollSlider>))]
        public BindableDouble MinScoreSpeed { get; } = new BindableDouble(DrawableManiaRuleset.MAX_TIME_RANGE)
        {
            MinValue = DrawableManiaRuleset.MIN_TIME_RANGE,
            MaxValue = DrawableManiaRuleset.MAX_TIME_RANGE,
            Precision = 5,
        };

        [SettingSource("Max Speed", "The Max speed will reach", SettingControlType = typeof(SettingsSlider<double, ManiaScrollSlider>))]
        public BindableDouble MaxScoreSpeed { get; } = new BindableDouble(DrawableManiaRuleset.MIN_TIME_RANGE)
        {
            MinValue = DrawableManiaRuleset.MIN_TIME_RANGE,
            MaxValue = DrawableManiaRuleset.MAX_TIME_RANGE,
            Precision = 5,
        };

        public ManiaModScrollUp()
        {
            MinScoreSpeed.BindValueChanged(val =>
            {
                if (val.NewValue <= MaxScoreSpeed.Value)
                    MaxScoreSpeed.Value = val.NewValue + MaxScoreSpeed.Precision;
            });

            MaxScoreSpeed.BindValueChanged(val =>
            {
                if (val.NewValue >= MinScoreSpeed.Value)
                    MinScoreSpeed.Value = val.NewValue + MinScoreSpeed.Precision;
            });
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<ManiaHitObject> drawableRuleset)
        {
            this.drawableRuleset = (DrawableManiaRuleset)drawableRuleset;
        }

        public void ApplyToPlayer(Player player)
        {
            drawableRuleset.ScoreSpeed.Disabled = false;
            scrollTime.BindTo(drawableRuleset.ScoreSpeed);
            scrollTime.Value = MinScoreSpeed.Value;
        }

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            scoreProcessor.Combo.BindValueChanged(s =>
            {
                if (s.NewValue > MaxComboCount.Value)
                {
                    return;
                }

                double speed = MinScoreSpeed.Value - (MinScoreSpeed.Value - MaxScoreSpeed.Value) * Math.Log(s.NewValue + 1, MaxComboCount.Value + 1);

                scoreProcessor.TransformBindableTo(scrollTime, speed, 500, Easing.OutQuint);
            });
        }

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy) => rank;
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Utils;
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
    public class ManiaModAccelerate : Mod, IApplicableToDrawableRuleset<ManiaHitObject>, IApplicableToScoreProcessor, IApplicableToPlayer, IUpdatableByPlayfield
    {
        public override string Name => "Accelerate";
        public override string Acronym => "AC";
        public override LocalisableString Description => @"Key will become faster..., until you miss";

        public override Type[] IncompatibleMods => new[] { typeof(ManiaModConstantSpeed) };
        public override double ScoreMultiplier => 1;
        public override IconUsage? Icon => null;
        public override ModType Type => ModType.Fun;

        private DrawableManiaRuleset drawableRuleset = null!;

        private readonly BindableDouble scrollTime = new BindableDouble();

        private readonly BindableDouble targetScrollTime = new BindableDouble();

        [SettingSource("Base Combo", "The combo count to start changing speed.")]
        public BindableInt BaseComboCount { get; } = new BindableInt
        {
            MinValue = 0,
            MaxValue = 1999,
        };

        [SettingSource("Max Speed combo", "The combo count at which let the speed reaches max.")]
        public BindableInt MaxComboCount { get; } = new BindableInt(480)
        {
            MinValue = 1,
            MaxValue = 2000,
        };

        [SettingSource("Min Speed", "The speed when combo is \"Base Combo\"", SettingControlType = typeof(SettingsSlider<int, ManiaScrollSlider>))]
        public BindableInt MinScoreSpeed { get; } = new BindableInt(18)
        {
            MinValue = 1,
            MaxValue = 39,
            Value = 18,
        };

        [SettingSource("Max Speed", "The Max speed will reach", SettingControlType = typeof(SettingsSlider<int, ManiaScrollSlider>))]
        public BindableInt MaxScoreSpeed { get; } = new BindableInt(30)
        {
            MinValue = 2,
            MaxValue = 40,
            Value = 30,
        };

        private int comboRange => MaxComboCount.Value - BaseComboCount.Value;
        private int speedRange => MaxScoreSpeed.Value - MinScoreSpeed.Value;

        public ManiaModAccelerate()
        {
            MinScoreSpeed.BindValueChanged(val =>
            {
                if (val.NewValue >= MaxScoreSpeed.Value)
                    MaxScoreSpeed.Value = val.NewValue + MaxScoreSpeed.Precision;
            });

            MaxScoreSpeed.BindValueChanged(val =>
            {
                if (val.NewValue <= MinScoreSpeed.Value)
                    MinScoreSpeed.Value = val.NewValue - MinScoreSpeed.Precision;
            });

            BaseComboCount.BindValueChanged(val =>
            {
                if (val.NewValue >= MaxComboCount.Value)
                {
                    MaxComboCount.Value = val.NewValue + MaxComboCount.Precision;
                }
            });

            MaxComboCount.BindValueChanged(val =>
            {
                if (val.NewValue <= BaseComboCount.Value)
                {
                    BaseComboCount.Value = val.NewValue - BaseComboCount.Precision;
                }
            });
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<ManiaHitObject> drawableRuleset)
        {
            this.drawableRuleset = (DrawableManiaRuleset)drawableRuleset;
        }

        public void ApplyToPlayer(Player player)
        {
            drawableRuleset.CustomSmoothTimeRange.Disabled = false;
            scrollTime.BindTo(drawableRuleset.CustomSmoothTimeRange);
            scrollTime.Value = DrawableManiaRuleset.ComputeScrollTime(MinScoreSpeed.Value);
            targetScrollTime.Value = DrawableManiaRuleset.ComputeScrollTime(MinScoreSpeed.Value);
        }

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            scoreProcessor.Combo.BindValueChanged(s =>
            {
                targetScrollTime.Value = DrawableManiaRuleset.ComputeScrollTime(targetScrollSpeed(s.NewValue));
            });
        }

        private int targetScrollSpeed(int combo)
        {
            if (combo <= BaseComboCount.Value)
                return MinScoreSpeed.Value;

            if (combo >= MaxComboCount.Value)
                return MaxScoreSpeed.Value;

            return (int)(MinScoreSpeed.Value + (double)speedRange * (combo - BaseComboCount.Value) / comboRange);
        }

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy) => rank;

        public void Update(Playfield playfield)
        {
            scrollTime.Value = Interpolation.DampContinuously(scrollTime.Value, targetScrollTime.Value, 250, Math.Abs(playfield.Clock.ElapsedFrameTime));
        }
    }
}

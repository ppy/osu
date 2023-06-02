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
        public override double ScoreMultiplier => 1;
        public override IconUsage? Icon => null;
        public override ModType Type => ModType.Fun;

        private DrawableManiaRuleset drawableRuleset = null!;

        private readonly BindableDouble scrollTime = new BindableDouble();

        private readonly BindableDouble targetScrollTime = new BindableDouble();

        [SettingSource("Max Speed combo", "The combo count at which point the spead reaches max.")]
        public BindableInt MaxComboCount { get; } = new BindableInt(480)
        {
            MinValue = 1,
            MaxValue = 2000,
        };

        [SettingSource("Min Speed", "The speed when combo is 0", SettingControlType = typeof(SettingsSlider<int, ManiaScrollSlider>))]
        public BindableInt MinScoreSpeed { get; } = new BindableInt(8)
        {
            MinValue = 1,
            MaxValue = 39,
            Value = 8,
        };

        [SettingSource("Max Speed", "The Max speed will reach", SettingControlType = typeof(SettingsSlider<int, ManiaScrollSlider>))]
        public BindableInt MaxScoreSpeed { get; } = new BindableInt(40)
        {
            MinValue = 2,
            MaxValue = 40,
            Value = 40,
        };

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
            targetScrollTime.Value = MinScoreSpeed.Value;
        }

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            scoreProcessor.Combo.BindValueChanged(s =>
            {
                if (s.NewValue >= MaxComboCount.Value)
                {
                    targetScrollTime.Value = DrawableManiaRuleset.ComputeScrollTime(MaxScoreSpeed.Value);
                }
                else
                {
                    targetScrollTime.Value = DrawableManiaRuleset.ComputeScrollTime(
                        MinScoreSpeed.Value + (MaxScoreSpeed.Value - MinScoreSpeed.Value) * (s.NewValue / MaxComboCount.Value));
                }
            });
        }

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy) => rank;

        public void Update(Playfield playfield)
        {
            scrollTime.Value = Interpolation.DampContinuously(scrollTime.Value, targetScrollTime.Value, 250, Math.Abs(playfield.Clock.ElapsedFrameTime));
        }
    }
}

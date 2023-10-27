// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using static osu.Game.Rulesets.Mania.ManiaSettingsSubsection;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModAccelerate : Mod, IApplicableToScoreProcessor, IManiaAdjustScrollSpeed
    {
        public override string Name => "Accelerate";
        public override string Acronym => "AL";
        public override LocalisableString Description => @"Key will become faster..., until you miss";
        public override double ScoreMultiplier => 1;
        public override IconUsage? Icon => null;
        public override ModType Type => ModType.Fun;

        public BindableInt ScrollSpeed { get; set; } = new BindableInt();

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
        public BindableInt MinScrollSpeed { get; } = new BindableInt(18)
        {
            MinValue = 1,
            MaxValue = 39,
            Value = 18,
        };

        [SettingSource("Max Speed", "The Max speed will reach", SettingControlType = typeof(SettingsSlider<int, ManiaScrollSlider>))]
        public BindableInt MaxScrollSpeed { get; } = new BindableInt(30)
        {
            MinValue = 2,
            MaxValue = 40,
            Value = 30,
        };

        private int comboRange => MaxComboCount.Value - BaseComboCount.Value;
        private int speedRange => MaxScrollSpeed.Value - MinScrollSpeed.Value;

        public ManiaModAccelerate()
        {
            MinScrollSpeed.BindValueChanged(val =>
            {
                if (val.NewValue >= MaxScrollSpeed.Value)
                    MaxScrollSpeed.Value = val.NewValue + MaxScrollSpeed.Precision;
            });

            MaxScrollSpeed.BindValueChanged(val =>
            {
                if (val.NewValue <= MinScrollSpeed.Value)
                    MinScrollSpeed.Value = val.NewValue - MinScrollSpeed.Precision;
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

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            scoreProcessor.Combo.BindValueChanged(s =>
            {
                ScrollSpeed.Value = targetScrollSpeed(s.NewValue);
            }, true);
        }

        private int targetScrollSpeed(int combo)
        {
            if (combo <= BaseComboCount.Value)
                return MinScrollSpeed.Value;

            if (combo >= MaxComboCount.Value)
                return MaxScrollSpeed.Value;

            return (int)(MinScrollSpeed.Value + (double)speedRange * (combo - BaseComboCount.Value) / comboRange);
        }

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy) => rank;
    }
}

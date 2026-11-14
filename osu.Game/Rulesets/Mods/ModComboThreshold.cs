// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModComboThreshold : ModFailCondition
    {
        public override string Name => "Combo Threshold";
        public override string Acronym => "CT";
        public override IconUsage? Icon => OsuIcon.ModPerfect;
        public override ModType Type => ModType.DifficultyIncrease;
        public override double ScoreMultiplier => 1.0;
        public override LocalisableString Description => "Maintain your combo above the threshold or restart.";
        public override bool Ranked => true;

        public Bindable<ComboThresholdMode> Mode { get; } = new Bindable<ComboThresholdMode>(ComboThresholdMode.CurrentCombo);

        [SettingSource("Minimum Combo", "The minimum combo threshold to maintain")]
        public BindableInt MinimumCombo { get; } = new BindableInt(50)
        {
            MinValue = 1,
            MaxValue = 50000,
            Default = 50,
            Precision = 1
        };

        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[]
        {
            typeof(ModSuddenDeath),
            typeof(ModPerfect),
        }
        ).ToArray();

        private int highestCombo;

        protected ModComboThreshold()
        {
            Restart.Value = Restart.Default = true;
        }

        protected override bool FailCondition(HealthProcessor healthProcessor, JudgementResult result)
        {
            if (!result.Type.AffectsCombo())
                return false;

            int comboAfterJudgement = result.Type.IncreasesCombo()
            ? result.ComboAtJudgement + 1
            : 0;

            if (comboAfterJudgement > highestCombo)
            {
                highestCombo = comboAfterJudgement;
            }

            switch (Mode.Value)
            {
                case ComboThresholdMode.MaxCombo:
                    if (!result.Type.IncreasesCombo() && highestCombo > 0 && highestCombo < MinimumCombo.Value)
                    {
                        return true;
                    }

                    break;

                case ComboThresholdMode.CurrentCombo:
                    if (!result.Type.IncreasesCombo() && comboAfterJudgement < MinimumCombo.Value && highestCombo >= MinimumCombo.Value)
                    {
                        return true;
                    }

                    break;
            }

            return false;
        }

        public enum ComboThresholdMode
        {
            MaxCombo,
            CurrentCombo
        }
    }
}

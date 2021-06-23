// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Humanizer;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModEasyWithExtraLives : ModEasy, IApplicableFailOverride, IApplicableToHealthProcessor
    {
        [SettingSource("Extra Lives", "Number of extra lives")]
        public Bindable<int> Retries { get; } = new BindableInt(2)
        {
            MinValue = 0,
            MaxValue = 10
        };

        public override string SettingDescription => Retries.IsDefault ? string.Empty : $"{"lives".ToQuantity(Retries.Value)}";

        private int retries;

        private BindableNumber<double> health;

        public override void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            base.ApplyToDifficulty(difficulty);
            retries = Retries.Value;
        }

        public bool PerformFail()
        {
            if (retries == 0) return true;

            health.Value = health.MaxValue;
            retries--;

            return false;
        }

        public bool RestartOnFail => false;

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            health = healthProcessor.Health.GetBoundCopy();
        }
    }
}

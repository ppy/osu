// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Judgements;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModHealthChallenge : ModChallenge
    {
        public override string Name => "Health Challenge";
        public override string Acronym => "HC";
        public override IconUsage? Icon => FontAwesome.Solid.Heartbeat;
        public override string Description => "Fail the beatmap if your health goes below a specified value.";

        [SettingSource("Minimum health", "Fail map if your health goes under this value.")]
        public BindableNumber<double> MinimumHealth { get; } = new BindableDouble
        {
            MinValue = 0,
            MaxValue = 100,
            Default = 75,
            Value = 75,
            Precision = 0.1,
        };

        private bool playIsPerfect = true;

        protected override bool FailCondition(HealthProcessor healthProcessor, JudgementResult result)
        {
            if (JudgementIsFlawed(result))
                playIsPerfect = false;
            if (!AllowChallengeFailureAtHitObject(result.HitObject))
                return false;

            if (CheckingInterval.Value == ChallengeCheckInterval.Continuously)
                return healthProcessor.Health.Value < MinimumHealth.Value / 100 && JudgementIsFlawed(result);

            return healthProcessor.Health.Value < MinimumHealth.Value && !playIsPerfect;
        }
    }
}
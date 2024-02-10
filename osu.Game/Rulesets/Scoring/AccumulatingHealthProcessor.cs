// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Scoring
{
    /// <summary>
    /// A <see cref="HealthProcessor"/> that accumulates health and causes a fail if the final health
    /// is less than a value required to pass the beatmap.
    /// </summary>
    public partial class AccumulatingHealthProcessor : HealthProcessor
    {
        protected override bool DefaultFailCondition => JudgedHits == MaxHits && Health.Value < requiredHealth;

        private readonly double requiredHealth;

        /// <summary>
        /// Creates a new <see cref="AccumulatingHealthProcessor"/>.
        /// </summary>
        /// <param name="requiredHealth">The minimum amount of health required to beatmap.</param>
        public AccumulatingHealthProcessor(double requiredHealth)
        {
            this.requiredHealth = requiredHealth;
        }

        protected override void Reset(bool storeResults)
        {
            base.Reset(storeResults);

            Health.Value = 0;
        }
    }
}

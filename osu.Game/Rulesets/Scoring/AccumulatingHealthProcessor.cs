// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Scoring
{
    public class AccumulatingHealthProcessor : HealthProcessor
    {
        public AccumulatingHealthProcessor(double gameplayStartTime)
            : base(gameplayStartTime)
        {
        }

        protected override void Reset(bool storeResults)
        {
            base.Reset(storeResults);

            Health.Value = 0;
        }
    }
}

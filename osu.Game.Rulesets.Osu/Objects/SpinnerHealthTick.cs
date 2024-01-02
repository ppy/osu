// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class SpinnerHealthTick : SpinnerTick
    {
        public override Judgement CreateJudgement() => new OsuSpinnerHealthTickJudgement();

        public class OsuSpinnerHealthTickJudgement : OsuSpinnerTickJudgement
        {
            public override HitResult MaxResult => HitResult.HealthBonus;
        }
    }
}

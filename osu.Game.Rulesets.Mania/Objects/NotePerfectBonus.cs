// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Objects
{
    public class NotePerfectBonus : ManiaHitObject
    {
        public override Judgement CreateJudgement() => new NotePerfectBonusJudgement();
        protected override HitWindows CreateHitWindows() => HitWindows.Empty;

        protected override HitObject CreateInstance() => new NotePerfectBonus();

        public class NotePerfectBonusJudgement : ManiaJudgement
        {
            public override HitResult MaxResult => HitResult.SmallBonus;
        }
    }
}

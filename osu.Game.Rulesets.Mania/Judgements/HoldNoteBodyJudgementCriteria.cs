// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Judgements
{
    public class HoldNoteBodyJudgementCriteria : ManiaJudgementCriteria
    {
        public override HitResult MaxResult => HitResult.IgnoreHit;
        public override HitResult MinResult => HitResult.ComboBreak;
    }
}

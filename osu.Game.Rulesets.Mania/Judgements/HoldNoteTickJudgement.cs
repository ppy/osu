// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Judgements
{
    public class HoldNoteTickJudgement : ManiaJudgement
    {
        public override bool AffectsCombo => false;

        protected override int NumericResultFor(HitResult result) => 20;
    }
}

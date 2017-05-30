// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.Mania.Judgements
{
    public class HoldNoteTickJudgement : ManiaJudgement
    {
        public override bool AffectsCombo => false;

        public override int NumericResultForScore(ManiaHitResult result) => 20;
        public override int NumericResultForAccuracy(ManiaHitResult result) => 0; // Don't count ticks into accuracy
    }
}
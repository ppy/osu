// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Catch.Judgements;
using osu.Game.Rulesets.Judgements;

namespace osu.Game.Rulesets.Catch.Objects
{
    public class Fruit : PalpableCatchHitObject
    {
        public override JudgementInfo CreateJudgement() => new CatchJudgementInfo();

        public static FruitVisualRepresentation GetVisualRepresentation(int indexInBeatmap) => (FruitVisualRepresentation)(indexInBeatmap % 4);
    }
}

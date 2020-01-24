// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Judgements;

namespace osu.Game.Rulesets.Taiko.Objects
{
    public class SwellTick : TaikoHitObject
    {
        public override Judgement CreateJudgement() => new TaikoSwellTickJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
    }
}

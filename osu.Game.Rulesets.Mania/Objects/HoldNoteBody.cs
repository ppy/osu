// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Objects
{
    /// <summary>
    /// The body of a <see cref="HoldNote"/>.
    /// Mostly a dummy hitobject that provides the judgement for the "holding" state.<br />
    /// On hit - the hold note was held correctly for the full duration.<br />
    /// On miss - the hold note was released at some point during its judgement period.
    /// </summary>
    public class HoldNoteBody : ManiaHitObject
    {
        public override JudgementCriteria CreateJudgement() => new HoldNoteBodyJudgementCriteria();
        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
    }
}

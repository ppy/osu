// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Objects
{
    /// <summary>
    /// A scoring tick of a hold note.
    /// </summary>
    public class HoldNoteTick : ManiaHitObject
    {
        public override Judgement CreateJudgement() => new HoldNoteTickJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
    }
}

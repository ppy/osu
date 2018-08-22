// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Judgements;

namespace osu.Game.Rulesets.Mania.Objects
{
    /// <summary>
    /// A scoring tick of a hold note.
    /// </summary>
    public class HoldNoteTick : ManiaHitObject
    {
        public override Judgement CreateJudgement() => new HoldNoteTickJudgement();
    }
}

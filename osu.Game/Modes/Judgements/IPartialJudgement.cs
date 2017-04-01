// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Scoring;

namespace osu.Game.Modes.Judgements
{
    /// <summary>
    /// Inidicates that the judgement this is attached to is a partial judgement and the scoring value may change.
    /// <para>
    /// This judgement will be continually processed by <see cref="DrawableHitObject{TObject, TJudgement}.CheckJudgement(bool)"/>
    /// unless the result is a miss and will trigger a full re-process of the <see cref="ScoreProcessor"/> when changed.
    /// </para>
    /// </summary>
    public interface IPartialJudgement
    {
        /// <summary>
        /// Indicates that this partial judgement has changed and requires a full re-process of the <see cref="ScoreProcessor"/>.
        /// <para>
        /// This is set to false once the judgement has been re-processed.
        /// </para>
        /// </summary>
        bool Changed { get; set; }
    }
}

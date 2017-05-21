// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.Judgements
{
    /// <summary>
    /// Inidicates that the judgement this is attached to is a partial judgement and the scoring value may change.
    /// </summary>
    public interface IPartialJudgement
    {
        /// <summary>
        /// Indicates that this partial judgement has changed and requires reprocessing.
        /// <para>
        /// This is set to false once the judgement has been re-processed.
        /// </para>
        /// </summary>
        bool Changed { get; set; }
    }
}

// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Judgements;
using System.ComponentModel;

namespace osu.Game.Rulesets.Scoring
{
    public enum HitResult
    {
        /// <summary>
        /// Indicates that the object has not been judged yet.
        /// </summary>
        [Description(@"")]
        None,

        /// <summary>
        /// Indicates that the object has been judged as a miss.
        /// </summary>
        [Description(@"Miss")]
        [JudgementColour(@"ed1121")]
        Miss,

        [Description(@"Meh")]
        [JudgementColour(@"ffcc22")]
        Meh,

        /// <summary>
        /// Optional judgement.
        /// </summary>
        [Description(@"OK")]
        [JudgementColour(@"88b300")]
        Ok,

        [Description(@"Good")]
        [JudgementColour(@"88b300")]
        Good,

        [Description(@"Great")]
        [JudgementColour(@"66ccff")]
        Great,

        /// <summary>
        /// Optional judgement.
        /// </summary>
        [Description(@"Perfect")]
        [JudgementColour(@"66ccff")]
        Perfect,
    }
}

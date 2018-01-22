// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
        Miss,

        [Description(@"Meh")]
        Meh,

        /// <summary>
        /// Optional judgement.
        /// </summary>
        [Description(@"OK")]
        Ok,

        [Description(@"Good")]
        Good,

        [Description(@"Great")]
        Great,

        /// <summary>
        /// Optional judgement.
        /// </summary>
        [Description(@"Perfect")]
        Perfect,
    }
}

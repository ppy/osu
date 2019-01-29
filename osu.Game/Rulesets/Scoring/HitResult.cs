// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

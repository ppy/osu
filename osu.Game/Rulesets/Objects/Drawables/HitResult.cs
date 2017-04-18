// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;

namespace osu.Game.Rulesets.Objects.Drawables
{
    public enum HitResult
    {
        /// <summary>
        /// Indicates that the object has not been judged yet.
        /// </summary>
        [Description("")]
        None,
        /// <summary>
        /// Indicates that the object has been judged as a miss.
        /// </summary>
        [Description(@"Miss")]
        Miss,
        /// <summary>
        /// Indicates that the object has been judged as a hit.
        /// </summary>
        [Description(@"Hit")]
        Hit,
    }
}
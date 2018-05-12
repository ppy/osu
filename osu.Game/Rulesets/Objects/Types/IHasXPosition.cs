// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.Objects.Types
{
    /// <summary>
    /// A HitObject that has a starting X-position.
    /// </summary>
    public interface IHasXPosition
    {
        /// <summary>
        /// The starting X-position of this HitObject.
        /// </summary>
        float X { get; }
    }
}

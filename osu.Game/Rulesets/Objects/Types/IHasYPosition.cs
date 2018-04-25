// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.Objects.Types
{
    /// <summary>
    /// A HitObject that has a starting Y-position.
    /// </summary>
    public interface IHasYPosition
    {
        /// <summary>
        /// The starting Y-position of this HitObject.
        /// </summary>
        float Y { get; }
    }
}

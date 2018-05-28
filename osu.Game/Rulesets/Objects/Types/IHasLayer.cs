// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.Objects.Types
{
    /// <summary>
    /// A type of hit object which belongs in an editor layer.
    /// </summary>
    public interface IHasLayer
    {
        /// <summary>
        /// The editor layer which the hit object belongs in.
        /// </summary>
        int Layer { get; }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Objects.Types
{
    /// <summary>
    /// A HitObject which has a preferred display colour. Will be used for editor timeline display.
    /// </summary>
    public interface IHasDisplayColour
    {
        /// <summary>
        /// The current display colour of this hit object.
        /// </summary>
        Bindable<Color4> DisplayColour { get; }
    }
}

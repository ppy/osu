// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Rulesets.UI
{
    /// <summary>
    /// A container which handles sizing of the <see cref="Playfield"/> and any other components that need to match their size.
    /// </summary>
    public class PlayfieldAdjustmentContainer : Container
    {
        public PlayfieldAdjustmentContainer()
        {
            RelativeSizeAxes = Axes.Both;
        }
    }
}

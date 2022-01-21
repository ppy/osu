// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays
{
    /// <summary>
    /// An <see cref="ExpandingControlContainer{TControl}"/> with a long hover expansion delay for buttons.
    /// </summary>
    /// <remarks>
    /// Mostly used for buttons with explanatory labels, in which the label would display after a "long hover".
    /// </remarks>
    public class ExpandingButtonContainer : ExpandingControlContainer<OsuButton>
    {
        protected ExpandingButtonContainer(float contractedWidth, float expandedWidth)
            : base(contractedWidth, expandedWidth)
        {
        }

        protected override double HoverExpansionDelay => 750;
    }
}

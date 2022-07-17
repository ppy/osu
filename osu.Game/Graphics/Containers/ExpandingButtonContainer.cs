// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

namespace osu.Game.Graphics.Containers
{
    /// <summary>
    /// An <see cref="ExpandingContainer"/> with a long hover expansion delay.
    /// </summary>
    /// <remarks>
    /// Mostly used for buttons with explanatory labels, in which the label would display after a "long hover".
    /// </remarks>
    public class ExpandingButtonContainer : ExpandingContainer
    {
        protected ExpandingButtonContainer(float contractedWidth, float expandedWidth)
            : base(contractedWidth, expandedWidth)
        {
        }

        protected override double HoverExpansionDelay => 400;
    }
}

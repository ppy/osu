// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Ranking.Statistics
{
    /// <summary>
    /// An item to be displayed in a row of statistics inside the results screen.
    /// </summary>
    public class StatisticItem
    {
        /// <summary>
        /// The name of this item.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The <see cref="Drawable"/> content to be displayed.
        /// </summary>
        public readonly Drawable Content;

        /// <summary>
        /// The <see cref="Dimension"/> of this row. This can be thought of as the column dimension of an encompassing <see cref="GridContainer"/>.
        /// </summary>
        public readonly Dimension Dimension;

        /// <summary>
        /// Creates a new <see cref="StatisticItem"/>, to be displayed inside a <see cref="StatisticRow"/> in the results screen.
        /// </summary>
        /// <param name="name">The name of the item. Can be <see cref="string.Empty"/> to hide the item header.</param>
        /// <param name="content">The <see cref="Drawable"/> content to be displayed.</param>
        /// <param name="dimension">The <see cref="Dimension"/> of this item. This can be thought of as the column dimension of an encompassing <see cref="GridContainer"/>.</param>
        public StatisticItem([NotNull] string name, [NotNull] Drawable content, [CanBeNull] Dimension dimension = null)
        {
            Name = name;
            Content = content;
            Dimension = dimension;
        }
    }
}

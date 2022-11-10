// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using JetBrains.Annotations;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;

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
        public readonly LocalisableString Name;

        /// <summary>
        /// A function returning the <see cref="Drawable"/> content to be displayed.
        /// </summary>
        public readonly Func<Drawable> CreateContent;

        /// <summary>
        /// The <see cref="Dimension"/> of this row. This can be thought of as the column dimension of an encompassing <see cref="GridContainer"/>.
        /// </summary>
        public readonly Dimension Dimension;

        /// <summary>
        /// Whether this item requires hit events. If true, <see cref="CreateContent"/> will not be called if no hit events are available.
        /// </summary>
        public readonly bool RequiresHitEvents;

        /// <summary>
        /// Creates a new <see cref="StatisticItem"/>, to be displayed inside a <see cref="StatisticRow"/> in the results screen.
        /// </summary>
        /// <param name="name">The name of the item. Can be <see langword="null"/> to hide the item header.</param>
        /// <param name="createContent">A function returning the <see cref="Drawable"/> content to be displayed.</param>
        /// <param name="requiresHitEvents">Whether this item requires hit events. If true, <see cref="CreateContent"/> will not be called if no hit events are available.</param>
        /// <param name="dimension">The <see cref="Dimension"/> of this item. This can be thought of as the column dimension of an encompassing <see cref="GridContainer"/>.</param>
        public StatisticItem(LocalisableString name, [NotNull] Func<Drawable> createContent, bool requiresHitEvents = false, [CanBeNull] Dimension dimension = null)
        {
            Name = name;
            RequiresHitEvents = requiresHitEvents;
            CreateContent = createContent;
            Dimension = dimension;
        }
    }
}

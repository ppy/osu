// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Ranking.Statistics
{
    /// <summary>
    /// Represents a simple statistic item (one that only needs textual display).
    /// Richer visualisations should be done with <see cref="StatisticItem"/>s.
    /// </summary>
    public abstract partial class SimpleStatisticItem : Container
    {
        /// <summary>
        /// The text to display as the statistic's value.
        /// </summary>
        protected string Value
        {
            set => this.value.Text = value;
        }

        private readonly OsuSpriteText value;

        /// <summary>
        /// Creates a new simple statistic item.
        /// </summary>
        /// <param name="name">The name of the statistic.</param>
        protected SimpleStatisticItem(string name)
        {
            Name = name;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            AddRange(new[]
            {
                new OsuSpriteText
                {
                    Text = Name,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Font = OsuFont.GetFont(size: StatisticItem.FONT_SIZE)
                },
                value = new OsuSpriteText
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Font = OsuFont.GetFont(size: StatisticItem.FONT_SIZE, weight: FontWeight.Bold)
                }
            });
        }
    }

    /// <summary>
    /// Strongly-typed generic specialisation for <see cref="SimpleStatisticItem"/>.
    /// </summary>
    public partial class SimpleStatisticItem<TValue> : SimpleStatisticItem
    {
        private TValue value = default!;

        /// <summary>
        /// The statistic's value to be displayed.
        /// </summary>
        public new TValue Value
        {
            get => value;
            set
            {
                this.value = value;
                base.Value = DisplayValue(value);
            }
        }

        /// <summary>
        /// Used to convert <see cref="Value"/> to a text representation.
        /// Defaults to using <see cref="object.ToString"/>.
        /// </summary>
        protected virtual string DisplayValue(TValue value) => value!.ToString() ?? string.Empty;

        public SimpleStatisticItem(string name)
            : base(name)
        {
        }
    }
}

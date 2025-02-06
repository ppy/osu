// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Screens.SelectV2
{
    /// <summary>
    /// Represents a single display item for display in a <see cref="Carousel{T}"/>.
    /// This is used to house information related to the attached model that helps with display and tracking.
    /// </summary>
    public sealed class CarouselItem : IComparable<CarouselItem>
    {
        public const float DEFAULT_HEIGHT = 40;

        /// <summary>
        /// The model this item is representing.
        /// </summary>
        public readonly object Model;

        /// <summary>
        /// The current Y position in the carousel.
        /// This is managed by <see cref="Carousel{T}"/> and should not be set manually.
        /// </summary>
        public double CarouselYPosition { get; set; }

        /// <summary>
        /// The height this item will take when displayed. Defaults to <see cref="DEFAULT_HEIGHT"/>.
        /// </summary>
        public float DrawHeight { get; set; } = DEFAULT_HEIGHT;

        /// <summary>
        /// Defines the display depth relative to other <see cref="CarouselItem"/>s.
        /// </summary>
        public int DepthLayer { get; set; }

        /// <summary>
        /// Whether this item is visible or hidden.
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Whether this item is expanded or not. Should only be used for headers of groups.
        /// </summary>
        public bool IsExpanded { get; set; }

        public CarouselItem(object model)
        {
            Model = model;
        }

        public int CompareTo(CarouselItem? other)
        {
            if (other == null) return 1;

            return CarouselYPosition.CompareTo(other.CarouselYPosition);
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;

namespace osu.Game.Screens.SelectV2
{
    /// <summary>
    /// Represents a single display item for display in a <see cref="Carousel{T}"/>.
    /// This is used to house information related to the attached model that helps with display and tracking.
    /// </summary>
    public abstract class CarouselItem : IComparable<CarouselItem>
    {
        public readonly BindableBool Selected = new BindableBool();

        /// <summary>
        /// The model this item is representing.
        /// </summary>
        public readonly object Model;

        /// <summary>
        /// The current Y position in the carousel. This is managed by <see cref="Carousel{T}"/> and should not be set manually.
        /// </summary>
        public double CarouselYPosition { get; set; }

        /// <summary>
        /// The height this item will take when displayed.
        /// </summary>
        public abstract float DrawHeight { get; }

        protected CarouselItem(object model)
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

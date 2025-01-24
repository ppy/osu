// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Screens.SelectV2
{
    /// <summary>
    /// An interface to be attached to any <see cref="Drawable"/>s which are used for display inside a <see cref="Carousel{T}"/>.
    /// </summary>
    public interface ICarouselPanel
    {
        /// <summary>
        /// The Y position which should be used for displaying this item within the carousel. This is managed by <see cref="Carousel{T}"/> and should not be set manually.
        /// </summary>
        double DrawYPosition { get; set; }

        /// <summary>
        /// The carousel item this drawable is representing. This is managed by <see cref="Carousel{T}"/> and should not be set manually.
        /// </summary>
        CarouselItem? Item { get; set; }
    }
}

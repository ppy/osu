// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;

namespace osu.Game.Screens.SelectV2
{
    /// <summary>
    /// An interface to be attached to any <see cref="Drawable"/>s which are used for display inside a <see cref="Carousel{T}"/>.
    /// Importantly, all properties in this interface are managed by <see cref="Carousel{T}"/> and should not be written to elsewhere.
    /// </summary>
    public interface ICarouselPanel
    {
        /// <summary>
        /// Whether this item has selection (see <see cref="Carousel{T}.CurrentSelection"/>). Should be read from to update the visual state.
        /// </summary>
        BindableBool Selected { get; }

        /// <summary>
        /// Whether this item is expanded (see <see cref="CarouselItem.IsExpanded"/>). Should be read from to update the visual state.
        /// </summary>
        BindableBool Expanded { get; }

        /// <summary>
        /// Whether this item has keyboard selection. Should be read from to update the visual state.
        /// </summary>
        BindableBool KeyboardSelected { get; }

        /// <summary>
        /// Called when the panel is activated. Should be used to update the panel's visual state.
        /// </summary>
        void Activated();

        /// <summary>
        /// The Y position used internally for positioning in the carousel.
        /// </summary>
        double DrawYPosition { get; set; }

        /// <summary>
        /// The carousel item this drawable is representing. Will be set before <see cref="PoolableDrawable.PrepareForUse"/> is called.
        /// </summary>
        CarouselItem? Item { get; set; }
    }
}

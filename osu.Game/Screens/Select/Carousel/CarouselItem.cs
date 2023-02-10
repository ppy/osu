// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;

namespace osu.Game.Screens.Select.Carousel
{
    public abstract class CarouselItem : IComparable<CarouselItem>
    {
        public virtual float TotalHeight => 0;

        /// <summary>
        /// An externally defined value used to determine this item's vertical display offset relative to the carousel.
        /// </summary>
        public float CarouselYPosition;

        public readonly BindableBool Filtered = new BindableBool();

        public readonly Bindable<CarouselItemState> State = new Bindable<CarouselItemState>(CarouselItemState.NotSelected);

        /// <summary>
        /// This item is not in a hidden state.
        /// </summary>
        public bool Visible => State.Value != CarouselItemState.Collapsed && !Filtered.Value;

        protected CarouselItem()
        {
            Filtered.ValueChanged += filtered =>
            {
                if (filtered.NewValue && State.Value == CarouselItemState.Selected)
                    State.Value = CarouselItemState.NotSelected;
            };
        }

        /// <summary>
        /// Used as a default sort method for <see cref="CarouselItem"/>s of differing types.
        /// </summary>
        internal ulong ItemID;

        /// <summary>
        /// Create a fresh drawable version of this item.
        /// </summary>
        public abstract DrawableCarouselItem? CreateDrawableRepresentation();

        public virtual void Filter(FilterCriteria criteria)
        {
        }

        public virtual int CompareTo(FilterCriteria criteria, CarouselItem other) => ItemID.CompareTo(other.ItemID);

        public int CompareTo(CarouselItem? other)
        {
            if (other == null) return 1;

            return CarouselYPosition.CompareTo(other.CarouselYPosition);
        }
    }

    public enum CarouselItemState
    {
        Collapsed,
        NotSelected,
        Selected,
    }
}

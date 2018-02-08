// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Configuration;

namespace osu.Game.Screens.Select.Carousel
{
    public abstract class CarouselItem
    {
        public readonly BindableBool Filtered = new BindableBool();

        public readonly Bindable<CarouselItemState> State = new Bindable<CarouselItemState>(CarouselItemState.NotSelected);

        /// <summary>
        /// This item is not in a hidden state.
        /// </summary>
        public bool Visible => State.Value != CarouselItemState.Collapsed && !Filtered;

        public virtual List<DrawableCarouselItem> Drawables
        {
            get
            {
                var items = new List<DrawableCarouselItem>();

                var self = drawableRepresentation.Value;
                if (self?.IsPresent == true) items.Add(self);

                return items;
            }
        }

        protected CarouselItem()
        {
            drawableRepresentation = new Lazy<DrawableCarouselItem>(CreateDrawableRepresentation);

            Filtered.ValueChanged += v =>
            {
                if (v && State == CarouselItemState.Selected)
                    State.Value = CarouselItemState.NotSelected;
            };
        }

        private readonly Lazy<DrawableCarouselItem> drawableRepresentation;

        protected abstract DrawableCarouselItem CreateDrawableRepresentation();

        public virtual void Filter(FilterCriteria criteria)
        {
        }

        public virtual int CompareTo(FilterCriteria criteria, CarouselItem other) => GetHashCode().CompareTo(other.GetHashCode());
    }

    public enum CarouselItemState
    {
        Collapsed,
        NotSelected,
        Selected,
    }
}

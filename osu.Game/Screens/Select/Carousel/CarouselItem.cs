// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
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

        public IReadOnlyList<CarouselItem> Children => InternalChildren;

        protected List<CarouselItem> InternalChildren { get; set; }

        /// <summary>
        /// This item is not in a hidden state.
        /// </summary>
        public bool Visible => State.Value != CarouselItemState.Collapsed && !Filtered;

        public IEnumerable<DrawableCarouselItem> Drawables
        {
            get
            {
                List<DrawableCarouselItem> items = new List<DrawableCarouselItem>();

                var self = drawableRepresentation.Value;
                if (self?.IsPresent == true) items.Add(self);

                if (InternalChildren != null)
                    foreach (var c in InternalChildren)
                        items.AddRange(c.Drawables);

                return items;
            }
        }

        public virtual void AddChild(CarouselItem i) => (InternalChildren ?? (InternalChildren = new List<CarouselItem>())).Add(i);

        public virtual void RemoveChild(CarouselItem i)
        {
            InternalChildren?.Remove(i);

            // it's important we do the deselection after removing, so any further actions based on
            // State.ValueChanged make decisions post-removal.
            i.State.Value = CarouselItemState.Collapsed;
        }

        protected CarouselItem()
        {
            drawableRepresentation = new Lazy<DrawableCarouselItem>(CreateDrawableRepresentation);

            State.ValueChanged += v =>
            {
                if (InternalChildren == null) return;

                switch (v)
                {
                    case CarouselItemState.Collapsed:
                    case CarouselItemState.NotSelected:
                        InternalChildren.ForEach(c => c.State.Value = CarouselItemState.Collapsed);
                        break;
                    case CarouselItemState.Selected:
                        InternalChildren.ForEach(c =>
                        {
                            if (c.State == CarouselItemState.Collapsed) c.State.Value = CarouselItemState.NotSelected;
                        });
                        break;
                }
            };

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
            InternalChildren?.Sort((x, y) => x.CompareTo(criteria, y));
            InternalChildren?.ForEach(c => c.Filter(criteria));
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

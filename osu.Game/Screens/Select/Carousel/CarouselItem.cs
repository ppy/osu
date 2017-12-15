// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Configuration;
using osu.Framework.Logging;

namespace osu.Game.Screens.Select.Carousel
{
    public abstract class CarouselItem
    {
        public readonly BindableBool Filtered = new BindableBool();

        public readonly Bindable<CarouselItemState> State = new Bindable<CarouselItemState>(CarouselItemState.NotSelected);

        public IReadOnlyList<CarouselItem> Children => InternalChildren;

        protected List<CarouselItem> InternalChildren { get; set; }

        public bool Visible => State.Value != CarouselItemState.Hidden && !Filtered.Value;

        public IEnumerable<DrawableCarouselItem> Drawables
        {
            get
            {
                List<DrawableCarouselItem> items = new List<DrawableCarouselItem>();

                var self = drawableRepresentation.Value;
                if (self != null) items.Add(self);

                if (InternalChildren != null)
                    foreach (var c in InternalChildren)
                        items.AddRange(c.Drawables);

                return items;
            }
        }

        public virtual void AddChild(CarouselItem i) => (InternalChildren ?? (InternalChildren = new List<CarouselItem>())).Add(i);

        public virtual void RemoveChild(CarouselItem i) => InternalChildren?.Remove(i);

        protected CarouselItem()
        {
            drawableRepresentation = new Lazy<DrawableCarouselItem>(CreateDrawableRepresentation);

            State.ValueChanged += v =>
            {
                if (InternalChildren == null) return;

                Logger.Log($"State changed to {v}");

                switch (v)
                {
                    case CarouselItemState.Hidden:
                    case CarouselItemState.NotSelected:
                        InternalChildren.ForEach(c => c.State.Value = CarouselItemState.Hidden);
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
        Hidden,
        NotSelected,
        Selected,
    }
}

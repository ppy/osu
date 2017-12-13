// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Configuration;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace osu.Game.Screens.Select.Carousel
{
    public abstract class CarouselItem
    {
        public readonly BindableBool Filtered = new BindableBool();

        public readonly Bindable<CarouselItemState> State = new Bindable<CarouselItemState>(CarouselItemState.NotSelected);

        protected virtual IEnumerable<CarouselItem> Children { get; set; }

        public bool Visible => State.Value != CarouselItemState.Hidden && !Filtered.Value;

        public readonly Lazy<IEnumerable<DrawableCarouselItem>> Drawables;

        protected CarouselItem()
        {
            Drawables = new Lazy<IEnumerable<DrawableCarouselItem>>(() =>
            {
                List<DrawableCarouselItem> items = new List<DrawableCarouselItem>();

                var self = CreateDrawableRepresentation();
                if (self != null) items.Add(self);

                if (Children != null)
                    foreach (var c in Children)
                        items.AddRange(c.Drawables.Value);

                return items;
            });

            State.ValueChanged += v =>
            {
                if (Children == null) return;

                switch (v)
                {
                    case CarouselItemState.Hidden:
                    case CarouselItemState.NotSelected:
                        Children.ForEach(c => c.State.Value = CarouselItemState.Hidden);
                        break;
                }
            };
        }

        protected abstract DrawableCarouselItem CreateDrawableRepresentation();

        public virtual void Filter(FilterCriteria criteria) => Children?.ForEach(c => c.Filter(criteria));
    }

    public enum CarouselItemState
    {
        Hidden,
        NotSelected,
        Selected,
    }
}

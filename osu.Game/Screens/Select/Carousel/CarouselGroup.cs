// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Configuration;

namespace osu.Game.Screens.Select.Carousel
{
    /// <summary>
    /// A group which ensures only one child is selected.
    /// </summary>
    public class CarouselGroup : CarouselItem
    {
        private readonly List<CarouselItem> items;

        public readonly Bindable<CarouselItem> Selected = new Bindable<CarouselItem>();

        protected override DrawableCarouselItem CreateDrawableRepresentation() => null;

        public override void AddChild(CarouselItem i)
        {
            i.State.ValueChanged += v => ItemStateChanged(i, v);
            base.AddChild(i);
        }

        public CarouselGroup(List<CarouselItem> items = null)
        {
            if (items != null) InternalChildren = items;
        }

        protected virtual void ItemStateChanged(CarouselItem item, CarouselItemState value)
        {
            // todo: check state of selected item.

            // ensure we are the only item selected
            if (value == CarouselItemState.Selected)
            {
                foreach (var b in InternalChildren)
                {
                    if (item == b) continue;
                    b.State.Value = CarouselItemState.NotSelected;
                }

                State.Value = CarouselItemState.Selected;
                Selected.Value = item;
            }
        }
    }
}

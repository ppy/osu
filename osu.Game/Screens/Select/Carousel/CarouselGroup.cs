// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Configuration;
using osu.Framework.Extensions.IEnumerableExtensions;

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

        protected override IEnumerable<CarouselItem> Children
        {
            get { return base.Children; }
            set
            {
                base.Children = value;
                value.ForEach(i => i.State.ValueChanged += v => itemStateChanged(i, v));
            }
        }

        public CarouselGroup(List<CarouselItem> items = null)
        {
            if (items != null) Children = items;
        }

        private void itemStateChanged(CarouselItem item, CarouselItemState value)
        {
            // todo: check state of selected item.

            // ensure we are the only item selected
            if (value == CarouselItemState.Selected)
            {
                foreach (var b in Children)
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

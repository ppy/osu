// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;

namespace osu.Game.Screens.Select.Carousel
{
    /// <summary>
    /// A group which ensures at least one child is selected (if the group itself is selected).
    /// </summary>
    public class CarouselGroupEagerSelect : CarouselGroup
    {
        public CarouselGroupEagerSelect()
        {
            State.ValueChanged += v =>
            {
                if (v == CarouselItemState.Selected)
                    attemptSelection();
            };
        }

        private int lastSelectedIndex;

        public override void Filter(FilterCriteria criteria)
        {
            base.Filter(criteria);
            attemptSelection();
        }

        protected override void ChildItemStateChanged(CarouselItem item, CarouselItemState value)
        {
            base.ChildItemStateChanged(item, value);

            switch (value)
            {
                case CarouselItemState.Selected:
                    lastSelectedIndex = InternalChildren.IndexOf(item);
                    break;
                case CarouselItemState.NotSelected:
                    attemptSelection();
                    break;
            }
        }

        private void attemptSelection()
        {
            // we only perform eager selection if we are a currently selected group.
            if (State != CarouselItemState.Selected) return;

            // we only perform eager selection if none of our children are in a selected state already.
            if (Children.Any(i => i.State == CarouselItemState.Selected)) return;

            CarouselItem nextToSelect =
                Children.Skip(lastSelectedIndex).FirstOrDefault(i => !i.Filtered) ??
                Children.Reverse().Skip(InternalChildren.Count - lastSelectedIndex).FirstOrDefault(i => !i.Filtered);

            if (nextToSelect != null)
                nextToSelect.State.Value = CarouselItemState.Selected;
        }
    }
}

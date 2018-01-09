// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
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

        /// <summary>
        /// We need to keep track of the index for cases where the selection is removed but we want to select a new item based on its old location.
        /// </summary>
        private int lastSelectedIndex;

        private CarouselItem lastSelected;

        /// <summary>
        /// To avoid overhead during filter operations, we don't attempt any selections until after all
        /// children have been filtered. This bool will be true during the base <see cref="Filter(FilterCriteria)"/>
        /// operation.
        /// </summary>
        private bool filteringChildren;

        public override void Filter(FilterCriteria criteria)
        {
            filteringChildren = true;
            base.Filter(criteria);
            filteringChildren = false;

            attemptSelection();
        }

        public override void RemoveChild(CarouselItem i)
        {
            base.RemoveChild(i);

            if (i != lastSelected)
                updateSelectedIndex();
        }

        public override void AddChild(CarouselItem i)
        {
            base.AddChild(i);
            attemptSelection();
        }

        protected override void ChildItemStateChanged(CarouselItem item, CarouselItemState value)
        {
            base.ChildItemStateChanged(item, value);

            switch (value)
            {
                case CarouselItemState.Selected:
                    updateSelected(item);
                    break;
                case CarouselItemState.NotSelected:
                case CarouselItemState.Collapsed:
                    attemptSelection();
                    break;
            }
        }

        private void attemptSelection()
        {
            if (filteringChildren) return;

            // we only perform eager selection if we are a currently selected group.
            if (State != CarouselItemState.Selected) return;

            // we only perform eager selection if none of our children are in a selected state already.
            if (Children.Any(i => i.State == CarouselItemState.Selected)) return;

            CarouselItem nextToSelect =
                Children.Skip(lastSelectedIndex).FirstOrDefault(i => !i.Filtered) ??
                Children.Reverse().Skip(InternalChildren.Count - lastSelectedIndex).FirstOrDefault(i => !i.Filtered);

            if (nextToSelect != null)
                nextToSelect.State.Value = CarouselItemState.Selected;
            else
                updateSelected(null);
        }

        private void updateSelected(CarouselItem newSelection)
        {
            lastSelected = newSelection;
            updateSelectedIndex();
        }

        private void updateSelectedIndex() => lastSelectedIndex = lastSelected == null ? 0 : Math.Max(0, InternalChildren.IndexOf(lastSelected));
    }
}

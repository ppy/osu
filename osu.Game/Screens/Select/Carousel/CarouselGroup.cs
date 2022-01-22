// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace osu.Game.Screens.Select.Carousel
{
    /// <summary>
    /// A group which ensures only one child is selected.
    /// </summary>
    public class CarouselGroup : CarouselItem
    {
        public override DrawableCarouselItem? CreateDrawableRepresentation() => null;

        public IReadOnlyList<CarouselItem> Children => InternalChildren;

        protected List<CarouselItem> InternalChildren = new List<CarouselItem>();

        /// <summary>
        /// Used to assign a monotonically increasing ID to children as they are added. This member is
        /// incremented whenever a child is added.
        /// </summary>
        private ulong currentChildID;

        private Comparer<CarouselItem>? criteriaComparer;

        private FilterCriteria? lastCriteria;

        public virtual void RemoveChild(CarouselItem i)
        {
            InternalChildren.Remove(i);

            // it's important we do the deselection after removing, so any further actions based on
            // State.ValueChanged make decisions post-removal.
            i.State.Value = CarouselItemState.Collapsed;
        }

        public virtual void AddChild(CarouselItem i)
        {
            i.State.ValueChanged += state => ChildItemStateChanged(i, state.NewValue);
            i.ChildID = ++currentChildID;

            if (lastCriteria != null)
            {
                i.Filter(lastCriteria);

                int index = InternalChildren.BinarySearch(i, criteriaComparer);
                if (index < 0) index = ~index; // BinarySearch hacks multiple return values with 2's complement.

                InternalChildren.Insert(index, i);
            }
            else
            {
                // criteria may be null for initial population. the filtering will be applied post-add.
                InternalChildren.Add(i);
            }
        }

        public CarouselGroup(List<CarouselItem>? items = null)
        {
            if (items != null) InternalChildren = items;

            State.ValueChanged += state =>
            {
                switch (state.NewValue)
                {
                    case CarouselItemState.Collapsed:
                    case CarouselItemState.NotSelected:
                        InternalChildren.ForEach(c => c.State.Value = CarouselItemState.Collapsed);
                        break;

                    case CarouselItemState.Selected:
                        InternalChildren.ForEach(c =>
                        {
                            if (c.State.Value == CarouselItemState.Collapsed) c.State.Value = CarouselItemState.NotSelected;
                        });
                        break;
                }
            };
        }

        public override void Filter(FilterCriteria criteria)
        {
            base.Filter(criteria);

            InternalChildren.ForEach(c => c.Filter(criteria));

            // IEnumerable<T>.OrderBy() is used instead of List<T>.Sort() to ensure sorting stability
            criteriaComparer = Comparer<CarouselItem>.Create((x, y) => x.CompareTo(criteria, y));
            InternalChildren = InternalChildren.OrderBy(c => c, criteriaComparer).ToList();

            lastCriteria = criteria;
        }

        protected virtual void ChildItemStateChanged(CarouselItem item, CarouselItemState value)
        {
            // ensure we are the only item selected
            if (value == CarouselItemState.Selected)
            {
                foreach (var b in InternalChildren)
                {
                    if (item == b) continue;

                    b.State.Value = CarouselItemState.NotSelected;
                }

                State.Value = CarouselItemState.Selected;
            }
        }
    }
}

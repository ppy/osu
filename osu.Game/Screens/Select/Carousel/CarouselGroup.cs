// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;

namespace osu.Game.Screens.Select.Carousel
{
    /// <summary>
    /// A group which ensures only one item is selected.
    /// </summary>
    public class CarouselGroup : CarouselItem
    {
        public override DrawableCarouselItem? CreateDrawableRepresentation() => null;

        public IReadOnlyList<CarouselItem> Items => items;

        public int TotalItemsNotFiltered { get; private set; }

        private readonly List<CarouselItem> items = new List<CarouselItem>();

        /// <summary>
        /// Used to assign a monotonically increasing ID to items as they are added. This member is
        /// incremented whenever an item is added.
        /// </summary>
        private ulong currentItemID;

        private Comparer<CarouselItem>? criteriaComparer;
        private FilterCriteria? lastCriteria;

        protected int GetIndexOfItem(CarouselItem lastSelected) => items.IndexOf(lastSelected);

        public virtual void RemoveItem(CarouselItem i)
        {
            items.Remove(i);

            if (!i.Filtered.Value)
                TotalItemsNotFiltered--;

            // it's important we do the deselection after removing, so any further actions based on
            // State.ValueChanged make decisions post-removal.
            i.State.Value = CarouselItemState.Collapsed;
        }

        public virtual void AddItem(CarouselItem i)
        {
            i.State.ValueChanged += state => ChildItemStateChanged(i, state.NewValue);
            i.ItemID = ++currentItemID;

            if (lastCriteria != null)
            {
                i.Filter(lastCriteria);

                int index = items.BinarySearch(i, criteriaComparer);
                if (index < 0) index = ~index; // BinarySearch hacks multiple return values with 2's complement.

                items.Insert(index, i);
            }
            else
            {
                // criteria may be null for initial population. the filtering will be applied post-add.
                items.Add(i);
            }

            if (!i.Filtered.Value)
                TotalItemsNotFiltered++;
        }

        public CarouselGroup(List<CarouselItem>? items = null)
        {
            if (items != null) this.items = items;

            State.ValueChanged += state =>
            {
                switch (state.NewValue)
                {
                    case CarouselItemState.Collapsed:
                    case CarouselItemState.NotSelected:
                        this.items.ForEach(c => c.State.Value = CarouselItemState.Collapsed);
                        break;

                    case CarouselItemState.Selected:
                        this.items.ForEach(c =>
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

            TotalItemsNotFiltered = 0;

            foreach (var c in items)
            {
                c.Filter(criteria);
                if (!c.Filtered.Value)
                    TotalItemsNotFiltered++;
            }

            // Sorting is expensive, so only perform if it's actually changed.
            if (lastCriteria?.RequiresSorting(criteria) != false)
            {
                criteriaComparer = Comparer<CarouselItem>.Create((x, y) =>
                {
                    int comparison = x.CompareTo(criteria, y);
                    if (comparison != 0)
                        return comparison;

                    return x.ItemID.CompareTo(y.ItemID);
                });

                items.Sort(criteriaComparer);
            }

            lastCriteria = criteria;
        }

        protected virtual void ChildItemStateChanged(CarouselItem item, CarouselItemState value)
        {
            // ensure we are the only item selected
            if (value == CarouselItemState.Selected)
            {
                foreach (var b in items)
                {
                    if (item == b) continue;

                    b.State.Value = CarouselItemState.NotSelected;
                }

                State.Value = CarouselItemState.Selected;
            }
        }
    }
}

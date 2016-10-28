using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;

namespace osu.Game.Graphics.UserInterface
{
    public class FilteringSearchList<T> : Container
        where T : Drawable
    {
        public List<FilteringSearchListItem<T>> Items { get; }
        public FilteringSearchListItem<T> SelectedItem { get; private set; }
        private Action<T> selectedItemAction, notSelectedItemAction;

        private FlowContainer contentFlowContainer;

        public FilteringSearchList(List<T> items, Action<T> selectedItemAction, Action<T> notSelectedItemAction, T selectedItem = null)
        {
            this.selectedItemAction = selectedItemAction;
            this.notSelectedItemAction = notSelectedItemAction;
            Items = new List<FilteringSearchListItem<T>>();
            foreach (var item in items)
            {
                var filteringSearchListItem = new FilteringSearchListItem<T>(item);
                filteringSearchListItem.OnSelected += selectionChanged;
                Items.Add(filteringSearchListItem);
            }
            SelectedItem = selectedItem != null ? Items.First(i => i.Item == selectedItem) : null;
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);

            Children = new Drawable[]
            {
                new ScrollContainer
                {
                    Children = new Drawable[]
                    {
                        contentFlowContainer = new FlowContainer
                        {
                            Direction = FlowDirection.VerticalOnly,
                            Children = Items
                        }
                    }
                }
            };
        }

        public void Filter(Func<T, bool> predicate)
        {
            foreach (var item in Items)
            {
                if (predicate(item.Item))
                    item.Show();
                else
                    item.Hide();
            }
            contentFlowContainer.Invalidate();
        }

        private void selectionChanged(object sender)
        {
            SelectedItem = (FilteringSearchListItem<T>)sender;
            selectedItemAction(SelectedItem.Item);
            foreach (var item in Items)
            {
                if (item != SelectedItem)
                {
                    item.State = SelectionState.NotSelected;
                    notSelectedItemAction(item.Item);
                }
            }
        }

        public class FilteringSearchListItem<T> : AutoSizeContainer, IStateful<SelectionState>
            where T : Drawable
        {
            public T Item { get; set; }

            public SelectionState State
            {
                get { return state; }
                set
                {
                    state = value;
                    if (value == SelectionState.Selected)
                        OnSelected?.Invoke(this);
                }
            }

            private SelectionState state;

            public FilteringSearchListItem(T item, SelectionState state = SelectionState.NotSelected)
            {
                Item = item;
                State = state;
            }

            public override void Load(BaseGame game)
            {
                base.Load(game);

                Children = new Drawable[]
                {
                    Item
                };
            }

            protected override bool OnClick(InputState state)
            {
                State = SelectionState.Selected;
                return true;
            }

            public delegate void OnSelectedHandler(object sender);

            public event OnSelectedHandler OnSelected;
        }

        public enum SelectionState
        {
            Selected,
            NotSelected
        }
    }
}
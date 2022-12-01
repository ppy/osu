// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Screens.Edit.List
{
    public partial class DrawableMinimisableList<T> : RearrangeableListItem<IDrawableListRepresetedItem<T>>, IRearrangableDrawableListItem<T>
        where T : Drawable
    {
        private Action<T, int> setItemDepth = IDrawableListItem<T>.DEFAULT_SET_ITEM_DEPTH;

        public Action<T, int> SetItemDepth
        {
            get => setItemDepth;
            set
            {
                setItemDepth = value;
                if (List is not null) List.SetItemDepth = value;
            }
        }

        private Action onDragAction = () => { };

        public Action OnDragAction
        {
            get => onDragAction;
            set
            {
                onDragAction = value;
                if (List is not null) List.OnDragAction = value;
            }
        }

        private Action<Action<IDrawableListItem<T>>> applyAll;

        public Action<Action<IDrawableListItem<T>>> ApplyAll
        {
            get => applyAll;
            set
            {
                applyAll = value;
                if (List is not null) List.ApplyAll = value;
            }
        }

        private Func<T, LocalisableString> getName = IDrawableListItem<T>.GetDefaultText;

        public Func<T, LocalisableString> GetName
        {
            get => getName;
            set
            {
                if (List is not null) List.GetName = value;
                getName = value;
            }
        }

        public BindableBool Enabled { get; } = new BindableBool();
        public readonly DrawableList<T>? List;
        public T? RepresentedItem => Model.RepresentedItem;

        private readonly DrawableListItem<T> representedListItem;

        public DrawableMinimisableList(T item)
            : base(new DrawableListRepresetedItem<T>(item))
        {
            SpriteIcon icon;
            Container head;
            ClickableContainer headClickableContainer;
            StateChanged = t =>
            {
                switch (t)
                {
                    case SelectionState.Selected:
                        Selected.Invoke();
                        break;

                    case SelectionState.NotSelected:
                        Deselected.Invoke();
                        break;
                }
            };
            applyAll = ApplyAction;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            InternalChildren = new Drawable[]
            {
                // RelativeSizeAxes = Axes.X,
                // AutoSizeAxes = Axes.Y,
                head = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        headClickableContainer = new ClickableContainer
                        {
                            RelativeSizeAxes = Axes.Y,
                            AutoSizeAxes = Axes.X,
                            Child = icon = new SpriteIcon
                            {
                                Size = new Vector2(8),
                                Icon = FontAwesome.Solid.ChevronRight,
                                Margin = new MarginPadding { Left = 3, Right = 3 },
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                            },
                        },
                        representedListItem = new DrawableListItem<T>(Model)
                        {
                            X = icon.LayoutSize.X,
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.CentreLeft,
                            ApplyAll = ApplyAll,
                            GetName = getName,
                            SetItemDepth = SetItemDepth,
                            OnDragAction = OnDragAction,
                        }
                    }
                },
                List = new DrawableList<T>()
            };

            headClickableContainer.Action = () =>
            {
                Enabled.Toggle();
                icon.Icon = Enabled.Value ? FontAwesome.Solid.ChevronDown : FontAwesome.Solid.ChevronRight;

                if (List is not null)
                {
                    List.X = icon.LayoutSize.X;
                    List.Y = head.LayoutSize.Y;
                    // List.Height = ChildSize.Y - head.LayoutSize.Y;
                    // List.OriginPosition = new Vector2(icon.LayoutSize.X, head.LayoutSize.Y);
                    // List.RelativeAnchorPosition = Vector2.Divide(List.Position, List.Parent?.ChildSize ?? Vector2.One * float.MaxValue);
                }
            };

            List.ItemAdded += t =>
            {
                if (t is IRearrangableDrawableListItem<T> listItem)
                {
                    listItem.Deselected += () =>
                    {
                        //If all elements are not selected we want to also deselect this element
                        if (checkAllSelectedState(SelectionState.NotSelected)) Deselect();
                        //If some elements are still selected, keep them selected, but deselect the representedListItem.
                        else representedListItem.Deselect();
                    };
                    listItem.Selected += () =>
                    {
                        //if all elements of a List are selected, the representedListItem should also be selected
                        if (checkAllSelectedState(SelectionState.Selected)) Select();
                    };
                }
            };
            representedListItem.Selected += Select;
            representedListItem.Deselected += () =>
            {
                //if all items are selected, then actually deselect all items.
                if (checkAllSelectedState(SelectionState.Selected)) Deselect();
                //else we just want to deselect the representedListItem, because we don't actually know if the representedListItem gotClicked
                //or we deselected it manually through a deselection of a child element
                StateChanged.Invoke(SelectionState.NotSelected);
            };

            Enabled.BindValueChanged(v =>
            {
                if (v.NewValue) ShowList(false);
                else HideList(false);
            }, true);

            Deselect();
            Scheduler.Add(UpdateItem);
        }

        private bool checkAllSelectedState(SelectionState state)
        {
            if (List is null) return false;

            foreach (var item in List.ItemMaps.Values)
            {
                if (item is IRearrangableDrawableListItem<T> rearrangeableItem && rearrangeableItem.State != state) return false;
            }

            return true;
        }

        public void ShowList(bool setValue = true)
        {
            List?.Show();
            UpdateItem();
            if (setValue) Enabled.Value = true;
        }

        public void HideList(bool setValue = true)
        {
            List?.Hide();
            UpdateItem();
            if (setValue) Enabled.Value = false;
        }

        public void UpdateItem()
        {
            representedListItem.ApplyAll = ApplyAll;
            representedListItem.GetName = getName;
            representedListItem.SetItemDepth = SetItemDepth;
            representedListItem.OnDragAction = OnDragAction;
            representedListItem.UpdateItem();
            List?.UpdateItem();
        }

        public void Select()
        {
            representedListItem.Select();
            List?.Select();
            StateChanged.Invoke(SelectionState.Selected);
        }

        public void Deselect()
        {
            representedListItem.Deselect();
            List?.Deselect();
            StateChanged.Invoke(SelectionState.NotSelected);
        }

        public void ApplyAction(Action<IDrawableListItem<T>> action) => List?.ApplyAction(action);

        public void SelectInternal()
        {
            representedListItem.SelectInternal();
            List?.SelectInternal();
        }

        public void DeselectInternal()
        {
            representedListItem.DeselectInternal();
            List?.DeselectInternal();
        }

        public Drawable GetDrawableListItem() => this;
        public RearrangeableListItem<IDrawableListRepresetedItem<T>> GetRearrangeableListItem() => this;

        public SelectionState State
        {
            get => representedListItem.State;
            set
            {
                switch (value)
                {
                    case SelectionState.Selected:
                        Select();
                        break;

                    case SelectionState.NotSelected:
                        Deselect();
                        break;
                }
            }
        }

        public event Action<SelectionState> StateChanged;
        public event Action Selected = () => { };
        public event Action Deselected = () => { };
    }
}

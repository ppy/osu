// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Screens.Edit.List
{
    public partial class DrawableMinimisableList<T> : AbstractListItem<T>
        where T : Drawable
    {
        private Action<Action<IDrawableListItem<T>>> applyAll;

        public override Action<Action<IDrawableListItem<T>>> ApplyAll
        {
            get => applyAll;
            set
            {
                applyAll = value;
                if (List is not null) List.ApplyAll = value;
            }
        }

        public BindableBool Enabled { get; } = new BindableBool();
        public readonly DrawableList<T>? List;

        private readonly DrawableListItem<T> representedListItem;

        public DrawableMinimisableList(T item)
            : this(new DrawableListRepresetedItem<T>(item, DrawableListEntryType.MinimisableList))
        {
        }

        public DrawableMinimisableList(DrawableListRepresetedItem<T> item)
            : base(item)
        {
            SpriteIcon icon;
            Container head;
            ClickableContainer headClickableContainer;
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
                            GetName = GetName,
                            SetItemDepth = SetItemDepth,
                            OnDragAction = OnDragAction,
                        }
                    }
                },
                List = new DrawableList<T>
                {
                    GetName = GetName,
                    SetItemDepth = SetItemDepth,
                    OnDragAction = OnDragAction,
                    ApplyAll = ApplyAll,
                }
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
                InvokeStateChanged(SelectionState.NotSelected);
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

        public override void UpdateItem()
        {
            representedListItem.ApplyAll = ApplyAll;
            representedListItem.GetName = GetName;
            representedListItem.SetItemDepth = SetItemDepth;
            representedListItem.OnDragAction = OnDragAction;
            representedListItem.UpdateItem();

            if (List is not null)
            {
                List.ApplyAll = ApplyAll;
                List.GetName = GetName;
                List.SetItemDepth = SetItemDepth;
                List.OnDragAction = OnDragAction;
                List.UpdateItem();
            }
        }

        public override void Select()
        {
            representedListItem.Select();
            List?.Select();
            InvokeStateChanged(SelectionState.Selected);
        }

        public override void Deselect()
        {
            representedListItem.Deselect();
            List?.Deselect();
            InvokeStateChanged(SelectionState.NotSelected);
        }

        public override void ApplyAction(Action<IDrawableListItem<T>> action)
        {
            representedListItem.ApplyAction(action);
            List?.ApplyAction(action);
        }

        public override void SelectInternal(bool invokeChildMethods = true)
        {
            if (invokeChildMethods)
            {
                representedListItem.SelectInternal();
                List?.SelectInternal();
            }
        }

        public override void DeselectInternal(bool invokeChildMethods = true)
        {
            if (invokeChildMethods)
            {
                representedListItem.DeselectInternal();
                List?.DeselectInternal();
            }
        }

        public override SelectionState State
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

        protected override void OnSetItemDepth(ref Action<T, int> value)
        {
            if (List is not null) List.SetItemDepth = value;
        }

        protected override void OnSetDragAction(ref Action value)
        {
            if (List is not null) List.OnDragAction = value;
        }
    }
}

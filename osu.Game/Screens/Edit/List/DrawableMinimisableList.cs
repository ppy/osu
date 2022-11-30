// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework;
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

            if (RepresentedItem is IStateful<SelectionState> selectable)
            {
                selectable.StateChanged += this.ApplySelectionState;
                this.ApplySelectionState(selectable.State);
            }

            Enabled.BindValueChanged(v =>
            {
                if (v.NewValue) ShowList();
                else HideList();
            }, true);

            Deselect();
            representedListItem.UpdateItem();
        }

        public void ShowList(bool setValue = false)
        {
            List?.Show();
            UpdateItem();
            if (setValue) Enabled.Value = true;
        }

        public void HideList(bool setValue = false)
        {
            List?.Hide();
            UpdateItem();
            if (setValue) Enabled.Value = false;
        }

        public void UpdateItem()
        {
            representedListItem.UpdateItem();
            List?.UpdateItem();
        }

        public void Select()
        {
            representedListItem.Select();
            List?.Select();
        }

        public void Deselect()
        {
            representedListItem.Deselect();
            List?.Deselect();
        }

        public void ApplyAction(Action<IDrawableListItem<T>> action) => List?.ApplyAction(action);
        void IDrawableListItem<T>.SelectInternal() => SelectInternal();
        void IDrawableListItem<T>.DeselectInternal() => DeselectInternal();

        public void SelectInternal(bool passThroughCall = true)
        {
            representedListItem.SelectInternal();
            if (passThroughCall) List?.SelectInternal();
        }

        public void DeselectInternal(bool passThroughCall = true)
        {
            representedListItem.DeselectInternal();
            if (passThroughCall) List?.DeselectInternal();
        }

        public Drawable GetDrawableListItem() => this;
        public RearrangeableListItem<IDrawableListRepresetedItem<T>> GetRearrangeableListItem() => this;
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;
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

        private readonly Box box;
        private readonly OsuSpriteText text;

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
                // Direction = FillDirection.Vertical,
                // Spacing = new Vector2(2),
                // Children = new Drawable[]
                head = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        box = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 1f,
                            Height = 1f,
                            Colour = new Colour4(255, 255, 0, 0.25f),
                        },
                        headClickableContainer = new ClickableContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                icon = new SpriteIcon
                                {
                                    Size = new Vector2(8),
                                    Icon = FontAwesome.Solid.ChevronRight,
                                    Margin = new MarginPadding { Left = 3, Right = 3 },
                                    Origin = Anchor.CentreLeft,
                                    Anchor = Anchor.CentreLeft,
                                },
                                text = new OsuSpriteText
                                {
                                    Text = GetName(item),
                                    X = icon.LayoutSize.X,
                                    Origin = Anchor.CentreLeft,
                                    Anchor = Anchor.CentreLeft,
                                }
                            },
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

            if (!((IDrawableListItem<T>)this).EnableSelection)
            {
                box.RemoveAndDisposeImmediately();
                box = new Box();
            }

            if (RepresentedItem is IStateful<SelectionState> selectable)
            {
                selectable.StateChanged += this.ApplySelectionState;
                this.ApplySelectionState(selectable.State);
            }

            box.Hide();
            Enabled.BindValueChanged(v => SetShown(v.NewValue), true);
            Deselect();
            updateText();
        }

        public void SetShown(bool value, bool setValue = false)
        {
            if (value) List?.Show();
            else List?.Hide();
            UpdateItem();
            if (setValue) Enabled.Value = value;
        }

        private void updateText()
        {
            if (RepresentedItem is not null) Scheduler.Add(() => text.Text = GetName(RepresentedItem));
        }

        public void UpdateItem()
        {
            updateText();
            List?.UpdateItem();
        }

        public void Select()
        {
            if (RepresentedItem is IStateful<SelectionState> selectable)
                selectable.State = SelectionState.Selected;
            SelectInternal(false);
            List?.Select();
        }

        public void Deselect()
        {
            if (RepresentedItem is IStateful<SelectionState> selectable)
                selectable.State = SelectionState.NotSelected;
            DeselectInternal(false);
            List?.Deselect();
        }

        public void ApplyAction(Action<IDrawableListItem<T>> action) => List?.ApplyAction(action);
        void IDrawableListItem<T>.SelectInternal() => SelectInternal();
        void IDrawableListItem<T>.DeselectInternal() => DeselectInternal();

        public void SelectInternal(bool passThroughCall = true)
        {
            Scheduler.Add(box.Show);
            if (passThroughCall) List?.SelectInternal();
        }

        public void DeselectInternal(bool passThroughCall = true)
        {
            Scheduler.Add(box.Hide);
            if (passThroughCall) List?.DeselectInternal();
        }

        public Drawable GetDrawableListItem() => this;
        public RearrangeableListItem<IDrawableListRepresetedItem<T>> GetRearrangeableListItem() => this;
    }
}

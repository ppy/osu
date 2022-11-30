// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Screens.Edit.List
{
    public partial class DrawableMinimisableList<T> : RearrangeableListItem<IDrawableListRepresetedItem<T>>, IRearrangableDrawableListItem<T>
        where T : Drawable
    {
        public Action<T, int> SetItemDepth
        {
            get => List.SetItemDepth;
            set => List.SetItemDepth = value;
        }

        public Action OnDragAction
        {
            get => List.OnDragAction;
            set => List.OnDragAction = value;
        }

        public Action<Action<IDrawableListItem<T>>> ApplyAll
        {
            get => List.ApplyAll;
            set => List.ApplyAll = value;
        }

        public Func<T, LocalisableString> GetName
        {
            get => List.GetName;
            set => List.GetName = value;
        }

        public BindableBool Enabled { get; } = new BindableBool();
        public readonly DrawableList<T> List = new DrawableList<T>();
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

            box.Hide();
            Enabled.BindValueChanged(v => SetShown(v.NewValue), true);
            Select(false);
        }

        public void SetShown(bool value, bool setValue = false)
        {
            if (value) List.Show();
            else List.Hide();
            UpdateItem();
            if (setValue) Enabled.Value = value;
        }

        public void UpdateItem() => List.UpdateItem();

        public void Select(bool value)
        {
            Scheduler.Add(() =>
            {
                if (value) box.Show();
                else box.Hide();
            });
            List.Select(value);
        }

        public void ApplyAction(Action<IDrawableListItem<T>> action) => List.ApplyAction(action);

        public void SelectInternal(bool value) => List.SelectInternal(value);

        public Drawable GetDrawableListItem() => this;
        public RearrangeableListItem<IDrawableListRepresetedItem<T>> GetRearrangeableListItem() => this;
    }
}

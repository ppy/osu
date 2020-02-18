﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osuTK;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Bindables;
using osu.Framework.Allocation;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;
using osu.Game.Overlays.Comments;
using JetBrains.Annotations;

namespace osu.Game.Overlays
{
    public class OverlaySortTabControl<T> : CompositeDrawable, IHasCurrentValue<T>
    {
        private readonly BindableWithCurrent<T> current = new BindableWithCurrent<T>();

        public Bindable<T> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        public OverlaySortTabControl()
        {
            AutoSizeAxes = Axes.Both;
            AddInternal(new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(10, 0),
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Font = OsuFont.GetFont(size: 12),
                        Text = @"Sort by"
                    },
                    CreateControl().With(c =>
                    {
                        c.Anchor = Anchor.CentreLeft;
                        c.Origin = Anchor.CentreLeft;
                        c.Current = current;
                    })
                }
            });
        }

        [NotNull]
        protected virtual SortTabControl CreateControl() => new SortTabControl();

        protected class SortTabControl : OsuTabControl<T>
        {
            protected override Dropdown<T> CreateDropdown() => null;

            protected override TabItem<T> CreateTabItem(T value) => new SortTabItem(value);

            protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(5, 0),
            };

            public SortTabControl()
            {
                AutoSizeAxes = Axes.Both;
            }
        }

        protected class SortTabItem : TabItem<T>
        {
            public SortTabItem(T value)
                : base(value)
            {
                AutoSizeAxes = Axes.Both;
                Child = CreateTabButton(value);
            }

            [NotNull]
            protected virtual TabButton CreateTabButton(T value) => new TabButton(value)
            {
                Active = { BindTarget = Active }
            };

            protected override void OnActivated()
            {
            }

            protected override void OnDeactivated()
            {
            }
        }

        protected class TabButton : HeaderButton
        {
            public readonly BindableBool Active = new BindableBool();

            protected override Container<Drawable> Content => content;

            protected virtual Color4 ContentColour
            {
                set => text.Colour = value;
            }

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; }

            private readonly SpriteText text;
            private readonly FillFlowContainer content;

            public TabButton(T value)
            {
                base.Content.Add(content = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(3, 0),
                    Children = new Drawable[]
                    {
                        text = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.GetFont(size: 12),
                            Text = value.ToString()
                        }
                    }
                });
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                Active.BindValueChanged(_ => UpdateState(), true);
            }

            protected override bool OnHover(HoverEvent e)
            {
                UpdateState();
                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e) => UpdateState();

            protected virtual void UpdateState()
            {
                if (Active.Value || IsHovered)
                    ShowBackground();
                else
                    HideBackground();

                ContentColour = Active.Value && !IsHovered ? colourProvider.Light1 : Color4.White;

                text.Font = text.Font.With(weight: Active.Value ? FontWeight.Bold : FontWeight.Medium);
            }
        }
    }
}

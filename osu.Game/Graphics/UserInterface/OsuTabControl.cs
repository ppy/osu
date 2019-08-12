// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.MathUtils;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuTabControl<T> : TabControl<T>
    {
        private readonly Box strip;

        protected override Dropdown<T> CreateDropdown() => new OsuTabDropdown();

        protected override TabItem<T> CreateTabItem(T value) => new OsuTabItem(value);

        protected virtual float StripWidth() => TabContainer.Children.Sum(c => c.IsPresent ? c.DrawWidth + TabContainer.Spacing.X : 0) - TabContainer.Spacing.X;
        protected virtual float StripHeight() => 1;

        /// <summary>
        /// Whether entries should be automatically populated if <see cref="T"/> is an <see cref="Enum"/> type.
        /// </summary>
        protected virtual bool AddEnumEntriesAutomatically => true;

        private static bool isEnumType => typeof(T).IsEnum;

        public OsuTabControl()
        {
            TabContainer.Spacing = new Vector2(10f, 0f);

            AddInternal(strip = new Box
            {
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                Height = StripHeight(),
                Colour = Color4.White.Opacity(0),
            });

            if (isEnumType && AddEnumEntriesAutomatically)
                foreach (var val in (T[])Enum.GetValues(typeof(T)))
                    AddItem(val);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            if (accentColour == default)
                AccentColour = colours.Blue;
        }

        private Color4 accentColour;

        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                accentColour = value;
                if (Dropdown is IHasAccentColour dropdown)
                    dropdown.AccentColour = value;
                foreach (var i in TabContainer.Children.OfType<IHasAccentColour>())
                    i.AccentColour = value;
            }
        }

        public Color4 StripColour
        {
            get => strip.Colour;
            set => strip.Colour = value;
        }

        protected override TabFillFlowContainer CreateTabFlow() => new OsuTabFillFlowContainer
        {
            Direction = FillDirection.Full,
            RelativeSizeAxes = Axes.Both,
            Depth = -1,
            Masking = true
        };

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            // dont bother calculating if the strip is invisible
            if (strip.Colour.MaxAlpha > 0)
                strip.Width = Interpolation.ValueAt(MathHelper.Clamp(Clock.ElapsedFrameTime, 0, 1000), strip.Width, StripWidth(), 0, 500, Easing.OutQuint);
        }

        public class OsuTabItem : TabItem<T>, IHasAccentColour
        {
            protected readonly SpriteText Text;
            protected readonly Box Bar;

            private Color4 accentColour;

            public Color4 AccentColour
            {
                get => accentColour;
                set
                {
                    accentColour = value;
                    if (!Active.Value)
                        Text.Colour = value;
                }
            }

            private const float transition_length = 500;

            private void fadeActive()
            {
                Bar.FadeIn(transition_length, Easing.OutQuint);
                Text.FadeColour(Color4.White, transition_length, Easing.OutQuint);
            }

            private void fadeInactive()
            {
                Bar.FadeOut(transition_length, Easing.OutQuint);
                Text.FadeColour(AccentColour, transition_length, Easing.OutQuint);
            }

            protected override bool OnHover(HoverEvent e)
            {
                if (!Active.Value)
                    fadeActive();
                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                if (!Active.Value)
                    fadeInactive();
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                if (accentColour == default)
                    AccentColour = colours.Blue;
            }

            public OsuTabItem(T value)
                : base(value)
            {
                AutoSizeAxes = Axes.X;
                RelativeSizeAxes = Axes.Y;

                Children = new Drawable[]
                {
                    Text = new OsuSpriteText
                    {
                        Margin = new MarginPadding { Top = 5, Bottom = 5 },
                        Origin = Anchor.BottomLeft,
                        Anchor = Anchor.BottomLeft,
                        Text = (value as IHasDescription)?.Description ?? (value as Enum)?.GetDescription() ?? value.ToString(),
                        Font = OsuFont.GetFont(size: 14)
                    },
                    Bar = new Box
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 1,
                        Alpha = 0,
                        Colour = Color4.White,
                        Origin = Anchor.BottomLeft,
                        Anchor = Anchor.BottomLeft,
                    },
                    new HoverClickSounds()
                };

                Active.BindValueChanged(active => Text.Font = Text.Font.With(Typeface.Exo, weight: active.NewValue ? FontWeight.Bold : FontWeight.Medium), true);
            }

            protected override void OnActivated() => fadeActive();

            protected override void OnDeactivated() => fadeInactive();
        }

        // todo: this needs to go
        private class OsuTabDropdown : OsuDropdown<T>
        {
            public OsuTabDropdown()
            {
                RelativeSizeAxes = Axes.X;
            }

            protected override DropdownMenu CreateMenu() => new OsuTabDropdownMenu();

            protected override DropdownHeader CreateHeader() => new OsuTabDropdownHeader
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight
            };

            private class OsuTabDropdownMenu : OsuDropdownMenu
            {
                public OsuTabDropdownMenu()
                {
                    Anchor = Anchor.TopRight;
                    Origin = Anchor.TopRight;

                    BackgroundColour = Color4.Black.Opacity(0.7f);
                    MaxHeight = 400;
                }

                protected override DrawableDropdownMenuItem CreateDrawableDropdownMenuItem(MenuItem item) => new DrawableOsuTabDropdownMenuItem(item) { AccentColour = AccentColour };

                private class DrawableOsuTabDropdownMenuItem : DrawableOsuDropdownMenuItem
                {
                    public DrawableOsuTabDropdownMenuItem(MenuItem item)
                        : base(item)
                    {
                        ForegroundColourHover = Color4.Black;
                    }
                }
            }

            protected class OsuTabDropdownHeader : OsuDropdownHeader
            {
                public override Color4 AccentColour
                {
                    get => base.AccentColour;
                    set
                    {
                        base.AccentColour = value;
                        Foreground.Colour = value;
                    }
                }

                public OsuTabDropdownHeader()
                {
                    RelativeSizeAxes = Axes.None;
                    AutoSizeAxes = Axes.X;

                    BackgroundColour = Color4.Black.Opacity(0.5f);

                    Background.Height = 0.5f;
                    Background.CornerRadius = 5;
                    Background.Masking = true;

                    Foreground.RelativeSizeAxes = Axes.None;
                    Foreground.AutoSizeAxes = Axes.X;
                    Foreground.RelativeSizeAxes = Axes.Y;
                    Foreground.Margin = new MarginPadding(5);

                    Foreground.Children = new Drawable[]
                    {
                        new SpriteIcon
                        {
                            Icon = FontAwesome.Solid.EllipsisH,
                            Size = new Vector2(14),
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                        }
                    };

                    Padding = new MarginPadding { Left = 5, Right = 5 };
                }

                protected override bool OnHover(HoverEvent e)
                {
                    Foreground.Colour = BackgroundColour;
                    return base.OnHover(e);
                }

                protected override void OnHoverLost(HoverLostEvent e)
                {
                    Foreground.Colour = BackgroundColourHover;
                    base.OnHoverLost(e);
                }
            }
        }

        private class OsuTabFillFlowContainer : TabFillFlowContainer
        {
            protected override int Compare(Drawable x, Drawable y) => CompareReverseChildID(x, y);
        }
    }
}

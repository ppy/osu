// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
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

        private static bool isEnumType => typeof(T).IsEnum;

        public OsuTabControl()
        {
            TabContainer.Spacing = new Vector2(10f, 0f);

            Add(strip = new Box
            {
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                Height = StripHeight(),
                Colour = Color4.White.Opacity(0),
            });

            if (isEnumType)
                foreach (var val in (T[])Enum.GetValues(typeof(T)))
                    AddItem(val);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            if (accentColour == default(Color4))
                AccentColour = colours.Blue;
        }

        private Color4 accentColour;
        public Color4 AccentColour
        {
            get { return accentColour; }
            set
            {
                accentColour = value;
                var dropdown = Dropdown as IHasAccentColour;
                if (dropdown != null)
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
                get { return accentColour; }
                set
                {
                    accentColour = value;
                    if (!Active)
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

            protected override bool OnHover(InputState state)
            {
                if (!Active)
                    fadeActive();
                return true;
            }

            protected override void OnHoverLost(InputState state)
            {
                if (!Active)
                    fadeInactive();
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                if (accentColour == default(Color4))
                    AccentColour = colours.Blue;
            }

            public OsuTabItem(T value) : base(value)
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
                        TextSize = 14,
                        Font = @"Exo2.0-Bold", // Font should only turn bold when active?
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

                protected override DrawableMenuItem CreateDrawableMenuItem(MenuItem item) => new DrawableOsuTabDropdownMenuItem(item) { AccentColour = AccentColour };

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
                    get
                    {
                        return base.AccentColour;
                    }

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
                            Icon = FontAwesome.fa_ellipsis_h,
                            Size = new Vector2(14),
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                        }
                    };

                    Padding = new MarginPadding { Left = 5, Right = 5 };
                }

                protected override bool OnHover(InputState state)
                {
                    Foreground.Colour = BackgroundColour;
                    return base.OnHover(state);
                }

                protected override void OnHoverLost(InputState state)
                {
                    Foreground.Colour = BackgroundColourHover;
                    base.OnHoverLost(state);
                }
            }
        }

        private class OsuTabFillFlowContainer : TabFillFlowContainer
        {
            protected override int Compare(Drawable x, Drawable y) => CompareReverseChildID(x, y);
        }
    }
}

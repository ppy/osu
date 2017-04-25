// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuTabControl<T> : TabControl<T>
    {
        protected override Dropdown<T> CreateDropdown() => new OsuTabDropdown();

        protected override TabItem<T> CreateTabItem(T value) => new OsuTabItem(value);

        protected override bool InternalContains(Vector2 screenSpacePos) => base.InternalContains(screenSpacePos) || Dropdown.Contains(screenSpacePos);

        public OsuTabControl()
        {
            TabContainer.Spacing = new Vector2(10f, 0f);

            if (!typeof(T).IsEnum)
                throw new InvalidOperationException("OsuTabControl only supports enums as the generic type argument");

            foreach (var val in (T[])Enum.GetValues(typeof(T)))
                AddItem(val);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            if (accentColour == null)
                AccentColour = colours.Blue;
        }

        private Color4? accentColour;
        public Color4 AccentColour
        {
            get { return accentColour.GetValueOrDefault(); }
            set
            {
                accentColour = value;
                var dropDown = Dropdown as OsuTabDropdown;
                if (dropDown != null)
                    dropDown.AccentColour = value;
                foreach (var item in TabContainer.Children.OfType<OsuTabItem>())
                    item.AccentColour = value;
            }
        }

        private class OsuTabItem : TabItem<T>
        {
            private readonly SpriteText text;
            private readonly Box box;

            private Color4? accentColour;
            public Color4 AccentColour
            {
                get { return accentColour.GetValueOrDefault(); }
                set
                {
                    accentColour = value;
                    if (!Active)
                        text.Colour = value;
                }
            }

            public override bool Active
            {
                get { return base.Active; }
                set
                {
                    if (Active == value) return;

                    if (value)
                        fadeActive();
                    else
                        fadeInactive();
                    base.Active = value;
                }
            }

            private const float transition_length = 500;

            private void fadeActive()
            {
                box.FadeIn(transition_length, EasingTypes.OutQuint);
                text.FadeColour(Color4.White, transition_length, EasingTypes.OutQuint);
            }

            private void fadeInactive()
            {
                box.FadeOut(transition_length, EasingTypes.OutQuint);
                text.FadeColour(AccentColour, transition_length, EasingTypes.OutQuint);
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
                if (accentColour == null)
                    AccentColour = colours.Blue;
            }

            public OsuTabItem(T value) : base(value)
            {
                AutoSizeAxes = Axes.X;
                RelativeSizeAxes = Axes.Y;

                Children = new Drawable[]
                {
                    text = new OsuSpriteText
                    {
                        Margin = new MarginPadding { Top = 5, Bottom = 5 },
                        Origin = Anchor.BottomLeft,
                        Anchor = Anchor.BottomLeft,
                        Text = (value as Enum)?.GetDescription(),
                        TextSize = 14,
                        Font = @"Exo2.0-Bold", // Font should only turn bold when active?
                    },
                    box = new Box
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 1,
                        Alpha = 0,
                        Colour = Color4.White,
                        Origin = Anchor.BottomLeft,
                        Anchor = Anchor.BottomLeft,
                    }
                };
            }
        }

        private class OsuTabDropdown : OsuDropdown<T>
        {
            protected override DropdownHeader CreateHeader() => new OsuTabDropdownHeader
            {
                AccentColour = AccentColour,
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            };

            protected override DropdownMenuItem<T> CreateMenuItem(string text, T value)
            {
                var item = base.CreateMenuItem(text, value);
                item.ForegroundColourHover = Color4.Black;
                return item;
            }

            public OsuTabDropdown()
            {
                DropdownMenu.Anchor = Anchor.TopRight;
                DropdownMenu.Origin = Anchor.TopRight;

                RelativeSizeAxes = Axes.X;

                DropdownMenu.Background.Colour = Color4.Black.Opacity(0.7f);
                DropdownMenu.MaxHeight = 400;
            }

            protected class OsuTabDropdownHeader : OsuDropdownHeader
            {
                public override Color4 AccentColour
                {
                    get { return base.AccentColour; }
                    set
                    {
                        base.AccentColour = value;
                        Foreground.Colour = value;
                    }
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
                        new TextAwesome
                        {
                            Icon = FontAwesome.fa_ellipsis_h,
                            TextSize = 14,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                        }
                    };

                    Padding = new MarginPadding { Left = 5, Right = 5 };
                }
            }
        }
    }
}

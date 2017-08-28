// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Sprites;
using OpenTK;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuDropdown<T> : Dropdown<T>
    {
        public readonly Bindable<Color4?> AccentColour = new Bindable<Color4?>();

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            if (AccentColour.Value == null)
                AccentColour.Value = colours.PinkDarker;
        }

        protected override DropdownHeader CreateHeader()
        {
            var newHeader = new OsuDropdownHeader();
            newHeader.AccentColour.BindTo(AccentColour);

            return newHeader;
        }

        protected override DropdownMenu CreateMenu()
        {
            var newMenu = new OsuDropdownMenu();
            newMenu.AccentColour.BindTo(AccentColour);

            return newMenu;
        }

        #region OsuDropdownMenu
        protected class OsuDropdownMenu : DropdownMenu
        {
            // todo: this uses the same styling as OsuMenu. hopefully we can just use OsuMenu in the future with some refactoring
            public OsuDropdownMenu()
            {
                CornerRadius = 4;
                BackgroundColour = Color4.Black.Opacity(0.5f);
            }

            // todo: this uses the same styling as OsuMenu. hopefully we can just use OsuMenu in the future with some refactoring
            protected override void AnimateOpen() => this.FadeIn(300, Easing.OutQuint);
            protected override void AnimateClose() => this.FadeOut(300, Easing.OutQuint);

            // todo: this uses the same styling as OsuMenu. hopefully we can just use OsuMenu in the future with some refactoring
            protected override MarginPadding ItemFlowContainerPadding => new MarginPadding(5);

            // todo: this uses the same styling as OsuMenu. hopefully we can just use OsuMenu in the future with some refactoring
            protected override void UpdateMenuHeight()
            {
                var actualHeight = (RelativeSizeAxes & Axes.Y) > 0 ? 1 : ContentHeight;
                this.ResizeHeightTo(State == MenuState.Opened ? actualHeight : 0, 300, Easing.OutQuint);
            }

            public readonly Bindable<Color4?> AccentColour = new Bindable<Color4?>();


            protected override DrawableMenuItem CreateDrawableMenuItem(DropdownMenuItem<T> item)
            {
                var newItem = new DrawableOsuDropdownMenuItem(item);
                newItem.AccentColour.BindTo(AccentColour);

                return newItem;
            }

            #region DrawableOsuDropdownMenuItem
            protected class DrawableOsuDropdownMenuItem : DrawableDropdownMenuItem
            {
                public readonly Bindable<Color4?> AccentColour = new Bindable<Color4?>();

                private SpriteIcon chevron;
                protected OsuSpriteText Label;

                private Color4 nonAccentHoverColour;
                private Color4 nonAccentSelectedColour;

                public DrawableOsuDropdownMenuItem(DropdownMenuItem<T> item)
                    : base(item)
                {
                    Foreground.Padding = new MarginPadding(2);

                    Masking = true;
                    CornerRadius = 6;

                    AccentColour.ValueChanged += updateAccent;
                }

                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    BackgroundColour = Color4.Transparent;
                    nonAccentHoverColour = colours.PinkDarker;
                    nonAccentSelectedColour = Color4.Black.Opacity(0.5f);
                }

                private void updateAccent(Color4? newValue)
                {
                    BackgroundColourHover = newValue ?? nonAccentHoverColour;
                    BackgroundColourSelected = newValue ?? nonAccentSelectedColour;
                    UpdateBackgroundColour();
                    UpdateForegroundColour();
                }

                protected override void UpdateForegroundColour()
                {
                    base.UpdateForegroundColour();
                    chevron.Alpha = IsHovered ? 1 : 0;
                }

                protected override Drawable CreateContent()
                {
                    var container = new FillFlowContainer
                    {
                        Direction = FillDirection.Horizontal,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            chevron = new SpriteIcon
                            {
                                AlwaysPresent = true,
                                Icon = FontAwesome.fa_chevron_right,
                                Colour = Color4.Black,
                                Alpha = 0.5f,
                                Size = new Vector2(8),
                                Margin = new MarginPadding { Left = 3, Right = 3 },
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                            },
                            Label = new OsuSpriteText
                            {
                                Text = Item.Text,
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                            }
                        }
                    };

                    Item.Text.ValueChanged += newText => Label.Text = newText;
                    return container;
                }
            }
            #endregion
        }
        #endregion

        public class OsuDropdownHeader : DropdownHeader
        {
            protected readonly SpriteText Text;
            protected override string Label
            {
                get { return Text.Text; }
                set { Text.Text = value; }
            }

            protected readonly SpriteIcon Icon;

            public readonly Bindable<Color4?> AccentColour = new Bindable<Color4?>();

            public OsuDropdownHeader()
            {
                Foreground.Padding = new MarginPadding(4);

                AutoSizeAxes = Axes.None;
                Margin = new MarginPadding { Bottom = 4 };
                CornerRadius = 4;
                Height = 40;

                Foreground.Children = new Drawable[]
                {
                    Text = new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                    },
                    Icon = new SpriteIcon
                    {
                        Icon = FontAwesome.fa_chevron_down,
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Margin = new MarginPadding { Right = 4 },
                        Size = new Vector2(20),
                    }
                };

                AccentColour.ValueChanged += accentColourChanged;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                BackgroundColour = Color4.Black.Opacity(0.5f);
                BackgroundColourHover = AccentColour?.Value ?? colours.PinkDarker;
            }

            private void accentColourChanged(Color4? newValue)
            {
                BackgroundColourHover = newValue ?? Color4.White;
            }
        }
    }
}

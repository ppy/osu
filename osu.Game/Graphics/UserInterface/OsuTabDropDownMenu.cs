// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Graphics.UserInterface.Tab;
using osu.Framework.Input;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuTabDropDownMenu<T> : TabDropDownMenu<T>
    {
        private Color4? accentColour;
        public Color4 AccentColour
        {
            get { return accentColour.GetValueOrDefault(); }
            set
            {
                accentColour = value;
                ((OsuTabDropDownHeader)Header).AccentColour = value;
                foreach (var item in ItemList.OfType<OsuTabDropDownMenuItem<T>>())
                    item.AccentColour = value;
            }
        }

        protected override DropDownHeader CreateHeader() => new OsuTabDropDownHeader
        {
            AccentColour = AccentColour
        };

        protected override DropDownMenuItem<T> CreateDropDownItem(string key, T value) =>
            new OsuTabDropDownMenuItem<T>(key, value) { AccentColour = AccentColour };

        public OsuTabDropDownMenu()
        {
            MaxDropDownHeight = int.MaxValue;
            ContentContainer.CornerRadius = 4;
            ContentBackground.Colour = Color4.Black.Opacity(0.9f);
            ScrollContainer.ScrollDraggerVisible = false;
            DropDownItemsContainer.Padding = new MarginPadding { Left = 5, Bottom = 7, Right = 5, Top = 7 };
        }

        protected override void AnimateOpen()
        {
            ContentContainer.FadeIn(300, EasingTypes.OutQuint);
        }

        protected override void AnimateClose()
        {
            ContentContainer.FadeOut(300, EasingTypes.OutQuint);
        }

        protected override void UpdateContentHeight()
        {
            ContentContainer.ResizeTo(new Vector2(1, State == DropDownMenuState.Opened ? ContentHeight : 0), 300, EasingTypes.OutQuint);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            if (accentColour == null)
                AccentColour = colours.Blue;
        }

        public class OsuTabDropDownHeader : DropDownHeader
        {
            protected override string Label { get; set; }

            private Color4? accentColour;
            public Color4 AccentColour
            {
                get { return accentColour.GetValueOrDefault(); }
                set
                {
                    accentColour = value;
                    BackgroundColourHover = value;
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

            public OsuTabDropDownHeader()
            {
                RelativeSizeAxes = Axes.None;
                AutoSizeAxes = Axes.X;

                BackgroundColour = Color4.Black;

                Background.Height = 0.5f;
                Background.CornerRadius = 3;
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

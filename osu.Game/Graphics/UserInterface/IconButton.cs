// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Graphics.Containers;

namespace osu.Game.Graphics.UserInterface
{
    public class IconButton : OsuClickableContainer
    {
        public const float BUTTON_SIZE = 30;

        private Color4? flashColour;
        /// <summary>
        /// The colour that should be flashed when the <see cref="IconButton"/> is clicked.
        /// </summary>
        public Color4 FlashColour
        {
            get { return flashColour ?? Color4.White; }
            set { flashColour = value; }
        }

        private Color4? iconColour;
        /// <summary>
        /// The icon colour. This does not affect <see cref="IconButton.Colour"/>.
        /// </summary>
        public Color4 IconColour
        {
            get { return iconColour ?? Color4.White; }
            set
            {
                iconColour = value;
                icon.Colour = value;
            }
        }

        private Color4? iconHoverColour;
        /// <summary>
        /// The icon colour while the <see cref="IconButton"/> is hovered.
        /// </summary>
        public Color4 IconHoverColour
        {
            get { return iconHoverColour ?? IconColour; }
            set { iconHoverColour = value; }
        }

        private Color4? hoverColour;
        /// <summary>
        /// The background colour of the <see cref="IconButton"/> while it is hovered.
        /// </summary>
        public Color4 HoverColour
        {
            get { return hoverColour ?? Color4.White; }
            set
            {
                hoverColour = value;
                hover.Colour = value;
            }
        }

        /// <summary>
        /// The icon.
        /// </summary>
        public FontAwesome Icon
        {
            get { return icon.Icon; }
            set { icon.Icon = value; }
        }

        /// <summary>
        /// The icon scale. This does not affect <see cref="IconButton.Scale"/>.
        /// </summary>
        public Vector2 IconScale
        {
            get { return icon.Scale; }
            set { icon.Scale = value; }
        }

        /// <summary>
        /// The size of the <see cref="IconButton"/> while it is not being pressed.
        /// </summary>
        public Vector2 ButtonSize
        {
            get { return content.Size; }
            set { content.Size = value; }
        }

        private readonly Container content;
        private readonly SpriteIcon icon;
        private readonly Box hover;

        public IconButton()
        {
            AutoSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                content = new Container
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Size = new Vector2(BUTTON_SIZE),
                    CornerRadius = 5,
                    Masking = true,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Colour = Color4.Black.Opacity(0.04f),
                        Type = EdgeEffectType.Shadow,
                        Radius = 5,
                    },
                    Children = new Drawable[]
                    {
                        hover = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                        },
                        icon = new SpriteIcon
                        {
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            Size = new Vector2(18),
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            if (hoverColour == null)
                HoverColour = colours.Yellow.Opacity(0.6f);

            if (flashColour == null)
                FlashColour = colours.Yellow;

            Enabled.ValueChanged += enabled => this.FadeColour(enabled ? Color4.White : colours.Gray9, 200, Easing.OutQuint);
        }

        protected override bool OnHover(InputState state)
        {
            hover.FadeIn(500, Easing.OutQuint);
            icon.FadeColour(IconHoverColour, 500, Easing.OutQuint);
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            hover.FadeOut(500, Easing.OutQuint);
            icon.FadeColour(IconColour, 500, Easing.OutQuint);
            base.OnHoverLost(state);
        }

        protected override bool OnClick(InputState state)
        {
            hover.FlashColour(FlashColour, 800, Easing.OutQuint);
            return base.OnClick(state);
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            content.ScaleTo(0.75f, 2000, Easing.OutQuint);
            return base.OnMouseDown(state, args);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            content.ScaleTo(1, 1000, Easing.OutElastic);
            return base.OnMouseUp(state, args);
        }
    }
}

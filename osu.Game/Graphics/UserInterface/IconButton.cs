﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osuTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;

namespace osu.Game.Graphics.UserInterface
{
    public class IconButton : OsuAnimatedButton
    {
        public const float BUTTON_SIZE = 30;

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
            get => Content.Size;
            set
            {
                Content.RelativeSizeAxes = Axes.None;
                Content.Size = value;
            }
        }

        private readonly SpriteIcon icon;

        public IconButton()
        {
            AutoSizeAxes = Axes.Both;
            ButtonSize = new Vector2(BUTTON_SIZE);

            Add(icon = new SpriteIcon
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Size = new Vector2(18),
            });
        }

        protected override bool OnHover(HoverEvent e)
        {
            icon.FadeColour(IconHoverColour, 500, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            icon.FadeColour(IconColour, 500, Easing.OutQuint);
            base.OnHoverLost(e);
        }
    }
}

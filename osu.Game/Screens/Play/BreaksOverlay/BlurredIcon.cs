// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Screens.Play.BreaksOverlay
{
    public class BlurredIcon : BufferedContainer
    {
        private const int blur_sigma = 20;

        private readonly GlowIcon icon;

        public FontAwesome Icon
        {
            set { icon.Icon = value; }
            get { return icon.Icon; }
        }

        public override Vector2 Size
        {
            set
            {
                icon.Size = value;
                base.Size = value + new Vector2(blur_sigma * 2);
            }
            get { return icon.Size; }
        }

        public BlurredIcon()
        {
            RelativePositionAxes = Axes.X;
            BlurSigma = new Vector2(blur_sigma);
            Alpha = 0.6f;
            CacheDrawnFrameBuffer = true;
            Child = icon = new GlowIcon
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
            };
        }
    }
}

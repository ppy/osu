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
        private const int icon_size = 130;

        private readonly GlowingIcon icon;

        public FontAwesome Icon
        {
            set { icon.Icon = value; }
            get { return icon.Icon; }
        }

        public BlurredIcon()
        {
            Anchor = Anchor.CentreLeft;
            RelativePositionAxes = Axes.X;
            Size = new Vector2(icon_size * 1.7f);
            Masking = true;
            BlurSigma = new Vector2(20);
            Alpha = 0.6f;
            CacheDrawnFrameBuffer = true;
            Child = icon = new GlowingIcon
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Size = new Vector2(icon_size),
            };
        }
    }
}

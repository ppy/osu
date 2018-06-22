// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using OpenTK;

namespace osu.Game.Screens.Play.Break
{
    public class BlurredIcon : BufferedContainer
    {
        private readonly SpriteIcon icon;

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
                base.Size = value + BlurSigma * 2.5f;
                ForceRedraw();
            }
            get { return base.Size; }
        }

        public BlurredIcon()
        {
            RelativePositionAxes = Axes.X;
            CacheDrawnFrameBuffer = true;
            Child = icon = new SpriteIcon
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Shadow = false,
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.BlueLighter;
        }
    }
}

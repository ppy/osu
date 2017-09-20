// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;

namespace osu.Game.Screens.Play.BreaksOverlay
{
    public class GlowingIcon : Container
    {
        private readonly SpriteIcon icon;
        private readonly SpriteIcon glow;
        private readonly BufferedContainer glowContainer;

        public FontAwesome Icon
        {
            set { icon.Icon = glow.Icon = value; }
            get { return icon.Icon; }
        }

        public override Vector2 Size
        {
            set
            {
                glow.Size = icon.Size = value;
                glowContainer.Size = value * 1.5f;
            }
            get
            {
                return glow.Size;
            }
        }

        public GlowingIcon()
        {
            Anchor = Anchor.CentreLeft;
            RelativePositionAxes = Axes.X;
            AutoSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                glowContainer = new BufferedContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Masking = true,
                    BlurSigma = new Vector2(10),
                    CacheDrawnFrameBuffer = true,
                    Child = glow = new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Shadow = false,
                    },
                },
                icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Shadow = false,
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            glow.Colour = colours.Blue;
        }
    }
}

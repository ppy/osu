// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using OpenTK;
using osu.Framework.Allocation;

namespace osu.Game.Screens.Play.BreaksOverlay
{
    public class GlowIcon : Container
    {
        private const int blur_sigma = 5;

        private readonly SpriteIcon spriteIcon;
        private readonly SpriteIcon glowIcon;
        private readonly BufferedContainer glowContainer;

        public override Vector2 Size
        {
            set
            {
                spriteIcon.Size = glowIcon.Size = value;
                glowContainer.Size = value + new Vector2(blur_sigma * 2);
            }
            get { return spriteIcon.Size; }
        }

        public FontAwesome Icon
        {
            set { spriteIcon.Icon = glowIcon.Icon = value; }
            get { return spriteIcon.Icon; }
        }

        public GlowIcon()
        {
            RelativePositionAxes = Axes.X;
            AutoSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                glowContainer = new BufferedContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    BlurSigma = new Vector2(blur_sigma),
                    CacheDrawnFrameBuffer = true,
                    Child = glowIcon = new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Shadow = false,
                    },

                },
                spriteIcon = new SpriteIcon
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
            glowIcon.Colour = colours.BlueLight;
        }
    }
}

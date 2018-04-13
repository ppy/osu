// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using OpenTK;

namespace osu.Game.Screens.Play.Break
{
    public class GlowIcon : Container
    {
        private readonly SpriteIcon spriteIcon;
        private readonly BlurredIcon blurredIcon;

        public override Vector2 Size
        {
            get { return base.Size; }
            set
            {
                blurredIcon.Size = spriteIcon.Size = value;
                blurredIcon.ForceRedraw();
            }
        }

        public Vector2 BlurSigma
        {
            get { return blurredIcon.BlurSigma; }
            set { blurredIcon.BlurSigma = value; }
        }

        public FontAwesome Icon
        {
            get { return spriteIcon.Icon; }
            set { spriteIcon.Icon = blurredIcon.Icon = value; }
        }

        public GlowIcon()
        {
            RelativePositionAxes = Axes.X;
            AutoSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                blurredIcon = new BlurredIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
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
            blurredIcon.Colour = colours.Blue;
        }
    }
}

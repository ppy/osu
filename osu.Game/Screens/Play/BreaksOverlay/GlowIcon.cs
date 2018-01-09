// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
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
        private readonly SpriteIcon spriteIcon;
        private readonly BlurredIcon blurredIcon;

        public override Vector2 Size
        {
            set
            {
                blurredIcon.Size = spriteIcon.Size = value;
                blurredIcon.ForceRedraw();
            }
            get { return base.Size; }
        }

        public Vector2 BlurSigma
        {
            set { blurredIcon.BlurSigma = value; }
            get { return blurredIcon.BlurSigma; }
        }

        public FontAwesome Icon
        {
            set { spriteIcon.Icon = blurredIcon.Icon = value; }
            get { return spriteIcon.Icon; }
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

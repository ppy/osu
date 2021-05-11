// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Screens.Play.Break
{
    public class GlowIcon : Container
    {
        private readonly SpriteIcon spriteIcon;
        private readonly BlurredIcon blurredIcon;

        public override Vector2 Size
        {
            get => base.Size;
            set
            {
                blurredIcon.Size = spriteIcon.Size = value;
                blurredIcon.ForceRedraw();
            }
        }

        public Vector2 BlurSigma
        {
            get => blurredIcon.BlurSigma;
            set => blurredIcon.BlurSigma = value;
        }

        public IconUsage Icon
        {
            get => spriteIcon.Icon;
            set => spriteIcon.Icon = blurredIcon.Icon = value;
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

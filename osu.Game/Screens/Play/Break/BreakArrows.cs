// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Screens.Play.Break
{
    public partial class BreakArrows : CompositeDrawable
    {
        private const int glow_icon_size = 60;
        private const int glow_icon_blur_sigma = 10;
        private const float glow_icon_final_offset = 0.22f;
        private const float glow_icon_offscreen_offset = 0.6f;

        private const int blurred_icon_blur_sigma = 20;
        private const int blurred_icon_size = 130;
        private const float blurred_icon_final_offset = 0.38f;
        private const float blurred_icon_offscreen_offset = 0.7f;

        private readonly GlowIcon leftGlowIcon;
        private readonly GlowIcon rightGlowIcon;

        private readonly BlurredIcon leftBlurredIcon;
        private readonly BlurredIcon rightBlurredIcon;

        public BreakArrows()
        {
            RelativeSizeAxes = Axes.Both;
            InternalChildren = new Drawable[]
            {
                new ParallaxContainer
                {
                    ParallaxAmount = -0.01f,
                    Children = new Drawable[]
                    {
                        leftGlowIcon = new GlowIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.CentreRight,
                            X = -glow_icon_offscreen_offset,
                            Icon = FontAwesome.Solid.ChevronRight,
                            BlurSigma = new Vector2(glow_icon_blur_sigma),
                            Size = new Vector2(glow_icon_size),
                        },
                        rightGlowIcon = new GlowIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.CentreLeft,
                            X = glow_icon_offscreen_offset,
                            Icon = FontAwesome.Solid.ChevronLeft,
                            BlurSigma = new Vector2(glow_icon_blur_sigma),
                            Size = new Vector2(glow_icon_size),
                        },
                    }
                },
                new ParallaxContainer
                {
                    ParallaxAmount = -0.02f,
                    Children = new Drawable[]
                    {
                        leftBlurredIcon = new BlurredIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.CentreRight,
                            Alpha = 0.7f,
                            X = -blurred_icon_offscreen_offset,
                            Icon = FontAwesome.Solid.ChevronRight,
                            BlurSigma = new Vector2(blurred_icon_blur_sigma),
                            Size = new Vector2(blurred_icon_size),
                        },
                        rightBlurredIcon = new BlurredIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.CentreLeft,
                            Alpha = 0.7f,
                            X = blurred_icon_offscreen_offset,
                            Icon = FontAwesome.Solid.ChevronLeft,
                            BlurSigma = new Vector2(blurred_icon_blur_sigma),
                            Size = new Vector2(blurred_icon_size),
                        },
                    }
                }
            };
        }

        public void Show(double duration)
        {
            leftGlowIcon.MoveToX(-glow_icon_final_offset, duration, Easing.OutQuint);
            rightGlowIcon.MoveToX(glow_icon_final_offset, duration, Easing.OutQuint);

            leftBlurredIcon.MoveToX(-blurred_icon_final_offset, duration, Easing.OutQuint);
            rightBlurredIcon.MoveToX(blurred_icon_final_offset, duration, Easing.OutQuint);
        }

        public void Hide(double duration)
        {
            leftGlowIcon.MoveToX(-glow_icon_offscreen_offset, duration, Easing.OutQuint);
            rightGlowIcon.MoveToX(glow_icon_offscreen_offset, duration, Easing.OutQuint);

            leftBlurredIcon.MoveToX(-blurred_icon_offscreen_offset, duration, Easing.OutQuint);
            rightBlurredIcon.MoveToX(blurred_icon_offscreen_offset, duration, Easing.OutQuint);
        }
    }
}

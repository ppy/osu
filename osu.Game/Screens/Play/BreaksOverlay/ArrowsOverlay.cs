// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using OpenTK;

namespace osu.Game.Screens.Play.BreaksOverlay
{
    public class ArrowsOverlay : Container
    {
        private const int glowing_size = 60;
        private const float glowing_final_offset = 0.25f;
        private const float glowing_offscreen_offset = 0.6f;

        private const int blurred_size = 130;
        private const float blurred_final_offset = 0.35f;
        private const float blurred_offscreen_offset = 0.7f;

        private readonly GlowIcon leftGlowIcon;
        private readonly GlowIcon rightGlowIcon;

        private readonly BlurredIcon leftBlurredIcon;
        private readonly BlurredIcon rightBlurredIcon;

        public ArrowsOverlay()
        {
            RelativeSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                leftGlowIcon = new GlowIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.CentreRight,
                    X = - glowing_offscreen_offset,
                    Icon = Graphics.FontAwesome.fa_chevron_right,
                    Size = new Vector2(glowing_size),
                },
                rightGlowIcon = new GlowIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.CentreLeft,
                    X = glowing_offscreen_offset,
                    Icon = Graphics.FontAwesome.fa_chevron_left,
                    Size = new Vector2(glowing_size),
                },
                leftBlurredIcon = new BlurredIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.CentreRight,
                    X = - blurred_offscreen_offset,
                    Icon = Graphics.FontAwesome.fa_chevron_right,
                    Size = new Vector2(blurred_size),
                },
                rightBlurredIcon = new BlurredIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.CentreLeft,
                    X = blurred_offscreen_offset,
                    Icon = Graphics.FontAwesome.fa_chevron_left,
                    Size = new Vector2(blurred_size),
                },
            };
        }

        public void Show(double fadeDuration)
        {
            leftGlowIcon.MoveToX(-glowing_final_offset, fadeDuration, Easing.OutQuint);
            rightGlowIcon.MoveToX(glowing_final_offset, fadeDuration, Easing.OutQuint);

            leftBlurredIcon.MoveToX(-blurred_final_offset, fadeDuration, Easing.OutQuint);
            rightBlurredIcon.MoveToX(blurred_final_offset, fadeDuration, Easing.OutQuint);
        }

        public void Hide(double fadeDuration)
        {
            leftGlowIcon.MoveToX(-glowing_offscreen_offset, fadeDuration, Easing.OutQuint);
            rightGlowIcon.MoveToX(glowing_offscreen_offset, fadeDuration, Easing.OutQuint);

            leftBlurredIcon.MoveToX(-blurred_offscreen_offset, fadeDuration, Easing.OutQuint);
            rightBlurredIcon.MoveToX(blurred_offscreen_offset, fadeDuration, Easing.OutQuint);
        }
    }
}

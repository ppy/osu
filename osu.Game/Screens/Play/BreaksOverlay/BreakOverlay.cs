// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Timing;
using osu.Game.Beatmaps.Timing;
using System.Collections.Generic;

namespace osu.Game.Screens.Play.BreaksOverlay
{
    public class BreakOverlay : Container
    {
        private const double fade_duration = BreakPeriod.MIN_BREAK_DURATION / 2;
        private const float remaining_time_container_max_size = 0.35f;
        private const int vertical_margin = 25;
        private const float glowing_x_offset = 0.13f;
        private const float glowing_x_final = 0.22f;
        private const float blurred_x_offset = 0.2f;

        public List<BreakPeriod> Breaks;

        public override IFrameBasedClock Clock
        {
            set
            {
                base.Clock = remainingTimeCounter.Clock = value;
            }
            get { return base.Clock; }
        }

        private readonly bool letterboxing;
        private readonly LetterboxOverlay letterboxOverlay;
        private readonly Container remainingTimeBox;
        private readonly RemainingTimeCounter remainingTimeCounter;
        private readonly InfoContainer info;

        private readonly GlowingIcon leftGlowingIcon;
        private readonly GlowingIcon rightGlowingIcon;

        private readonly BlurredIcon leftBlurredIcon;
        private readonly BlurredIcon rightBlurredIcon;

        public BreakOverlay(bool letterboxing)
        {
            this.letterboxing = letterboxing;

            RelativeSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                letterboxOverlay = new LetterboxOverlay
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                remainingTimeBox = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    Size = new Vector2(0, 8),
                    CornerRadius = 4,
                    Masking = true,
                    Child = new Box { RelativeSizeAxes = Axes.Both }
                },
                remainingTimeCounter = new RemainingTimeCounter
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.BottomCentre,
                    Margin = new MarginPadding { Bottom = vertical_margin },
                },
                info = new InfoContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.TopCentre,
                    Margin = new MarginPadding { Top = vertical_margin },
                },
                leftGlowingIcon = new GlowingIcon
                {
                    Origin = Anchor.CentreRight,
                    Icon = Graphics.FontAwesome.fa_chevron_left,
                    Size = new Vector2(60),
                    X = 1 + glowing_x_offset,
                },
                rightGlowingIcon = new GlowingIcon
                {
                    Origin = Anchor.CentreLeft,
                    Icon = Graphics.FontAwesome.fa_chevron_right,
                    Size = new Vector2(60),
                    X = -glowing_x_offset,
                },
                leftBlurredIcon = new BlurredIcon
                {
                    Origin = Anchor.CentreRight,
                    Icon = Graphics.FontAwesome.fa_chevron_left,
                    X = 1 + blurred_x_offset,
                },
                rightBlurredIcon = new BlurredIcon
                {
                    Origin = Anchor.CentreLeft,
                    Icon = Graphics.FontAwesome.fa_chevron_right,
                    X = -blurred_x_offset,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            InitializeBreaks();
        }

        public void InitializeBreaks()
        {
            if (Breaks != null)
            {
                foreach (var b in Breaks)
                {
                    if (b.HasEffect)
                    {
                        using (BeginAbsoluteSequence(b.StartTime, true))
                        {
                            onBreakIn(b);

                            using (BeginDelayedSequence(b.Duration, true))
                                onBreakOut();
                        }
                    }
                }
            }
        }

        private void onBreakIn(BreakPeriod b)
        {
            if (letterboxing)
                letterboxOverlay.FadeIn(fade_duration);

            remainingTimeBox
                .ResizeWidthTo(remaining_time_container_max_size, fade_duration, Easing.OutQuint)
                .Then()
                .ResizeWidthTo(0, b.Duration);

            Scheduler.AddDelayed(() => remainingTimeCounter.StartCounting(b.EndTime), b.StartTime);
            remainingTimeCounter.FadeIn(fade_duration);

            info.FadeIn(fade_duration);

            leftGlowingIcon.MoveToX(1 - glowing_x_final, fade_duration, Easing.OutQuint);
            rightGlowingIcon.MoveToX(glowing_x_final, fade_duration, Easing.OutQuint);

            leftBlurredIcon.MoveToX(1, fade_duration, Easing.OutQuint);
            rightBlurredIcon.MoveToX(0, fade_duration, Easing.OutQuint);
        }

        private void onBreakOut()
        {
            if (letterboxing)
                letterboxOverlay.FadeOut(fade_duration);

            remainingTimeCounter.FadeOut(fade_duration);
            info.FadeOut(fade_duration);

            leftGlowingIcon.MoveToX(1 + glowing_x_offset, fade_duration, Easing.OutQuint);
            rightGlowingIcon.MoveToX(-glowing_x_offset, fade_duration, Easing.OutQuint);

            leftBlurredIcon.MoveToX(1 + blurred_x_offset, fade_duration, Easing.OutQuint);
            rightBlurredIcon.MoveToX(-blurred_x_offset, fade_duration, Easing.OutQuint);
        }
    }
}

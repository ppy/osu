// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.Timing;
using System.Collections.Generic;

namespace osu.Game.Screens.Play.BreaksOverlay
{
    public class BreakOverlay : Container
    {
        private const double fade_duration = BreakPeriod.MIN_BREAK_DURATION / 2;
        private const float remaining_time_container_max_size = 0.3f;
        private const int vertical_margin = 25;

        public List<BreakPeriod> Breaks;

        private readonly bool letterboxing;
        private readonly LetterboxOverlay letterboxOverlay;
        private readonly Container remainingTimeBox;
        private readonly RemainingTimeCounter remainingTimeCounter;
        private readonly InfoContainer info;
        private readonly ArrowsOverlay arrowsOverlay;

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
                arrowsOverlay = new ArrowsOverlay
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
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
            if (Breaks == null)
                return;

            foreach (var b in Breaks)
            {
                if (!b.HasEffect)
                    continue;

                using (BeginAbsoluteSequence(b.StartTime, true))
                {
                    onBreakIn(b);

                    using (BeginDelayedSequence(b.Duration - fade_duration, true))
                        onBreakOut();
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
                .ResizeWidthTo(0, b.Duration - fade_duration);

            Scheduler.AddDelayed(() => remainingTimeCounter.StartCounting(b.EndTime), b.StartTime);
            remainingTimeCounter.FadeIn(fade_duration);

            info.FadeIn(fade_duration);
            arrowsOverlay.Show(fade_duration);
        }

        private void onBreakOut()
        {
            if (letterboxing)
                letterboxOverlay.FadeOut(fade_duration);

            remainingTimeCounter.FadeOut(fade_duration);
            info.FadeOut(fade_duration);
            arrowsOverlay.Hide(fade_duration);
        }
    }
}

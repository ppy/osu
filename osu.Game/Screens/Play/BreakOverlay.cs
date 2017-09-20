// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using System.Collections.Generic;
using osu.Game.Beatmaps.Timing;
using OpenTK;
using osu.Framework.Graphics.Shapes;
using OpenTK.Graphics;
using osu.Game.Graphics.Sprites;
using System;
using osu.Framework.Timing;

namespace osu.Game.Screens.Play
{
    public class BreakOverlay : Container
    {
        private const double fade_duration = BreakPeriod.MIN_BREAK_DURATION / 2;
        private const int remaining_time_container_max_size = 500;

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

        public BreakOverlay(bool letterboxing)
        {
            this.letterboxing = letterboxing;

            RelativeSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                letterboxOverlay = new LetterboxOverlay(),
                remainingTimeBox = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(0, 8),
                    CornerRadius = 4,
                    Masking = true,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.White,
                    }
                },
                remainingTimeCounter = new RemainingTimeCounter
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.BottomCentre,
                    Margin = new MarginPadding { Bottom = 25 },
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

            Scheduler.AddDelayed(() => remainingTimeCounter.StartCounting(b.EndTime), b.StartTime - Clock.CurrentTime);
            remainingTimeCounter.FadeIn(fade_duration);
        }

        private void onBreakOut()
        {
            if (letterboxing)
                letterboxOverlay.FadeOut(fade_duration);

            remainingTimeCounter.FadeOut(fade_duration);
        }

        private class RemainingTimeCounter : Container
        {
            private readonly OsuSpriteText counter;

            private int? previousSecond;

            private double remainingTime;

            private bool isCounting;

            public RemainingTimeCounter()
            {
                AutoSizeAxes = Axes.Both;
                Alpha = 0;
                Child = counter = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    TextSize = 35,
                    Font = "Venera",
                };
            }

            public void StartCounting(double remainingTime)
            {
                this.remainingTime = remainingTime;
                isCounting = true;
            }

            protected override void Update()
            {
                base.Update();

                if (isCounting)
                {
                    var currentTime = Clock.CurrentTime;
                    if (currentTime < remainingTime)
                    {
                        int currentSecond = (int)Math.Floor((remainingTime - Clock.CurrentTime) / 1000.0) + 1;
                        if (currentSecond != previousSecond)
                        {
                            counter.Text = currentSecond.ToString();
                            previousSecond = currentSecond;
                        }
                    }
                    else isCounting = false;
                }
            }
        }
    }
}

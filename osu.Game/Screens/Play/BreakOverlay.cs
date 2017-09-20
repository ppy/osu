// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using System.Collections.Generic;
using osu.Game.Beatmaps.Timing;
using OpenTK;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Sprites;
using System;
using osu.Framework.Timing;
using osu.Framework.Allocation;
using osu.Game.Graphics;

namespace osu.Game.Screens.Play
{
    public class BreakOverlay : Container
    {
        private const double fade_duration = BreakPeriod.MIN_BREAK_DURATION / 2;
        private const int remaining_time_container_max_size = 450;
        private const int element_margin = 25;

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
                    Child = new Box { RelativeSizeAxes = Axes.Both }
                },
                remainingTimeCounter = new RemainingTimeCounter
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.BottomCentre,
                    Margin = new MarginPadding { Bottom = element_margin },
                },
                info = new InfoContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.TopCentre,
                    Margin = new MarginPadding { Top = element_margin },
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

            info.FadeIn(fade_duration);
        }

        private void onBreakOut()
        {
            if (letterboxing)
                letterboxOverlay.FadeOut(fade_duration);

            remainingTimeCounter.FadeOut(fade_duration);
            info.FadeOut(fade_duration);
        }

        private class InfoContainer : FillFlowContainer
        {
            public InfoContainer()
            {
                AutoSizeAxes = Axes.Both;
                Alpha = 0;
                Direction = FillDirection.Vertical;
                Spacing = new Vector2(5);
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Text = @"current progress".ToUpper(),
                        TextSize = 15,
                        Font = "Exo2.0-Black",
                    },
                    new FillFlowContainer<InfoLine>
                    {
                        AutoSizeAxes = Axes.Both,
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                        Direction = FillDirection.Vertical,
                        Children = new []
                        {
                            new InfoLine(@"Accuracy", @"88.54%"),
                            new InfoLine(@"Rank", @"#6584"),
                            new InfoLine(@"Grade", @"A"),
                        },
                    }
                };
            }

            private class InfoLine : Container
            {
                private const int margin = 2;

                private readonly OsuSpriteText text;
                private readonly OsuSpriteText valueText;

                public InfoLine(string name, string value)
                {
                    AutoSizeAxes = Axes.Y;
                    Children = new Drawable[]
                    {
                        text = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.CentreRight,
                            Text = name,
                            TextSize = 17,
                            Margin = new MarginPadding { Right = margin }
                        },
                        valueText = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.CentreLeft,
                            Text = value,
                            TextSize = 17,
                            Font = "Exo2.0-Bold",
                            Margin = new MarginPadding { Left = margin }
                        }
                    };
                }

                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    text.Colour = colours.Yellow;
                    valueText.Colour = colours.YellowLight;
                }
            }
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
                    TextSize = 33,
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

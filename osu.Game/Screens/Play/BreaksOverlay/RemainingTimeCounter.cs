// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Framework.Graphics;
using System;
using osu.Game.Beatmaps.Timing;

namespace osu.Game.Screens.Play.BreaksOverlay
{
    public class RemainingTimeCounter : VisibilityContainer
    {
        private const double fade_duration = BreakPeriod.MIN_BREAK_DURATION / 2;

        private readonly OsuSpriteText counter;

        private int? previousSecond;

        private double endTime;

        private bool isCounting;

        public RemainingTimeCounter()
        {
            AutoSizeAxes = Axes.Both;
            Child = counter = new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                TextSize = 33,
                Font = "Venera",
            };
        }

        public void StartCounting(double endTime)
        {
            this.endTime = endTime;
            isCounting = true;
        }

        protected override void Update()
        {
            base.Update();

            if (isCounting)
            {
                var currentTime = Clock.CurrentTime;
                if (currentTime < endTime)
                {
                    int currentSecond = (int)Math.Ceiling((endTime - Clock.CurrentTime) / 1000.0);
                    if (currentSecond != previousSecond)
                    {
                        counter.Text = currentSecond.ToString();
                        previousSecond = currentSecond;
                    }
                }
                else isCounting = false;
            }
        }

        protected override void PopIn() => this.FadeIn(fade_duration);
        protected override void PopOut() => this.FadeOut(fade_duration);
    }
}

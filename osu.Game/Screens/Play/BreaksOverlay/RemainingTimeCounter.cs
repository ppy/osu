// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Framework.Graphics;
using System;
using osu.Game.Beatmaps.Timing;

namespace osu.Game.Screens.Play.BreaksOverlay
{
    public class RemainingTimeCounter : Container
    {
        private const double fade_duration = BreakPeriod.MIN_BREAK_DURATION / 2;

        private readonly OsuSpriteText counter;

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

            Alpha = 0;
        }

        public void CountTo(double duration)
        {
            double offset = 0;

            while (duration > 0)
            {
                int seconds = (int)Math.Ceiling(duration / 1000);
                counter.Delay(offset).TransformTextTo(seconds.ToString());

                double localOffset = duration - (seconds - 1) * 1000 + 1; // +1 because we want the duration to be the next second when ceiled

                offset += localOffset;
                duration -= localOffset;
            }
        }

        public override void Show() => this.FadeIn(fade_duration);
        public override void Hide() => this.FadeOut(fade_duration);
    }
}

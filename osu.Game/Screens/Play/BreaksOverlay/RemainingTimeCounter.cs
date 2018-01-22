// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Graphics.Sprites;
using osu.Framework.Graphics;
using System;
using osu.Game.Beatmaps.Timing;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Screens.Play.BreaksOverlay
{
    public class RemainingTimeCounter : Counter
    {
        private const double fade_duration = BreakPeriod.MIN_BREAK_DURATION / 2;

        private readonly OsuSpriteText counter;

        public RemainingTimeCounter()
        {
            AutoSizeAxes = Axes.Both;
            InternalChild = counter = new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                TextSize = 33,
                Font = "Venera",
            };

            Alpha = 0;
        }

        protected override void OnCountChanged(double count) => counter.Text = ((int)Math.Ceiling(count / 1000)).ToString();

        public override void Show() => this.FadeIn(fade_duration);
        public override void Hide() => this.FadeOut(fade_duration);
    }
}

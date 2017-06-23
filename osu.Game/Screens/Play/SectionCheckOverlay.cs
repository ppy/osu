// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using osu.Game.Graphics.Sprites;
using System.Collections.Generic;

namespace osu.Game.Screens.Play
{
    public class SectionCheckOverlay : Container
    {
        private List<BreakPeriod> breaks = new List<BreakPeriod>();

        private int currentBreakIndex;
        private double imageAppearTime;
        private bool isBreak;
        private bool imageHasBeenShown;

        private OsuSpriteText tempText;

        private IClock audioClock;
        public IClock AudioClock { set { audioClock = value; } }

        public List<BreakPeriod> Breaks { set { breaks = value; } }

        public SectionCheckOverlay()
        {
            AutoSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Add(tempText = new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = @"BREAK",
                TextSize = 50,
                Alpha = 0,
                AlwaysPresent = true,
            });
        }

        protected override void Update()
        {
            if (breaks.Count == 0) return;

            if (currentBreakIndex == breaks.Count) return;

            double currentTime = audioClock?.CurrentTime ?? Time.Current;

            if (!isBreak)
            {
                if (currentTime > breaks[currentBreakIndex].StartTime)
                {
                    isBreak = true;
                    imageHasBeenShown = false;
                    imageAppearTime = currentTime + (breaks[currentBreakIndex].EndTime - breaks[currentBreakIndex].StartTime) / 2;
                    return;
                }
            }
            else
            {
                if (currentTime > imageAppearTime && !imageHasBeenShown)
                {
                    if (breaks[currentBreakIndex].HasEffect)
                    {
                        // Show image depends on HP
                        tempText.FadeTo(1, 100);
                        Delay(1000);
                        Schedule(() => tempText.FadeTo(0, 100));

                        imageHasBeenShown = true;
                    }
                    // Increase bg dim
                    // Hide overlay
                }
                // Exit from break
                if (currentTime > breaks[currentBreakIndex].EndTime)
                {
                    currentBreakIndex++;
                    isBreak = false;
                }
            }
        }
    }
}

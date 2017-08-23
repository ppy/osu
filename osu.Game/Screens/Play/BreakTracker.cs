// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using System;
using System.Collections.Generic;

namespace osu.Game.Screens.Play
{
    public class BreakTracker : Container
    {
        public Action OnBreakIn;
        public Action OnBreakOut;

        private readonly BreakOverlay breakOverlay;

        private readonly List<BreakPeriod> breaks;

        private readonly int currentBreakIndex;

        private IClock audioClock;
        public IClock AudioClock { set { breakOverlay.AudioClock = audioClock = value; } }

        public BreakTracker(WorkingBeatmap beatmap)
        {
            this.breaks = beatmap.Beatmap.Breaks;

            RelativeSizeAxes = Axes.Both;
            Child = breakOverlay = new BreakOverlay(beatmap.BeatmapInfo.LetterboxInBreaks);
        }

        protected override void Update()
        {
            if (currentBreakIndex == breaks.Count) return;

            double currentTime = audioClock?.CurrentTime ?? Time.Current;
        }
    }
}

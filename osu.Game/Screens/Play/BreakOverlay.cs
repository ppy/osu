// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Beatmaps.Timing;
using System;
using System.Collections.Generic;

namespace osu.Game.Screens.Play
{
    public class BreakOverlay : Container, IStateful<Visibility>
    {
        public Action OnBreakIn;
        public Action OnBreakOut;

        private readonly bool letterboxing;
        private readonly List<BreakPeriod> breaks;

        private IClock audioClock;
        public IClock AudioClock { set { audioClock = value; } }

        private Visibility state;
        public Visibility State
        {
            get
            {
                return state;
            }
            set
            {
                state = value;

                switch (state)
                {
                    case Visibility.Visible:
                        OnBreakIn?.Invoke();
                        break;
                    case Visibility.Hidden:
                        OnBreakOut?.Invoke();
                        break;
                }
            }
        }

        public BreakOverlay(List<BreakPeriod> breaks, bool letterboxing)
        {
            this.breaks = breaks;
            this.letterboxing = letterboxing;
        }

        protected override void Update()
        {
            double currentTime = audioClock?.CurrentTime ?? Time.Current;
        }
    }
}

// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using System.Collections.Generic;
using osu.Game.Beatmaps.Timing;

namespace osu.Game.Screens.Play
{
    public class BreakOverlay : VisibilityContainer
    {
        private const double fade_duration = BreakPeriod.MIN_BREAK_DURATION / 2;

        public List<BreakPeriod> Breaks;

        private readonly bool letterboxing;
        private readonly LetterboxOverlay letterboxOverlay;

        public BreakOverlay(bool letterboxing)
        {
            this.letterboxing = letterboxing;

            RelativeSizeAxes = Axes.Both;
            Child = letterboxOverlay = new LetterboxOverlay();
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
                            Show();

                            using (BeginDelayedSequence(b.Duration, true))
                                Hide();
                        }
                    }
                }
            }
        }

        protected override void PopIn()
        {
            if (letterboxing) letterboxOverlay.FadeIn(fade_duration);
        }

        protected override void PopOut()
        {
            if (letterboxing) letterboxOverlay.FadeOut(fade_duration);
        }
    }
}

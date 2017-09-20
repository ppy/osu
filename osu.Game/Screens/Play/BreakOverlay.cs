// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using System.Collections.Generic;
using osu.Game.Beatmaps.Timing;
using OpenTK;
using osu.Framework.Graphics.Shapes;
using OpenTK.Graphics;

namespace osu.Game.Screens.Play
{
    public class BreakOverlay : Container
    {
        private const double fade_duration = BreakPeriod.MIN_BREAK_DURATION / 2;
        private const int remaining_time_container_max_size = 500;

        public List<BreakPeriod> Breaks;

        private readonly bool letterboxing;
        private readonly LetterboxOverlay letterboxOverlay;
        private readonly Container remainingTimeContainer;

        public BreakOverlay(bool letterboxing)
        {
            this.letterboxing = letterboxing;

            RelativeSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                letterboxOverlay = new LetterboxOverlay(),
                remainingTimeContainer = new Container
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

            remainingTimeContainer.ResizeWidthTo(remaining_time_container_max_size, fade_duration, Easing.OutQuint).Then().ResizeWidthTo(0, b.Duration);
        }

        private void onBreakOut()
        {
            if (letterboxing) letterboxOverlay.FadeOut(fade_duration);
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.Timing;
using osu.Game.Lists;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Screens.Play
{
    public class BreakTracker : Component
    {
        private readonly ScoreProcessor scoreProcessor;

        private readonly double gameplayStartTime;

        /// <summary>
        /// Whether the gameplay is currently in a break.
        /// </summary>
        public IBindable<bool> IsBreakTime => isBreakTime;

        private readonly BindableBool isBreakTime = new BindableBool();

        private readonly IntervalList<double> breakIntervals = new IntervalList<double>();

        public IReadOnlyList<BreakPeriod> Breaks
        {
            set
            {
                isBreakTime.Value = false;
                breakIntervals.Clear();

                foreach (var b in value)
                {
                    if (!b.HasEffect)
                        continue;

                    breakIntervals.Add(b.StartTime, b.EndTime - BreakOverlay.BREAK_FADE_DURATION);
                }
            }
        }

        public BreakTracker(double gameplayStartTime = 0, ScoreProcessor scoreProcessor = null)
        {
            this.gameplayStartTime = gameplayStartTime;
            this.scoreProcessor = scoreProcessor;
        }

        protected override void Update()
        {
            base.Update();

            var time = Clock.CurrentTime;

            isBreakTime.Value = breakIntervals.IsInAnyInterval(time)
                                || time < gameplayStartTime
                                || scoreProcessor?.HasCompleted == true;
        }
    }
}

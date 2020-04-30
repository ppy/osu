// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.Timing;
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

        protected int CurrentBreakIndex;

        private readonly BindableBool isBreakTime = new BindableBool();

        private IReadOnlyList<BreakPeriod> breaks;

        public IReadOnlyList<BreakPeriod> Breaks
        {
            get => breaks;
            set
            {
                breaks = value;

                // reset index in case the new breaks list is smaller than last one
                isBreakTime.Value = false;
                CurrentBreakIndex = 0;
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

            isBreakTime.Value = getCurrentBreak()?.HasEffect == true
                                || Clock.CurrentTime < gameplayStartTime
                                || scoreProcessor?.HasCompleted.Value == true;
        }

        private BreakPeriod getCurrentBreak()
        {
            if (breaks?.Count > 0)
            {
                var time = Clock.CurrentTime;

                if (time > breaks[CurrentBreakIndex].EndTime)
                {
                    while (time > breaks[CurrentBreakIndex].EndTime && CurrentBreakIndex < breaks.Count - 1)
                        CurrentBreakIndex++;
                }
                else
                {
                    while (time < breaks[CurrentBreakIndex].StartTime && CurrentBreakIndex > 0)
                        CurrentBreakIndex--;
                }

                var closest = breaks[CurrentBreakIndex];

                return closest.Contains(time) ? closest : null;
            }

            return null;
        }
    }
}

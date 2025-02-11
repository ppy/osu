// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Scoring;
using osu.Game.Utils;

namespace osu.Game.Screens.Play
{
    public partial class BreakTracker : Component
    {
        private readonly ScoreProcessor scoreProcessor;
        private readonly double gameplayStartTime;

        private PeriodTracker breaks = new PeriodTracker(Enumerable.Empty<Period>());

        /// <summary>
        /// Whether the gameplay is currently in a break.
        /// </summary>
        public IBindable<bool> IsBreakTime => isBreakTime;

        private readonly BindableBool isBreakTime = new BindableBool(true);

        public readonly Bindable<Period?> CurrentPeriod = new Bindable<Period?>();

        public IReadOnlyList<BreakPeriod> Breaks
        {
            set
            {
                breaks = new PeriodTracker(value.Where(b => b.HasEffect)
                                                .Select(b => new Period(b.StartTime, b.EndTime - BreakOverlay.BREAK_FADE_DURATION)));

                if (IsLoaded)
                    updateBreakTime();
            }
        }

        public BreakTracker(double gameplayStartTime, ScoreProcessor scoreProcessor)
        {
            this.gameplayStartTime = gameplayStartTime;
            this.scoreProcessor = scoreProcessor;
        }

        protected override void Update()
        {
            base.Update();
            updateBreakTime();
        }

        private void updateBreakTime()
        {
            double time = Clock.CurrentTime;

            if (breaks.IsInAny(time, out var currentBreak))
            {
                CurrentPeriod.Value = currentBreak;
                isBreakTime.Value = true;
            }
            else
            {
                CurrentPeriod.Value = null;
                isBreakTime.Value = time < gameplayStartTime || scoreProcessor.HasCompleted.Value;
            }
        }
    }
}

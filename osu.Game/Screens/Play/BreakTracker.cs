// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps.Timing;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Scoring;
using osu.Game.Utils;

namespace osu.Game.Screens.Play
{
    public partial class BreakTracker : Component, IKeyBindingHandler<GlobalAction>
    {
        private readonly ScoreProcessor scoreProcessor;
        private readonly double gameplayStartTime;

        private PeriodTracker breaks;

        public Action SkipBreak;

        /// <summary>
        /// Whether the gameplay is currently in a break.
        /// </summary>
        public IBindable<bool> IsBreakTime => isBreakTime;

        private readonly BindableBool isBreakTime = new BindableBool(true);

        public BreakPeriod CurrentBreak;

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

        public BreakTracker(double gameplayStartTime = 0, ScoreProcessor scoreProcessor = null)
        {
            this.gameplayStartTime = gameplayStartTime;
            this.scoreProcessor = scoreProcessor;
        }

        protected override void Update()
        {
            base.Update();
            updateBreakTime();

            CurrentBreak = getCurrentBreak();
        }

        private void updateBreakTime()
        {
            double time = Clock.CurrentTime;

            isBreakTime.Value = breaks?.IsInAny(time) == true
                                || time < gameplayStartTime
                                || scoreProcessor?.HasCompleted.Value == true;
        }

        [CanBeNull]
        private BreakPeriod getCurrentBreak()
        {
            double time = Clock.CurrentTime;
            Period? period = breaks?.GetPeriodIfAny(time);

            return period == null ? null : new BreakPeriod(period.Value.Start, period.Value.End);
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat || !isBreakTime.Value || CurrentBreak == null)
                return false;

            switch (e.Action)
            {
                case GlobalAction.SkipCutscene:
                    SkipBreak?.Invoke();
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }
    }
}

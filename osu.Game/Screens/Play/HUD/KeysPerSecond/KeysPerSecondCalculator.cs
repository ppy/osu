// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using osu.Game.Rulesets.UI;

namespace osu.Game.Screens.Play.HUD.KeysPerSecond
{
    public class KeysPerSecondCalculator : Component
    {
        private readonly List<double> timestamps;

        private InputListener? listener;

        [Resolved]
        private GameplayClock? gameplayClock { get; set; }

        [Resolved(canBeNull: true)]
        private DrawableRuleset? drawableRuleset { get; set; }

        public InputListener Listener
        {
            set
            {
                onResetRequested?.Invoke();
                listener = value;
            }
        }

        private event Action? onResetRequested;

        private IClock? workingClock => drawableRuleset?.FrameStableClock;

        private double baseRate;

        private double rate
        {
            get
            {
                if (gameplayClock?.TrueGameplayRate > 0)
                {
                    baseRate = gameplayClock.TrueGameplayRate;
                }

                return baseRate;
            }
        }

        private double maxTime = double.NegativeInfinity;

        public bool Ready => workingClock != null && gameplayClock != null && listener != null;
        public int Value => timestamps.Count(isTimestampWithinSpan);

        public KeysPerSecondCalculator()
        {
            RelativeSizeAxes = Axes.Both;
            timestamps = new List<double>();
            onResetRequested += cleanUp;
        }

        private void cleanUp()
        {
            timestamps.Clear();
            maxTime = double.NegativeInfinity;
        }

        public void AddTimestamp()
        {
            if (workingClock == null) return;

            if (workingClock.CurrentTime >= maxTime)
            {
                timestamps.Add(workingClock.CurrentTime);
                maxTime = workingClock.CurrentTime;
            }
        }

        private bool isTimestampWithinSpan(double timestamp)
        {
            if (workingClock == null) return false;

            double span = 1000 * rate;
            double relativeTime = workingClock.CurrentTime - timestamp;
            return relativeTime > 0 && relativeTime <= span;
        }

        public abstract class InputListener : Component
        {
            protected KeysPerSecondCalculator Calculator;

            protected InputListener(KeysPerSecondCalculator calculator)
            {
                RelativeSizeAxes = Axes.Both;
                Depth = float.MinValue;
                Calculator = calculator;
            }
        }
    }
}

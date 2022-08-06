// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Timing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Screens.Play.HUD.KPSCounter
{
    public class KeysPerSecondCalculator
    {
        public static void AddInput()
        {
            onNewInput?.Invoke();
        }

        private readonly List<double> timestamps;
        private GameplayClock? gameplayClock;
        private DrawableRuleset? drawableRuleset;

        public GameplayClock? GameplayClock
        {
            get => gameplayClock;
            set
            {
                onResetRequested?.Invoke();

                if (value != null)
                {
                    gameplayClock = value;
                }
            }
        }

        public DrawableRuleset? DrawableRuleset
        {
            get => drawableRuleset;
            set
            {
                onResetRequested?.Invoke();

                if (value != null)
                {
                    drawableRuleset = value;
                    baseRate = (drawableRuleset.Mods.FirstOrDefault(m => m is ModRateAdjust) as ModRateAdjust)?.SpeedChange.Value
                               ?? 1;
                }
            }
        }

        private static event Action? onNewInput;
        private static event Action? onResetRequested;

        private IClock? workingClock => drawableRuleset?.FrameStableClock;

        private double baseRate;

        private double rate
        {
            get
            {
                if (gameplayClock != null)
                {
                    if (gameplayClock.TrueGameplayRate > 0)
                    {
                        baseRate = gameplayClock.TrueGameplayRate;
                    }
                }

                return baseRate;
            }
        }

        private double maxTime = double.NegativeInfinity;

        public bool Ready => workingClock != null && gameplayClock != null;
        public int Value => timestamps.Count(isTimestampWithinSpan);

        public KeysPerSecondCalculator()
        {
            timestamps = new List<double>();
            onNewInput += addTimestamp;
            onResetRequested += cleanUp;
        }

        private void cleanUp()
        {
            timestamps.Clear();
            maxTime = double.NegativeInfinity;
        }

        private void addTimestamp()
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
            return relativeTime >= 0 && relativeTime <= span;
        }
    }
}

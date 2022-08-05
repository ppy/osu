// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Timing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Screens.Play.HUD.KPSCounter
{
    public class KeysPerSecondCalculator : IDisposable
    {
        private static KeysPerSecondCalculator instance;

        public static void AddInput()
        {
            instance?.onNewInput.Invoke();
        }

        public static KeysPerSecondCalculator GetInstance(GameplayClock gameplayClock = null, DrawableRuleset drawableRuleset = null)
        {
            if (instance != null) return instance;

            try
            {
                return new KeysPerSecondCalculator(gameplayClock, drawableRuleset);
            }
            catch (ArgumentNullException)
            {
                return null;
            }
        }

        private readonly List<double> timestamps;
        private readonly GameplayClock gameplayClock;
        private readonly DrawableRuleset drawableRuleset;

        private event Action onNewInput;

        private IClock workingClock => (IClock)drawableRuleset.FrameStableClock ?? gameplayClock;

        // Having the rate from mods is preffered to using GameplayClock.TrueGameplayRate()
        // as it returns 0 when paused in replays, not useful for players who want to "analyze" a replay.
        private double rate => (drawableRuleset.Mods.FirstOrDefault(m => m is ModRateAdjust) as ModRateAdjust)?.SpeedChange.Value
                               ?? 1;

        private double maxTime = double.NegativeInfinity;

        public bool Ready => workingClock != null && gameplayClock != null;
        public int Value => timestamps.Count(isTimestampWithinSpan);

        private KeysPerSecondCalculator(GameplayClock gameplayClock, DrawableRuleset drawableRuleset)
        {
            instance = this;
            timestamps = new List<double>();
            this.gameplayClock = gameplayClock ?? throw new ArgumentNullException(nameof(gameplayClock));
            this.drawableRuleset = drawableRuleset;
            onNewInput += addTimestamp;
        }

        private void addTimestamp()
        {
            if (workingClock != null && workingClock.CurrentTime >= maxTime && gameplayClock.TrueGameplayRate > 0)
            {
                timestamps.Add(workingClock.CurrentTime);
                maxTime = workingClock.CurrentTime;
            }
        }

        private bool isTimestampWithinSpan(double timestamp)
        {
            if (!Ready)
                return false;

            double span = 1000 * rate;
            double relativeTime = workingClock.CurrentTime - timestamp;
            return relativeTime >= 0 && relativeTime <= span;
        }

        public void Dispose()
        {
            instance = null;
        }

        ~KeysPerSecondCalculator() => Dispose();
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.UI;

namespace osu.Game.Screens.Play.HUD.ClicksPerSecond
{
    public class ClicksPerSecondCalculator : Component
    {
        private readonly List<double> timestamps;

        [Resolved]
        private IGameplayClock gameplayClock { get; set; } = null!;

        [Resolved]
        private DrawableRuleset drawableRuleset { get; set; } = null!;

        private double rate;

        // The latest timestamp GC seeked. Does not affect normal gameplay
        // but prevents duplicate inputs on replays.
        private double latestTime = double.NegativeInfinity;

        public int Value { get; private set; }

        public ClicksPerSecondCalculator()
        {
            RelativeSizeAxes = Axes.Both;
            timestamps = new List<double>();
        }

        protected override void Update()
        {
            base.Update();

            // When pausing in replays (using the space bar) GC.TrueGameplayRate returns 0
            // To prevent CPS value being 0, we store and use the last non-zero TrueGameplayRate
            if (gameplayClock.TrueGameplayRate > 0)
            {
                rate = gameplayClock.TrueGameplayRate;
            }

            Value = timestamps.Count(timestamp =>
            {
                double window = 1000 * rate;
                double relativeTime = drawableRuleset.FrameStableClock.CurrentTime - timestamp;
                return relativeTime > 0 && relativeTime <= window;
            });
        }

        public void AddTimestamp()
        {
            // Discard inputs if current gameplay time is not the latest
            // to prevent duplicate inputs
            if (drawableRuleset.FrameStableClock.CurrentTime >= latestTime)
            {
                timestamps.Add(drawableRuleset.FrameStableClock.CurrentTime);
                latestTime = drawableRuleset.FrameStableClock.CurrentTime;
            }
        }
    }
}

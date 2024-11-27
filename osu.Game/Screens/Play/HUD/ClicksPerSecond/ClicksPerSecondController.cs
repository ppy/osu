// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.UI;

namespace osu.Game.Screens.Play.HUD.ClicksPerSecond
{
    public partial class ClicksPerSecondController : Component
    {
        private readonly List<double> timestamps = new List<double>();

        [Resolved]
        private IGameplayClock gameplayClock { get; set; } = null!;

        [Resolved]
        private IFrameStableClock? frameStableClock { get; set; }

        public int Value { get; private set; }

        private IGameplayClock clock => frameStableClock ?? gameplayClock;

        public ClicksPerSecondController()
        {
            RelativeSizeAxes = Axes.Both;
        }

        public void AddInputTimestamp() => timestamps.Add(clock.CurrentTime);

        protected override void Update()
        {
            base.Update();

            double latestValidTime = clock.CurrentTime;
            double earliestTimeValid = latestValidTime - 1000 * gameplayClock.GetTrueGameplayRate();

            int count = 0;

            for (int i = timestamps.Count - 1; i >= 0; i--)
            {
                // handle rewinding by removing future timestamps as we go
                if (timestamps[i] > latestValidTime)
                {
                    timestamps.RemoveAt(i);
                    continue;
                }

                if (timestamps[i] >= earliestTimeValid)
                    count++;
            }

            Value = count;
        }
    }
}

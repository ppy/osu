// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.MathUtils;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Screens.Gameplay.Components;

namespace osu.Game.Tournament.Tests
{
    public class TestCaseMatchScoreDisplay : LadderTestCase
    {
        [Cached(Type = typeof(MatchIPCInfo))]
        private MatchIPCInfo matchInfo = new MatchIPCInfo();

        public TestCaseMatchScoreDisplay()
        {
            Add(new MatchScoreDisplay
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Scheduler.AddDelayed(() =>
            {
                int amount = (int)((RNG.NextDouble() - 0.5) * 10000);
                if (amount < 0)
                    matchInfo.Score1.Value -= amount;
                else
                    matchInfo.Score2.Value += amount;
            }, 100, true);
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Screens.Gameplay.Components;

namespace osu.Game.Tournament.Tests.Components
{
    public partial class TestSceneMatchScoreDisplay : TournamentTestScene
    {
        [Cached(Type = typeof(MatchIPCInfo))]
        private MatchIPCInfo matchInfo = new MatchIPCInfo();

        public TestSceneMatchScoreDisplay()
        {
            Add(new TournamentMatchScoreDisplay
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

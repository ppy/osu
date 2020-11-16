// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Screens.Gameplay.Components;

namespace osu.Game.Tournament.Tests.Components
{
    public class TestSceneMatchScoreDisplay : TournamentTestScene
    {
        [Cached(Type = typeof(MatchIPCInfo))]
        private MatchIPCInfo matchInfo = new MatchIPCInfo();

        private readonly MatchScoreDisplay matchScoreDisplay;

        public TestSceneMatchScoreDisplay()
        {
            Add(matchScoreDisplay = new MatchScoreDisplay
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }

        private IEnumerable<MatchScoreDisplay.MatchScoreCounter> getCounters() => matchScoreDisplay.ChildrenOfType<MatchScoreDisplay.MatchScoreCounter>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddRepeatStep("add random score", addRandomScore, 20);

            AddAssert("score displays are above zero", () =>
            {
                return getCounters().All(c => c.Current.Value > 0);
            });

            AddStep("end match", () => matchInfo.State.Value = TourneyState.Ranking);

            double scoreBefore = 0;

            AddStep("store scores", () => scoreBefore = getCounters().Sum(c => c.Current.Value));

            AddStep("add random score", addRandomScore);

            AddAssert("displayed scores didn't change", () => scoreBefore == getCounters().Sum(c => c.Current.Value));
        }

        private void addRandomScore()
        {
            int amount = (int)((RNG.NextDouble() - 0.5) * 10000);
            if (amount < 0)
                matchInfo.Score1.Value -= amount;
            else
                matchInfo.Score2.Value += amount;
        }
    }
}

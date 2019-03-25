// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Lists;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestCasePlayerReferenceLeaking : AllPlayersTestCase
    {
        private readonly WeakList<WorkingBeatmap> workingWeakReferences = new WeakList<WorkingBeatmap>();

        private readonly WeakList<Player> playerWeakReferences = new WeakList<Player>();

        protected override void AddCheckSteps()
        {
            AddUntilStep("no leaked beatmaps", () =>
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                int count = 0;

                workingWeakReferences.ForEachAlive(_ => count++);
                return count == 1;
            });

            AddUntilStep("no leaked players", () =>
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                int count = 0;

                playerWeakReferences.ForEachAlive(_ => count++);
                return count == 1;
            });
        }

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, IFrameBasedClock clock)
        {
            var working = base.CreateWorkingBeatmap(beatmap, clock);
            workingWeakReferences.Add(working);
            return working;
        }

        protected override Player CreatePlayer(Ruleset ruleset)
        {
            var player = base.CreatePlayer(ruleset);
            playerWeakReferences.Add(player);
            return player;
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual
{
    /// <summary>
    /// A base class which runs <see cref="Player"/> test for all available rulesets.
    /// Steps to be run for each ruleset should be added via <see cref="AddCheckSteps"/>.
    /// </summary>
    public abstract class AllPlayersTestCase : RateAdjustedBeatmapTestCase
    {
        protected Player Player;

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            foreach (var r in rulesets.AvailableRulesets)
            {
                Player p = null;
                AddStep(r.Name, () => p = loadPlayerFor(r));
                AddUntilStep("player loaded", () =>
                {
                    if (p?.IsLoaded == true)
                    {
                        p = null;
                        return true;
                    }

                    return false;
                });

                AddCheckSteps();
            }

            OsuConfigManager manager;
            Dependencies.Cache(manager = new OsuConfigManager(LocalStorage));
            manager.GetBindable<double>(OsuSetting.DimLevel).Value = 1.0;
        }

        protected abstract void AddCheckSteps();

        protected virtual IBeatmap CreateBeatmap(Ruleset ruleset) => new TestBeatmap(ruleset.RulesetInfo);

        protected virtual WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, IFrameBasedClock clock) =>
            new TestWorkingBeatmap(beatmap, Clock);

        private Player loadPlayerFor(RulesetInfo ri)
        {
            Ruleset.Value = ri;
            var r = ri.CreateInstance();

            var beatmap = CreateBeatmap(r);
            var working = CreateWorkingBeatmap(beatmap, Clock);

            Beatmap.Value = working;
            Mods.Value = new[] { r.GetAllMods().First(m => m is ModNoFail) };

            Player?.Exit();
            Player = null;

            Player = CreatePlayer(r);

            LoadScreen(Player);

            return Player;
        }

        protected virtual Player CreatePlayer(Ruleset ruleset) => new TestPlayer(false, false);
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual
{
    /// <summary>
    /// A base class which runs <see cref="Player"/> test for all available rulesets.
    /// Steps to be run for each ruleset should be added via <see cref="AddCheckSteps"/>.
    /// </summary>
    public abstract class AllPlayersTestScene : RateAdjustedBeatmapTestScene
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

        private Player loadPlayerFor(RulesetInfo rulesetInfo)
        {
            Ruleset.Value = rulesetInfo;
            var ruleset = rulesetInfo.CreateInstance();

            var working = CreateWorkingBeatmap(rulesetInfo);

            Beatmap.Value = working;
            Mods.Value = new[] { ruleset.GetAllMods().First(m => m is ModNoFail) };

            Player?.Exit();
            Player = null;

            Player = CreatePlayer(ruleset);

            LoadScreen(Player);

            return Player;
        }

        protected virtual Player CreatePlayer(Ruleset ruleset) => new TestPlayer(false, false);
    }
}

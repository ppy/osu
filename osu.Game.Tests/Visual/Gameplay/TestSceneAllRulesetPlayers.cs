// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual.Gameplay
{
    /// <summary>
    /// A base class which runs <see cref="Player"/> test for all available rulesets.
    /// Steps to be run for each ruleset should be added via <see cref="AddCheckSteps"/>.
    /// </summary>
    public abstract class TestSceneAllRulesetPlayers : RateAdjustedBeatmapTestScene
    {
        protected Player Player { get; private set; }

        protected OsuConfigManager Config { get; private set; }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            Dependencies.Cache(Config = new OsuConfigManager(LocalStorage));
            Config.GetBindable<double>(OsuSetting.DimLevel).Value = 1.0;
        }

        [Test]
        public void TestOsu() => runForRuleset(new OsuRuleset().RulesetInfo);

        [Test]
        public void TestTaiko() => runForRuleset(new TaikoRuleset().RulesetInfo);

        [Test]
        public void TestCatch() => runForRuleset(new CatchRuleset().RulesetInfo);

        [Test]
        public void TestMania() => runForRuleset(new ManiaRuleset().RulesetInfo);

        private void runForRuleset(RulesetInfo ruleset)
        {
            Player p = null;
            AddStep($"load {ruleset.Name} player", () => p = loadPlayerFor(ruleset));
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

        protected abstract void AddCheckSteps();

        private Player loadPlayerFor(RulesetInfo rulesetInfo)
        {
            Ruleset.Value = rulesetInfo;
            var ruleset = rulesetInfo.CreateInstance();

            var working = CreateWorkingBeatmap(rulesetInfo);

            Beatmap.Value = working;
            SelectedMods.Value = new[] { ruleset.CreateMod<ModNoFail>() };

            Player = CreatePlayer(ruleset);

            LoadScreen(Player);

            return Player;
        }

        protected virtual Player CreatePlayer(Ruleset ruleset) => new TestPlayer(false, false);
    }
}

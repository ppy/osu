// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneUnknownMod : ModTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();

        /// <summary>
        /// This test also covers the scenario of exiting Player after an unsuccessful beatmap load.
        /// </summary>
        [Test]
        public void TestUnknownModDoesntEnterGameplay()
        {
            CreateModTest(new ModTestData
            {
                Beatmap = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo).Beatmap,
                Mod = new UnknownMod("WNG"),
                PassCondition = () => Player.IsLoaded && !Player.LoadedBeatmapSuccessfully
            });
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests
{
    public partial class TestSceneManiaPlayerLegacySkin : LegacySkinPlayerTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new ManiaRuleset();

        // play with a converted beatmap to allow dual stages mod to work.
        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(new RulesetInfo());

        protected override bool HasCustomSteps => true;

        [Test]
        public void TestSingleStage()
        {
            AddStep("Load single stage", LoadPlayer);
            AddUntilStep("player loaded", () => Player.IsLoaded && Player.Alpha == 1);
        }

        [Test]
        public void TestDualStage()
        {
            AddStep("Load dual stage", () => LoadPlayer(new Mod[] { new ManiaModDualStages() }));
            AddUntilStep("player loaded", () => Player.IsLoaded && Player.Alpha == 1);
        }
    }
}

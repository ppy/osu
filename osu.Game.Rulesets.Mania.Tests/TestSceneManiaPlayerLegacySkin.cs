// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Skinning;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Mania.Tests
{
    public class TestSceneManiaPlayerLegacySkin : LegacySkinPlayerTestScene
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

        [Test]
        public void TestLegacyOsuComboCounterHidden([Values] bool withModifiedSkin)
        {
            CreateTest();

            if (withModifiedSkin)
            {
                AddStep("change component scale", () => Player.ChildrenOfType<LegacyScoreCounter>().First().Scale = new Vector2(2f));
                AddStep("update target", () => Player.ChildrenOfType<SkinnableTargetContainer>().ForEach(LegacySkin.UpdateDrawableTarget));
                AddStep("exit player", () => Player.Exit());
                CreateTest();
            }

            AddAssert("legacy osu combo counter hidden", () =>
            {
                return Player.ChildrenOfType<LegacyDefaultComboCounter>().All(c => c.ChildrenOfType<Container>().Single().Alpha == 0f);
            });
        }
    }
}

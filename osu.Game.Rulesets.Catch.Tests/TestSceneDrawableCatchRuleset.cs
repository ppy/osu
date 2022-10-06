// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Logging;
using osu.Framework.Testing;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Mods;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class TestSceneDrawableCatchRulesetWithRelax : OsuTestScene
    {
        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create drawable ruleset with relax mod", () =>
            {
                Child = new DrawableCatchRuleset(new CatchRuleset(), new CatchBeatmap(), new List<Mod>() {
                    new CatchModRelax()
                });
            });
            AddUntilStep("wait for load", () => Child.IsLoaded);
        }

        [Test]
        public void TestBasic()
        {
            AddAssert("check if touch catcher is showing", () => this.ChildrenOfType<CatchTouchInputMapper>().Any() == false);
        }
    }

    [TestFixture]
    public class TestSceneDrawableCatchRulesetWithoutRelax : OsuTestScene
    {
        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create drawable ruleset without relax mod", () =>
            {
                Child = new DrawableCatchRuleset(new CatchRuleset(), new CatchBeatmap(), new List<Mod>());
            });
            AddUntilStep("wait for load", () => Child.IsLoaded);
            Logger.Log("Ready");
        }

        [Test]
        public void TestBasic()
        {
            AddAssert("check if touch catcher is showing", () => this.ChildrenOfType<CatchTouchInputMapper>().Any());
        }
    }
}

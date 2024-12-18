// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Mods;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public partial class TestSceneCatchTouchInput : OsuTestScene
    {
        [Test]
        public void TestBasic()
        {
            CatchTouchInputMapper catchTouchInputMapper = null!;

            AddStep("create input overlay", () =>
            {
                Child = new CatchInputManager(new CatchRuleset().RulesetInfo)
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        catchTouchInputMapper = new CatchTouchInputMapper
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        }
                    }
                };
            });

            AddStep("show overlay", () => catchTouchInputMapper.Show());
        }

        [Test]
        public void TestWithoutRelax()
        {
            AddStep("create drawable ruleset without relax mod", () =>
            {
                Child = new DrawableCatchRuleset(new CatchRuleset(), new CatchBeatmap(), new List<Mod>());
            });
            AddUntilStep("wait for load", () => Child.IsLoaded);
            AddAssert("check touch input is shown", () => this.ChildrenOfType<CatchTouchInputMapper>().Any());
        }

        [Test]
        public void TestWithRelax()
        {
            AddStep("create drawable ruleset with relax mod", () =>
            {
                Child = new DrawableCatchRuleset(new CatchRuleset(), new CatchBeatmap(), new List<Mod> { new CatchModRelax() });
            });
            AddUntilStep("wait for load", () => Child.IsLoaded);
            AddAssert("check touch input is not shown", () => !this.ChildrenOfType<CatchTouchInputMapper>().Any());
        }
    }
}

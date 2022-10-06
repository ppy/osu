// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneLegacyComboSplash : SkinnableHUDComponentTestScene
    {
        [Cached(typeof(ScoreProcessor))]
        private TestScoreProcessor scoreProcessor = new TestScoreProcessor();

        private readonly List<LegacyComboSplash> lcss = new List<LegacyComboSplash>();

        private readonly Bindable<LegacyComboSplash.Side> side = new Bindable<LegacyComboSplash.Side>(LegacyComboSplash.Side.Random);

        protected override Drawable CreateLegacyImplementation()
        {
            var lcs = new LegacyComboSplash();
            lcs.BurstsSide.BindTo(side);
            lcss.Add(lcs);
            return lcs;
        }

        protected override Drawable CreateDefaultImplementation() => CreateLegacyImplementation();

        [Test]
        public void TestShowing()
        {
            AddStep("reset score processor", () => scoreProcessor.Reset());
            AddStep("add switcher", () =>
            {
                Cell(5).Child = new OsuEnumDropdown<LegacyComboSplash.Side>
                {
                    Current = side,
                    RelativeSizeAxes = Axes.X,
                };
            });
            AddAssert("nothing shown", () =>
            {
                return lcss.All(lcs => lcs.ChildrenOfType<LegacyComboSplash.LegacyComboSplashSide>().All(x => x.Children.All(y => y.Alpha == 0)));
            });
            AddStep("set combo to 30", () => scoreProcessor.Combo.Value = 30);
            AddUntilStep("burst shown", () =>
            {
                // all LCS'es subcontainers must be empty or at least one sprite must be shown.
                foreach (var lcs in lcss)
                {
                    if (lcs.ChildrenOfType<LegacyComboSplash.LegacyComboSplashSide>().All(x => x.Count != 0 && x.All(y => y.Alpha == 0))) return false;
                }

                return true;
            });
            AddStep("set combo to 31", () => scoreProcessor.Combo.Value++);
            AddUntilStep("nothing shown", () =>
            {
                return lcss.All(lcs => lcs.ChildrenOfType<LegacyComboSplash.LegacyComboSplashSide>().All(x => x.ChildrenOfType<Sprite>().All(y => y.Alpha == 0)));
            });
            AddSliderStep("current combo", 0, 201, 30, i => scoreProcessor.Combo.Value = i);
        }

        private class TestScoreProcessor : ScoreProcessor
        {
            public TestScoreProcessor()
                : base(new OsuRuleset())
            {
            }

            public void Reset() => base.Reset(false);
        }
    }
}

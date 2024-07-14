// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests.Mods
{
    public partial class TestSceneManiaModHidden : ModTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new ManiaRuleset();

        [Test]
        public void TestMinCoverageFullWidth()
        {
            CreateModTest(new ModTestData
            {
                Mod = new ManiaModHidden(),
                PassCondition = () => checkCoverage(ManiaModHidden.MIN_COVERAGE)
            });
        }

        [Test]
        public void TestMinCoverageHalfWidth()
        {
            CreateModTest(new ModTestData
            {
                Mod = new ManiaModHidden(),
                PassCondition = () => checkCoverage(ManiaModHidden.MIN_COVERAGE)
            });

            AddStep("set playfield width to 0.5", () => Player.Width = 0.5f);
        }

        [Test]
        public void TestMaxCoverageFullWidth()
        {
            CreateModTest(new ModTestData
            {
                Mod = new ManiaModHidden(),
                PassCondition = () => checkCoverage(ManiaModHidden.MAX_COVERAGE)
            });

            AddStep("set combo to 480", () => Player.ScoreProcessor.Combo.Value = 480);
        }

        [Test]
        public void TestMaxCoverageHalfWidth()
        {
            CreateModTest(new ModTestData
            {
                Mod = new ManiaModHidden(),
                PassCondition = () => checkCoverage(ManiaModHidden.MAX_COVERAGE)
            });

            AddStep("set combo to 480", () => Player.ScoreProcessor.Combo.Value = 480);
            AddStep("set playfield width to 0.5", () => Player.Width = 0.5f);
        }

        [Test]
        public void TestNoCoverageDuringBreak()
        {
            CreateModTest(new ModTestData
            {
                Mod = new ManiaModHidden(),
                Beatmap = new Beatmap
                {
                    HitObjects = Enumerable.Range(1, 100).Select(i => (HitObject)new Note { StartTime = 1000 + 200 * i }).ToList(),
                    Breaks = { new BreakPeriod(2000, 28000) }
                },
                PassCondition = () => Player.IsBreakTime.Value && checkCoverage(0)
            });
        }

        private bool checkCoverage(float expected)
        {
            Drawable? cover = this.ChildrenOfType<PlayfieldCoveringWrapper>().FirstOrDefault();
            Drawable? filledArea = cover?.ChildrenOfType<Box>().LastOrDefault();

            if (filledArea == null)
                return false;

            float scale = cover!.DrawHeight / (768 - Stage.HIT_TARGET_POSITION);

            // A bit of lenience because the test may end up hitting hitobjects before any assertions.
            return Precision.AlmostEquals(filledArea.DrawHeight / scale, expected, 0.1);
        }
    }
}

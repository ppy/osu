// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Catch.Mods;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Catch.Tests.Mods
{
    public class TestSceneCatchModNoScope : ModTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new CatchRuleset();

        [Test]
        public void TestVisibleDuringBreak()
        {
            CreateModTest(new ModTestData
            {
                Mod = new CatchModNoScope
                {
                    HiddenComboCount = { Value = 0 },
                },
                Autoplay = true,
                PassCondition = () => true,
                Beatmap = new Beatmap
                {
                    HitObjects = new List<HitObject>
                    {
                        new Fruit
                        {
                            X = CatchPlayfield.CENTER_X,
                            StartTime = 1000,
                        },
                        new Fruit
                        {
                            X = CatchPlayfield.CENTER_X,
                            StartTime = 5000,
                        }
                    },
                    Breaks = new List<BreakPeriod>
                    {
                        new BreakPeriod(2000, 4000),
                    }
                }
            });

            AddUntilStep("wait for catcher to hide", () => catcherAlphaAlmostEquals(0));
            AddUntilStep("wait for start of break", isBreak);
            AddUntilStep("wait for catcher to show", () => catcherAlphaAlmostEquals(1));
            AddUntilStep("wait for end of break", () => !isBreak());
            AddUntilStep("wait for catcher to hide", () => catcherAlphaAlmostEquals(0));
        }

        [Test]
        public void TestVisibleAfterComboBreak()
        {
            CreateModTest(new ModTestData
            {
                Mod = new CatchModNoScope
                {
                    HiddenComboCount = { Value = 2 },
                },
                Autoplay = true,
                PassCondition = () => true,
                Beatmap = new Beatmap
                {
                    HitObjects = new List<HitObject>
                    {
                        new Fruit
                        {
                            X = 0,
                            StartTime = 1000,
                        },
                        new Fruit
                        {
                            X = CatchPlayfield.CENTER_X,
                            StartTime = 3000,
                        },
                        new Fruit
                        {
                            X = CatchPlayfield.WIDTH,
                            StartTime = 5000,
                        },
                    }
                }
            });

            AddAssert("catcher must start visible", () => catcherAlphaAlmostEquals(1));
            AddUntilStep("wait for combo", () => Player.ScoreProcessor.Combo.Value >= 2);
            AddAssert("catcher must dim after combo", () => !catcherAlphaAlmostEquals(1));
            AddStep("break combo", () => Player.ScoreProcessor.Combo.Value = 0);
            AddUntilStep("wait for catcher to show", () => catcherAlphaAlmostEquals(1));
        }

        private bool isBreak() => Player.IsBreakTime.Value;

        private bool catcherAlphaAlmostEquals(float alpha)
        {
            var playfield = (CatchPlayfield)Player.DrawableRuleset.Playfield;
            return Precision.AlmostEquals(playfield.CatcherArea.Alpha, alpha);
        }
    }
}

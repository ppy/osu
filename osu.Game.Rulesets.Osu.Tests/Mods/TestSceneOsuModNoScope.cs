// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Utils;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public class TestSceneOsuModNoScope : OsuModTestScene
    {
        [Test]
        public void TestVisibleDuringBreak()
        {
            CreateModTest(new ModTestData
            {
                Mod = new OsuModNoScope
                {
                    HiddenComboCount = { Value = 0 },
                },
                Autoplay = true,
                PassCondition = () => true,
                Beatmap = new Beatmap
                {
                    HitObjects = new List<HitObject>
                    {
                        new HitCircle
                        {
                            Position = new Vector2(300, 192),
                            StartTime = 1000,
                        },
                        new HitCircle
                        {
                            Position = new Vector2(300, 192),
                            StartTime = 5000,
                        }
                    },
                    Breaks = new List<BreakPeriod>
                    {
                        new BreakPeriod(2000, 4000),
                    }
                }
            });

            AddUntilStep("wait for cursor to hide", () => cursorAlphaAlmostEquals(0));
            AddUntilStep("wait for start of break", isBreak);
            AddUntilStep("wait for cursor to show", () => cursorAlphaAlmostEquals(1));
            AddUntilStep("wait for end of break", () => !isBreak());
            AddUntilStep("wait for cursor to hide", () => cursorAlphaAlmostEquals(0));
        }

        [Test]
        public void TestVisibleDuringSpinner()
        {
            CreateModTest(new ModTestData
            {
                Mod = new OsuModNoScope
                {
                    HiddenComboCount = { Value = 0 },
                },
                Autoplay = true,
                PassCondition = () => true,
                Beatmap = new Beatmap
                {
                    HitObjects = new List<HitObject>
                    {
                        new HitCircle
                        {
                            Position = new Vector2(300, 192),
                            StartTime = 1000,
                        },
                        new Spinner
                        {
                            Position = new Vector2(256, 192),
                            StartTime = 2000,
                            Duration = 2000,
                        },
                        new HitCircle
                        {
                            Position = new Vector2(300, 192),
                            StartTime = 5000,
                        }
                    }
                }
            });

            AddUntilStep("wait for cursor to hide", () => cursorAlphaAlmostEquals(0));
            AddUntilStep("wait for start of spinner", isSpinning);
            AddUntilStep("wait for cursor to show", () => cursorAlphaAlmostEquals(1));
            AddUntilStep("wait for end of spinner", () => !isSpinning());
            AddUntilStep("wait for cursor to hide", () => cursorAlphaAlmostEquals(0));
        }

        [Test]
        public void TestVisibleAfterComboBreak()
        {
            CreateModTest(new ModTestData
            {
                Mod = new OsuModNoScope
                {
                    HiddenComboCount = { Value = 2 },
                },
                Autoplay = true,
                PassCondition = () => true,
                Beatmap = new Beatmap
                {
                    HitObjects = new List<HitObject>
                    {
                        new HitCircle
                        {
                            Position = new Vector2(100, 192),
                            StartTime = 1000,
                        },
                        new HitCircle
                        {
                            Position = new Vector2(150, 192),
                            StartTime = 3000,
                        },
                        new HitCircle
                        {
                            Position = new Vector2(200, 192),
                            StartTime = 5000,
                        },
                    }
                }
            });

            AddAssert("cursor must start visible", () => cursorAlphaAlmostEquals(1));
            AddUntilStep("wait for combo", () => Player.ScoreProcessor.Combo.Value >= 2);
            AddAssert("cursor must dim after combo", () => !cursorAlphaAlmostEquals(1));
            AddStep("break combo", () => Player.ScoreProcessor.Combo.Value = 0);
            AddUntilStep("wait for cursor to show", () => cursorAlphaAlmostEquals(1));
        }

        private bool isSpinning() => Player.ChildrenOfType<DrawableSpinner>().SingleOrDefault()?.Progress > 0;

        private bool isBreak() => Player.IsBreakTime.Value;

        private bool cursorAlphaAlmostEquals(float alpha) => Precision.AlmostEquals(Player.DrawableRuleset.Cursor.Alpha, alpha);
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public class TestSceneOsuModNoScope : OsuModTestScene
    {
        [Test]
        public void CursorVisibleDuringBreak()
        {
            CreateModTest(new ModTestData
            {
                Mod = new OsuModNoScope
                {
                    HiddenComboCount = { Value = 0 },
                },
                Autoplay = true,
                Beatmap = new Beatmap
                {
                    HitObjects = new List<HitObject>
                    {
                        new HitCircle
                        {
                            Position = new Vector2(300, 192),
                            StartTime = 0,
                        },
                        new HitCircle
                        {
                            Position = new Vector2(300, 192),
                            StartTime = 6000,
                        }
                    },
                    Breaks = new List<BreakPeriod>
                    {
                        new BreakPeriod
                        (
                            2000,
                            4000
                        ),
                    }
                },
                PassCondition = () => breakTime() && cursorVisible(true)
            });
        }

        [Test]
        public void CursorVisibleDuringSpinner()
        {
            CreateModTest(new ModTestData
            {
                Mod = new OsuModNoScope
                {
                    HiddenComboCount = { Value = 0 },
                },
                Autoplay = true,
                Beatmap = new Beatmap
                {
                    HitObjects = new List<HitObject>
                    {
                        new HitCircle
                        {
                            Position = new Vector2(300, 192),
                            StartTime = 0,
                        },
                        new Spinner
                        {
                            Position = new Vector2(256, 192),
                            StartTime = 2000,
                            EndTime = 4000,
                        },
                        new HitCircle
                        {
                            Position = new Vector2(300, 192),
                            StartTime = 6000,
                        }
                    },
                },
                PassCondition = () => spinnerTime() && cursorVisible(true)
            });
        }

        [Test]
        public void CursorHiddenAfterHit()
        {
            CreateModTest(new ModTestData
            {
                Mod = new OsuModNoScope
                {
                    HiddenComboCount = { Value = 1 },
                },
                Autoplay = true,
                Beatmap = new Beatmap
                {
                    HitObjects = new List<HitObject>
                    {
                        new HitCircle
                        {
                            Position = new Vector2(300, 192),
                            StartTime = 0,
                        },
                        new HitCircle
                        {
                            Position = new Vector2(300, 192),
                            StartTime = 2000,
                        },
                    },
                },
                PassCondition = () => checkSomeHit() && cursorVisible(false)
            });
        }

        [Test]
        public void CursorVisibleAfterComboBreak()
        {
            CreateModTest(new ModTestData
            {
                Mod = new OsuModNoScope
                {
                    HiddenComboCount = { Value = 1 },
                },
                Autoplay = false,
                Beatmap = new Beatmap
                {
                    HitObjects = new List<HitObject>
                    {
                        new HitCircle
                        {
                            Position = new Vector2(300, 192),
                            StartTime = 0,
                        },
                        new HitCircle
                        {
                            Position = new Vector2(300, 192),
                            StartTime = 2000,
                        },
                    },
                },
                PassCondition = () => comboBreak() && cursorVisible(true)
            });

            AddStep("increase combo", () => Player.ScoreProcessor.Combo.Value += 1);
        }

        private bool checkSomeHit() => Player.ScoreProcessor.JudgedHits >= 1;

        private bool cursorVisible(bool visible)
        {
            float alpha = visible ? 1 : 0;
            return Precision.AlmostEquals(Player.DrawableRuleset.Cursor.Alpha, alpha, 0.1);
        }

        private bool spinnerTime()
        {
            return Player.ChildrenOfType<DrawableSpinner>().SingleOrDefault()?.Progress > 0;
        }

        private bool breakTime()
        {
            return Player.IsBreakTime.Value;
        }

        private bool comboBreak()
        {
            return Player.ScoreProcessor.HighestCombo != Player.ScoreProcessor.Combo;
        }
    }
}

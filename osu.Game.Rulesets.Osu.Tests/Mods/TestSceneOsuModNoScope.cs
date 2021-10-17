// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;
using osu.Game.Beatmaps.Timing;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public class TestSceneOsuModNoScope : OsuModTestScene
    {
        [Test]
        public void BreaksAndSpinners() => CreateModTest(new ModTestData
        {
            Mod = new OsuModNoScope
            {
                HiddenComboCount = { Value = 0 },
                AlwaysHidden = { Value = false },
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
                        StartTime = 5000,
                    },
                    new Spinner
                    {
                        Position = new Vector2(256, 192),
                        StartTime = 6000,
                        EndTime = 9000,
                    },
                    new HitCircle
                    {
                        Position = new Vector2(300, 192),
                        StartTime = 10000,
                    },
                    new HitCircle
                    {
                        Position = new Vector2(300, 192),
                        StartTime = 12000,
                    },
                },
                Breaks = new List<BreakPeriod>
                {
                    new BreakPeriod
                    (
                        1000,
                        4000
                    ),
                }
            },
            PassCondition = () => Player.ScoreProcessor.Combo.Value >= 5
        });
    }
}

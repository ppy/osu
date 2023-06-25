// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Catch.Tests
{
    public partial class TestSceneJuiceStream : TestSceneCatchPlayer
    {
        [Test]
        public void TestJuiceStreamEndingCombo()
        {
            AddUntilStep("player is done", () => !Player.ValidForResume);
        }

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new Beatmap
        {
            BeatmapInfo = new BeatmapInfo
            {
                Difficulty = new BeatmapDifficulty { CircleSize = 5, SliderMultiplier = 2 },
                Ruleset = ruleset
            },
            HitObjects = new List<HitObject>
            {
                new JuiceStream
                {
                    X = CatchPlayfield.CENTER_X,
                    Path = new SliderPath(PathType.Linear, new[]
                    {
                        Vector2.Zero,
                        new Vector2(0, 100)
                    }),
                    StartTime = 200
                },
                new Banana
                {
                    X = CatchPlayfield.CENTER_X,
                    StartTime = 1000,
                    NewCombo = true
                }
            }
        };
    }
}

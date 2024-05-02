// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Catch.Tests
{
    public partial class TestSceneOutOfBoundsObjects : TestSceneCatchPlayer
    {
        protected override bool Autoplay => true;

        [Test]
        public void TestNoOutOfBoundsObjects()
        {
            bool anyObjectOutOfBounds = false;

            AddStep("reset flag", () => anyObjectOutOfBounds = false);

            AddUntilStep("check for out-of-bounds objects",
                () =>
                {
                    anyObjectOutOfBounds |= Player.ChildrenOfType<DrawableCatchHitObject>().Any(dho => dho.X < 0 || dho.X > CatchPlayfield.WIDTH);
                    return Player.ScoreProcessor.HasCompleted.Value;
                });

            AddAssert("no out of bound objects found", () => !anyObjectOutOfBounds);
        }

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new Beatmap
        {
            BeatmapInfo = new BeatmapInfo
            {
                Ruleset = ruleset,
            },
            HitObjects = new List<HitObject>
            {
                new Fruit { StartTime = 1000, X = -50 },
                new Fruit { StartTime = 1200, X = CatchPlayfield.WIDTH + 50 },
                new JuiceStream
                {
                    StartTime = 1500,
                    X = 10,
                    Path = new SliderPath(PathType.LINEAR, new[]
                    {
                        Vector2.Zero,
                        new Vector2(-200, 0)
                    })
                },
                new JuiceStream
                {
                    StartTime = 3000,
                    X = CatchPlayfield.WIDTH - 10,
                    Path = new SliderPath(PathType.LINEAR, new[]
                    {
                        Vector2.Zero,
                        new Vector2(200, 0)
                    })
                },
            }
        };
    }
}

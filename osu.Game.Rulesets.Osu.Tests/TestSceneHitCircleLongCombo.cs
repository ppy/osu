// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class TestSceneHitCircleLongCombo : TestSceneOsuPlayer
    {
        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            var beatmap = new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Difficulty = new BeatmapDifficulty { CircleSize = 6 },
                    Ruleset = ruleset
                }
            };

            for (int i = 0; i < 512; i++)
            {
                if (i % 32 < 20)
                    beatmap.HitObjects.Add(new HitCircle { Position = new Vector2(256, 192), StartTime = i * 100 });
            }

            return beatmap;
        }
    }
}

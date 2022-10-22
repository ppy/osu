// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests
{
    public class TestSceneControlPointConversion : OsuTestScene
    {
        [Test]
        public void TestSceneScrollSpeedConversion()
        {
            const double start_time = 1000;
            const double slider_velocity = 10;

            var beatmap = new Beatmap<HitObject>
            {
                HitObjects =
                {
                    new HitObject
                    {
                        StartTime = start_time,
                        DifficultyControlPoint = new DifficultyControlPoint { SliderVelocity = slider_velocity }
                    }
                },
                BeatmapInfo = { Ruleset = { OnlineID = 0 } },
            };

            var convertedBeatmap = new TaikoRuleset().CreateBeatmapConverter(beatmap).Convert();
            
            AddAssert("effect point generated", () => convertedBeatmap.ControlPointInfo.EffectPointAt(start_time).ScrollSpeed == slider_velocity);
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public partial class TestSceneOsuModCrumblingCircles : OsuModTestScene
    {
        private const float initial_circle_size_f = 5;
        private const float target_circle_size_f = 10;

        [Test]
        public void TestOsuModCrumblingCircles() => CreateModTest(new ModTestData
        {
            Mod = new OsuModCrumblingCircles
            {
                TargetCircleSize = { Value = target_circle_size_f }
            },
            CreateBeatmap = () => new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Difficulty = new BeatmapDifficulty
                    {
                        CircleSize = initial_circle_size_f
                    }
                },
                HitObjects = new List<HitObject>
                {
                    new HitCircle { StartTime = 1000 },
                    new HitCircle { StartTime = 1100 },
                    new HitCircle { StartTime = 1200 }
                }
            },
            Autoplay = true,
            PassCondition = () =>
            {
                var objects = Player.ChildrenOfType<DrawableHitCircle>();
                if (!objects.Any())
                    return false;

                bool firstHitObjectIsInitialSize =
                    Precision.AlmostEquals(objects.Last().HitObject.Scale, getRawScaleForCircleSize(initial_circle_size_f));
                bool lastHitObjectIsTargetSize =
                    Precision.AlmostEquals(objects.First().HitObject.Scale, getRawScaleForCircleSize(target_circle_size_f));

                // First object of the map should be the initial circle size and last one should be the target size
                return firstHitObjectIsInitialSize && lastHitObjectIsTargetSize;
            }
        });

        private float getRawScaleForCircleSize(float circleSize)
        {
            return LegacyRulesetExtensions.CalculateScaleFromCircleSize(circleSize, true);
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public partial class TestSceneOsuModRandom : OsuModTestScene
    {
        [TestCase(1)]
        [TestCase(7)]
        [TestCase(10)]
        public void TestDefaultBeatmap(float angleSharpness) => CreateModTest(new ModTestData
        {
            Mod = new OsuModRandom
            {
                AngleSharpness = { Value = angleSharpness }
            },
            Autoplay = true,
            PassCondition = () => true
        });

        [TestCase(1)]
        [TestCase(7)]
        [TestCase(10)]
        public void TestJumpBeatmap(float angleSharpness) => CreateModTest(new ModTestData
        {
            Mod = new OsuModRandom
            {
                AngleSharpness = { Value = angleSharpness }
            },
            Beatmap = jumpBeatmap,
            Autoplay = true,
            PassCondition = () => true
        });

        [TestCase(1)]
        [TestCase(7)]
        [TestCase(10)]
        public void TestStreamBeatmap(float angleSharpness) => CreateModTest(new ModTestData
        {
            Mod = new OsuModRandom
            {
                AngleSharpness = { Value = angleSharpness }
            },
            Beatmap = streamBeatmap,
            Autoplay = true,
            PassCondition = () => true
        });

        private OsuBeatmap jumpBeatmap =>
            createHitCircleBeatmap(new[] { 100, 200, 300, 400 }, 8, 300, 2 * 300);

        private OsuBeatmap streamBeatmap =>
            createHitCircleBeatmap(new[] { 10, 20, 30, 40, 50, 60, 70, 80 }, 16, 150, 4 * 150);

        private OsuBeatmap createHitCircleBeatmap(IEnumerable<int> spacings, int objectsPerSpacing, int interval, int beatLength)
        {
            var controlPointInfo = new ControlPointInfo();
            controlPointInfo.Add(0, new TimingControlPoint
            {
                Time = 0,
                BeatLength = beatLength
            });

            var beatmap = new OsuBeatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    StackLeniency = 0,
                    Difficulty = new BeatmapDifficulty
                    {
                        ApproachRate = 8.5f
                    }
                },
                ControlPointInfo = controlPointInfo
            };

            foreach (int spacing in spacings)
            {
                for (int i = 0; i < objectsPerSpacing; i++)
                {
                    beatmap.HitObjects.Add(new HitCircle
                    {
                        StartTime = interval * beatmap.HitObjects.Count,
                        Position = beatmap.HitObjects.Count % 2 == 0 ? Vector2.Zero : new Vector2(spacing, 0),
                        NewCombo = i == 0
                    });
                }
            }

            return beatmap;
        }
    }
}

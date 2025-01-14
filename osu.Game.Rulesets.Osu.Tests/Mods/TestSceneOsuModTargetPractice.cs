// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public partial class TestSceneOsuModTargetPractice : OsuModTestScene
    {
        [TestCase(1)]
        [TestCase(2000)]
        [TestCase(3000000)]
        public void TestSeed(int seed) => CreateModTest(new ModTestData
        {
            Mod = new OsuModTargetPractice
            {
                Seed = { Value = seed }
            },
            Autoplay = true,
            PassCondition = () => true
        });

        [Test]
        public void TestMetronome([Values] bool metronome) => CreateModTest(new ModTestData
        {
            Mod = new OsuModTargetPractice
            {
                Metronome = { Value = metronome }
            },
            Autoplay = true,
            PassCondition = () => true
        });

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void TestBeatLengthDivisor(double beatLengthDivisor) => CreateModTest(new ModTestData
        {
            Mod = new OsuModTargetPractice
            {
                BeatLengthDivisor = { Value = beatLengthDivisor }
            },
            Autoplay = true,
            PassCondition = () => true
        });

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void TestVariableTimingPointBeatmap(double beatLengthDivisor) => CreateModTest(new ModTestData
        {
            Mod = new OsuModTargetPractice
            {
                BeatLengthDivisor = { Value = beatLengthDivisor }
            },
            Beatmap = variableTimingPointBeatmap,
            Autoplay = true,
            PassCondition = () => true
        });

        private OsuBeatmap variableTimingPointBeatmap =>
            createHitCircleBeatmap(256, 60, 8 * 60);

        private OsuBeatmap createHitCircleBeatmap(int objectsNumber, int interval, int beatLength)
        {
            var controlPointInfo = new ControlPointInfo();
            controlPointInfo.Add(0, new TimingControlPoint
            {
                Time = 0,
                BeatLength = beatLength
            });
            controlPointInfo.Add(64 * interval, new TimingControlPoint
            {
                Time = 0,
                BeatLength = beatLength * 2
            });
            controlPointInfo.Add(128 * interval, new TimingControlPoint
            {
                Time = 0,
                BeatLength = beatLength / 2
            });
            controlPointInfo.Add(192 * interval, new TimingControlPoint
            {
                Time = 0,
                BeatLength = beatLength * 1.5
            });

            var beatmap = new OsuBeatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    StackLeniency = 0
                },
                ControlPointInfo = controlPointInfo
            };
            for (int i = 0; i < objectsNumber; i++)
            {
                beatmap.HitObjects.Add(new HitCircle
                {
                    StartTime = interval * beatmap.HitObjects.Count
                });
            }

            return beatmap;
        }
    }
}

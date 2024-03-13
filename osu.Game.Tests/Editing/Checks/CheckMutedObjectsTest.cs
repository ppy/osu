// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Editing.Checks
{
    [TestFixture]
    public class CheckMutedObjectsTest
    {
        private CheckMutedObjects check = null!;
        private ControlPointInfo cpi = null!;

        private const int volume_regular = 50;
        private const int volume_low = 15;
        private const int volume_muted = 5;

        [SetUp]
        public void Setup()
        {
            check = new CheckMutedObjects();

            cpi = new LegacyControlPointInfo();
            cpi.Add(0, new SampleControlPoint { SampleVolume = volume_regular });
            cpi.Add(1000, new SampleControlPoint { SampleVolume = volume_low });
            cpi.Add(2000, new SampleControlPoint { SampleVolume = volume_muted });
        }

        [Test]
        public void TestNormalSampleVolume()
        {
            // The sample volume should take precedence over the control point volume.
            var hitCircle = new HitCircle
            {
                StartTime = 2000,
                Samples = new List<HitSampleInfo> { new HitSampleInfo(HitSampleInfo.HIT_NORMAL, volume: volume_regular) }
            };
            hitCircle.ApplyDefaults(cpi, new BeatmapDifficulty());

            assertOk(new List<HitObject> { hitCircle });
        }

        [Test]
        public void TestLowSampleVolume()
        {
            var hitCircle = new HitCircle
            {
                StartTime = 2000,
                Samples = new List<HitSampleInfo> { new HitSampleInfo(HitSampleInfo.HIT_NORMAL, volume: volume_low) }
            };
            hitCircle.ApplyDefaults(cpi, new BeatmapDifficulty());

            assertLowVolume(new List<HitObject> { hitCircle });
        }

        [Test]
        public void TestMutedSampleVolume()
        {
            var hitCircle = new HitCircle
            {
                StartTime = 0,
                Samples = new List<HitSampleInfo> { new HitSampleInfo(HitSampleInfo.HIT_NORMAL, volume: volume_muted) }
            };
            hitCircle.ApplyDefaults(cpi, new BeatmapDifficulty());

            assertMuted(new List<HitObject> { hitCircle });
        }

        [Test]
        public void TestNormalSampleVolumeSlider()
        {
            var sliderHead = new SliderHeadCircle
            {
                StartTime = 0,
                Samples = new List<HitSampleInfo> { new HitSampleInfo(HitSampleInfo.HIT_NORMAL, volume: volume_regular) }
            };
            sliderHead.ApplyDefaults(cpi, new BeatmapDifficulty());

            var sliderTick = new SliderTick
            {
                StartTime = 250,
                Samples = new List<HitSampleInfo> { new HitSampleInfo("slidertick", volume: volume_muted) } // Should be fine.
            };
            sliderTick.ApplyDefaults(cpi, new BeatmapDifficulty());

            var slider = new MockNestableHitObject(new List<HitObject> { sliderHead, sliderTick, }, startTime: 0, endTime: 500)
            {
                Samples = new List<HitSampleInfo> { new HitSampleInfo(HitSampleInfo.HIT_NORMAL, volume: volume_regular) }
            };
            slider.ApplyDefaults(cpi, new BeatmapDifficulty());

            assertOk(new List<HitObject> { slider });
        }

        [Test]
        public void TestMutedSampleVolumeSliderHead()
        {
            var sliderHead = new SliderHeadCircle
            {
                StartTime = 0,
                Samples = new List<HitSampleInfo> { new HitSampleInfo(HitSampleInfo.HIT_NORMAL, volume: volume_muted) }
            };
            sliderHead.ApplyDefaults(cpi, new BeatmapDifficulty());

            var sliderTick = new SliderTick
            {
                StartTime = 250,
                Samples = new List<HitSampleInfo> { new HitSampleInfo("slidertick", volume: volume_regular) }
            };
            sliderTick.ApplyDefaults(cpi, new BeatmapDifficulty());

            var slider = new MockNestableHitObject(new List<HitObject> { sliderHead, sliderTick, }, startTime: 0, endTime: 500)
            {
                Samples = new List<HitSampleInfo> { new HitSampleInfo(HitSampleInfo.HIT_NORMAL, volume: volume_regular) } // Applies to the tail.
            };
            slider.ApplyDefaults(cpi, new BeatmapDifficulty());

            assertMuted(new List<HitObject> { slider });
        }

        [Test]
        public void TestMutedSampleVolumeSliderTail()
        {
            var sliderHead = new SliderHeadCircle
            {
                StartTime = 0,
                Samples = new List<HitSampleInfo> { new HitSampleInfo(HitSampleInfo.HIT_NORMAL, volume: volume_regular) }
            };
            sliderHead.ApplyDefaults(cpi, new BeatmapDifficulty());

            var sliderTick = new SliderTick
            {
                StartTime = 250,
                Samples = new List<HitSampleInfo> { new HitSampleInfo("slidertick", volume: volume_regular) }
            };
            sliderTick.ApplyDefaults(cpi, new BeatmapDifficulty());

            var slider = new MockNestableHitObject(new List<HitObject> { sliderHead, sliderTick, }, startTime: 0, endTime: 2500)
            {
                Samples = new List<HitSampleInfo> { new HitSampleInfo(HitSampleInfo.HIT_NORMAL, volume: volume_muted) } // Applies to the tail.
            };
            slider.ApplyDefaults(cpi, new BeatmapDifficulty());

            assertMutedPassive(new List<HitObject> { slider });
        }

        private void assertOk(List<HitObject> hitObjects)
        {
            Assert.That(check.Run(getContext(hitObjects)), Is.Empty);
        }

        private void assertLowVolume(List<HitObject> hitObjects, int count = 1)
        {
            var issues = check.Run(getContext(hitObjects)).ToList();

            Assert.That(issues, Has.Count.EqualTo(count));
            Assert.That(issues.All(issue => issue.Template is CheckMutedObjects.IssueTemplateLowVolumeActive));
        }

        private void assertMuted(List<HitObject> hitObjects, int count = 1)
        {
            var issues = check.Run(getContext(hitObjects)).ToList();

            Assert.That(issues, Has.Count.EqualTo(count));
            Assert.That(issues.All(issue => issue.Template is CheckMutedObjects.IssueTemplateMutedActive));
        }

        private void assertMutedPassive(List<HitObject> hitObjects)
        {
            var issues = check.Run(getContext(hitObjects)).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Any(issue => issue.Template is CheckMutedObjects.IssueTemplateMutedPassive));
        }

        private BeatmapVerifierContext getContext(List<HitObject> hitObjects)
        {
            var beatmap = new Beatmap<HitObject>
            {
                ControlPointInfo = cpi,
                HitObjects = hitObjects
            };

            return new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));
        }
    }
}

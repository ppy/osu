// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using ManagedBass.Fx;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Game.Audio.Effects;
using osu.Game.Beatmaps;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Audio
{
    public class TestSceneFilter : OsuTestScene
    {
        [Resolved]
        private AudioManager audio { get; set; }

        private WorkingBeatmap testBeatmap;
        private Filter lowPassFilter;
        private Filter highPassFilter;
        private Filter bandPassFilter;

        [BackgroundDependencyLoader]
        private void load()
        {
            testBeatmap = new WaveformTestBeatmap(audio);
            AddRange(new Drawable[]
            {
                lowPassFilter = new Filter(audio.TrackMixer)
                {
                    FilterType = BQFType.LowPass,
                    SweepCutoffStart = 2000,
                    SweepCutoffEnd = 150,
                    SweepDuration = 1000
                },
                highPassFilter = new Filter(audio.TrackMixer)
                {
                    FilterType = BQFType.HighPass,
                    SweepCutoffStart = 150,
                    SweepCutoffEnd = 2000,
                    SweepDuration = 1000
                },
                bandPassFilter = new Filter(audio.TrackMixer)
                {
                    FilterType = BQFType.BandPass,
                    SweepCutoffStart = 150,
                    SweepCutoffEnd = 20000,
                    SweepDuration = 1000
                },
            });
        }

        [Test]
        public void TestLowPass()
        {
            testFilter(lowPassFilter);
        }

        [Test]
        public void TestHighPass()
        {
            testFilter(highPassFilter);
        }

        [Test]
        public void TestBandPass()
        {
            testFilter(bandPassFilter);
        }

        private void testFilter(Filter filter)
        {
            AddStep("Prepare Track", () =>
            {
                testBeatmap = new WaveformTestBeatmap(audio);
                testBeatmap.LoadTrack();
            });
            AddStep("Play Track", () =>
            {
                testBeatmap.Track.Start();
            });
            AddWaitStep("Let track play", 10);
            AddStep("Enable Filter", filter.Enable);
            AddWaitStep("Let track play", 10);
            AddStep("Disable Filter", filter.Disable);
            AddWaitStep("Let track play", 10);
            AddStep("Stop Track", () =>
            {
                testBeatmap.Track.Stop();
            });
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Audio.Track;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Tests.Rulesets.Mods
{
    [TestFixture]
    public class ModTimeRampTest
    {
        private const double start_time = 1000;
        private const double duration = 9000;

        private TrackVirtual track;

        [SetUp]
        public void SetUp()
        {
            track = new TrackVirtual(20_000);
        }

        [TestCase(0, 1)]
        [TestCase(start_time, 1)]
        [TestCase(start_time + duration * ModTimeRamp.FINAL_RATE_PROGRESS / 2, 1.25)]
        [TestCase(start_time + duration * ModTimeRamp.FINAL_RATE_PROGRESS, 1.5)]
        [TestCase(start_time + duration, 1.5)]
        [TestCase(15000, 1.5)]
        public void TestModWindUp(double time, double expectedRate)
        {
            var beatmap = createSingleSpinnerBeatmap();
            var mod = new ModWindUp();
            mod.ApplyToBeatmap(beatmap);
            mod.ApplyToTrack(track);

            seekTrackAndUpdateMod(mod, time);

            Assert.That(mod.SpeedChange.Value, Is.EqualTo(expectedRate));
        }

        [TestCase(0, 1)]
        [TestCase(start_time, 1)]
        [TestCase(start_time + duration * ModTimeRamp.FINAL_RATE_PROGRESS / 2, 0.75)]
        [TestCase(start_time + duration * ModTimeRamp.FINAL_RATE_PROGRESS, 0.5)]
        [TestCase(start_time + duration, 0.5)]
        [TestCase(15000, 0.5)]
        public void TestModWindDown(double time, double expectedRate)
        {
            var beatmap = createSingleSpinnerBeatmap();
            var mod = new ModWindDown
            {
                FinalRate = { Value = 0.5 }
            };
            mod.ApplyToBeatmap(beatmap);
            mod.ApplyToTrack(track);

            seekTrackAndUpdateMod(mod, time);

            Assert.That(mod.SpeedChange.Value, Is.EqualTo(expectedRate));
        }

        [TestCase(0, 1)]
        [TestCase(start_time, 1)]
        [TestCase(2 * start_time, 1.5)]
        public void TestZeroDurationMap(double time, double expectedRate)
        {
            var beatmap = createSingleObjectBeatmap();
            var mod = new ModWindUp();
            mod.ApplyToBeatmap(beatmap);
            mod.ApplyToTrack(track);

            seekTrackAndUpdateMod(mod, time);

            Assert.That(mod.SpeedChange.Value, Is.EqualTo(expectedRate));
        }

        private void seekTrackAndUpdateMod(ModTimeRamp mod, double time)
        {
            track.Seek(time);
            // update the mod via a fake playfield to re-calculate the current rate.
            mod.Update(null);
        }

        private static Beatmap createSingleSpinnerBeatmap()
        {
            return new Beatmap
            {
                HitObjects =
                {
                    new Spinner
                    {
                        StartTime = start_time,
                        Duration = duration
                    }
                }
            };
        }

        private static Beatmap createSingleObjectBeatmap()
        {
            return new Beatmap
            {
                HitObjects =
                {
                    new HitCircle { StartTime = start_time }
                }
            };
        }
    }
}

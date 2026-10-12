// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    [HeadlessTest]
    public partial class TestSceneAutoGeneration : OsuTestScene
    {
        [TestCase(-1, true)]
        [TestCase(0, false)]
        [TestCase(1, false)]
        public void TestAlternating(double offset, bool shouldAlternate)
        {
            const double first_object_time = 1000;
            double secondObjectTime = first_object_time + AutoGenerator.KEY_UP_DELAY + OsuAutoGenerator.MIN_FRAME_SEPARATION_FOR_ALTERNATING + offset;

            var beatmap = new OsuBeatmap();
            beatmap.HitObjects.Add(new HitCircle { StartTime = first_object_time });
            beatmap.HitObjects.Add(new HitCircle { StartTime = secondObjectTime });

            var generated = new OsuAutoGenerator(beatmap, []).Generate();
            var frames = generated.Frames.OfType<OsuReplayFrame>().ToList();

            Assert.That(frames.Exists(f => f.Time == first_object_time && f.Actions.SingleOrDefault() == OsuAction.LeftButton));
            Assert.That(frames.Exists(f => f.Time == first_object_time + AutoGenerator.KEY_UP_DELAY && !f.Actions.Any()));

            Assert.That(frames.Exists(f => f.Time == secondObjectTime && f.Actions.SingleOrDefault() == (shouldAlternate ? OsuAction.RightButton : OsuAction.LeftButton)));
            Assert.That(frames.Exists(f => f.Time == secondObjectTime + AutoGenerator.KEY_UP_DELAY && !f.Actions.Any()));
        }

        [TestCase(300)]
        [TestCase(600)]
        [TestCase(1200)]
        public void TestAlternatingSpecificBPM(double bpm)
        {
            const double first_object_time = 1000;
            double secondObjectTime = first_object_time + 60000 / bpm;

            var beatmap = new OsuBeatmap();
            beatmap.HitObjects.Add(new HitCircle { StartTime = first_object_time });
            beatmap.HitObjects.Add(new HitCircle { StartTime = secondObjectTime });

            var generated = new OsuAutoGenerator(beatmap, []).Generate();
            var frames = generated.Frames.OfType<OsuReplayFrame>().ToList();

            Assert.That(frames.Exists(f => f.Time == first_object_time && f.Actions.SingleOrDefault() == OsuAction.LeftButton));
            Assert.That(frames.Exists(f => f.Time == first_object_time + AutoGenerator.KEY_UP_DELAY && !f.Actions.Any()));

            Assert.That(frames.Exists(f => f.Time == secondObjectTime && f.Actions.SingleOrDefault() == OsuAction.RightButton));
            Assert.That(frames.Exists(f => f.Time == secondObjectTime + AutoGenerator.KEY_UP_DELAY && !f.Actions.Any()));
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using ManagedBass;
using Moq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Models;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Objects;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Resources;
using osuTK.Audio;

namespace osu.Game.Tests.Editing.Checks
{
    [TestFixture]
    public class CheckDelayedHitsoundsTest
    {
        private CheckDelayedHitsounds check = null!;
        private IBeatmap beatmap = null!;

        [SetUp]
        public void SetUp()
        {
            check = new CheckDelayedHitsounds();
            beatmap = new Beatmap<HitObject>
            {
                BeatmapInfo = new BeatmapInfo
                {
                    BeatmapSet = new BeatmapSetInfo
                    {
                        Files =
                        {
                            new RealmNamedFileUsage(new RealmFile { Hash = "abcdef" }, "normal-hitnormal.wav"),
                        }
                    }
                }
            };

            if (!Bass.Init(0) && Bass.LastError != Errors.Already)
                throw new AudioException("Could not initialize Bass.");
        }

        [Test]
        public void TestNoDelayedHitsounds()
        {
            using var resourceStream = TestResources.OpenResource("Samples/hitsound-no-delay.wav");
            Assert.IsEmpty(check.Run(getContext(resourceStream)));
        }

        [Test]
        public void TestMinorDelayedHitsounds()
        {
            // 1 ms of silence -> 1 ms of noise at 0.3 amplitude -> hitsound transient
            // => The transient is delayed by 2 ms
            // Waveform: https://github.com/ppy/osu/assets/39100084/d5b9edbe-0ba2-401d-94b0-6d57228bdbd3
            using (var resourceStream = TestResources.OpenResource("Samples/hitsound-minor-delay.wav"))
            {
                var issues = check.Run(getContext(resourceStream)).ToList();

                Assert.That(issues, Has.Count.EqualTo(1));
                Assert.That(issues.Single().Template is CheckDelayedHitsounds.IssueTemplateMinorDelay);
            }
        }

        [Test]
        public void TestDelayedHitsounds()
        {
            // 3 ms of silence -> 3 ms of noise at 0.3 amplitude -> hitsound transient
            // => The transient is delayed by 6 ms
            // Waveform: https://github.com/ppy/osu/assets/39100084/2509ff35-d908-414b-b7b9-583681348772
            using var resourceStream = TestResources.OpenResource("Samples/hitsound-delay.wav");

            var issues = check.Run(getContext(resourceStream)).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckDelayedHitsounds.IssueTemplateDelay);
        }

        [Test]
        public void TestConsequentlyDelayedHitsounds()
        {
            // The hitsound is delayed by 10 ms
            // Waveform: https://github.com/ppy/osu/assets/39100084/3a7ede0d-8523-4b99-a222-3624cd208267
            using var resourceStream = TestResources.OpenResource("Samples/hitsound-consequent-delay.wav");

            var issues = check.Run(getContext(resourceStream)).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckDelayedHitsounds.IssueTemplateConsequentDelay);
        }

        private BeatmapVerifierContext getContext(Stream? resourceStream)
        {
            var mockWorkingBeatmap = new Mock<TestWorkingBeatmap>(beatmap, null, null);
            mockWorkingBeatmap.Setup(w => w.GetStream(It.IsAny<string>())).Returns(resourceStream);

            return new BeatmapVerifierContext(beatmap, mockWorkingBeatmap.Object);
        }
    }
}

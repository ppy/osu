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
            using (var resourceStream = TestResources.OpenResource("Samples/hitsound-minor-delay.wav"))
            {
                var issues = check.Run(getContext(resourceStream)).ToList();

                Assert.That(issues, Has.Count.EqualTo(1));
                Assert.That(issues.Single().Template is CheckDelayedHitsounds.IssuTemplateMinorDelay);
            }
        }

        [Test]
        public void TestDelayedHitsounds()
        {
            using var resourceStream = TestResources.OpenResource("Samples/hitsound-delay.wav");

            var issues = check.Run(getContext(resourceStream)).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckDelayedHitsounds.IssueTemplateDelay);
        }

        [Test]
        public void TestConsequentlyDelayedHitsounds()
        {
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

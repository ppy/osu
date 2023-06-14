// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Objects;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Editing.Checks
{
    public class CheckPreviewTimeTest
    {
        private CheckPreviewTime check = null!;

        private IBeatmap beatmap = null!;

        [SetUp]
        public void Setup()
        {
            check = new CheckPreviewTime();
        }

        [Test]
        public void TestPreviewTimeNotSet()
        {
            setNoPreviewTimeBeatmap();
            var content = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));

            var issues = check.Run(content).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckPreviewTime.IssueTemplateHasNoPreviewTime);
        }

        [Test]
        public void TestPreviewTimeconflict()
        {
            setPreviewTimeConflictBeatmap();

            var content = new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));

            var issues = check.Run(content).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckPreviewTime.IssueTemplatePreviewTimeConflict);
            Assert.That(issues.Single().Arguments.FirstOrDefault()?.ToString() == "Test1");
        }

        private void setNoPreviewTimeBeatmap()
        {
            beatmap = new Beatmap<HitObject>
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Metadata = new BeatmapMetadata { PreviewTime = -1 },
                }
            };
        }

        private void setPreviewTimeConflictBeatmap()
        {
            beatmap = new Beatmap<HitObject>
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Metadata = new BeatmapMetadata { PreviewTime = 10 },
                    BeatmapSet = new BeatmapSetInfo(new List<BeatmapInfo>
                    {
                        new BeatmapInfo
                        {
                            DifficultyName = "Test1",
                            Metadata = new BeatmapMetadata { PreviewTime = 5 },
                        },
                        new BeatmapInfo
                        {
                            DifficultyName = "Test2",
                            Metadata = new BeatmapMetadata { PreviewTime = 10 },
                        },
                    })
                }
            };
        }
    }
}

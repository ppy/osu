// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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

        [SetUp]
        public void Setup()
        {
            check = new CheckPreviewTime();
        }

        [Test]
        public void TestPreviewTimeNotSet()
        {
            // single difficulty with no preview time
            var current = createBeatmapWithPreviewPoint(-1, "Current");
            var context = createContext(current, Array.Empty<IBeatmap>());

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckPreviewTime.IssueTemplateHasNoPreviewTime);
        }

        [Test]
        public void TestPreviewTimeConflict()
        {
            var beatmaps = createBeatmapSetWithPreviewPoint(
                ("Current", 10),
                ("Test1", 5),
                ("Test2", 10)
            );

            var context = createContext(beatmaps[0], new[] { beatmaps[1], beatmaps[2] });

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues.Single().Template is CheckPreviewTime.IssueTemplatePreviewTimeConflict);
            Assert.That(issues.Single().Arguments.FirstOrDefault()?.ToString() == "Test1");
        }

        private IBeatmap[] createBeatmapSetWithPreviewPoint(params (string name, int preview)[] entries)
        {
            var beatmapSet = new BeatmapSetInfo();
            var beatmaps = new IBeatmap[entries.Length];

            for (int i = 0; i < entries.Length; i++)
            {
                beatmaps[i] = createBeatmapWithPreviewPoint(entries[i].preview, entries[i].name);
                beatmaps[i].BeatmapInfo.BeatmapSet = beatmapSet;
            }

            foreach (var b in beatmaps)
                beatmapSet.Beatmaps.Add(b.BeatmapInfo);

            return beatmaps;
        }

        private IBeatmap createBeatmapWithPreviewPoint(int previewTime, string difficultyName)
        {
            return new Beatmap<HitObject>
            {
                BeatmapInfo = new BeatmapInfo
                {
                    DifficultyName = difficultyName,
                    Metadata = new BeatmapMetadata { PreviewTime = previewTime }
                }
            };
        }

        private BeatmapVerifierContext createContext(IBeatmap currentBeatmap, IBeatmap[] otherDifficulties)
        {
            var verifiedCurrentBeatmap = new BeatmapVerifierContext.VerifiedBeatmap(new TestWorkingBeatmap(currentBeatmap), currentBeatmap);
            var verifiedOtherBeatmaps = otherDifficulties.Select(b => new BeatmapVerifierContext.VerifiedBeatmap(new TestWorkingBeatmap(b), b)).ToList();

            return new BeatmapVerifierContext(verifiedCurrentBeatmap, verifiedOtherBeatmaps, DifficultyRating.ExpertPlus);
        }
    }
}

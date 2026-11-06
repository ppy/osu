// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Audio.Track;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Localisation;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Editing
{
    [TestFixture]
    public class BeatmapVerifierBeatmapSetTest
    {
        [Test]
        public void TestProblemsAreStableAcrossPreferredDifficulty()
        {
            var clean = createBeatmap("Clean",
                new HitCircle { StartTime = 100 },
                new HitCircle { StartTime = 200 });

            var withConcurrent = createBeatmap("Has Concurrent",
                new HitCircle { StartTime = 100 },
                new HitCircle { StartTime = 100 });

            var fromClean = run(0, i => i.Issue.Template is CheckConcurrentObjects.IssueTemplateConcurrent, clean, withConcurrent);
            var fromBroken = run(1, i => i.Issue.Template is CheckConcurrentObjects.IssueTemplateConcurrent, clean, withConcurrent);

            Assert.That(fromClean.Select(format), Is.EquivalentTo(fromBroken.Select(format)));
            Assert.That(fromClean, Has.Count.EqualTo(1));
            Assert.That(format(fromClean[0]), Does.StartWith("Has Concurrent:"));
        }

        [Test]
        public void TestBeatmapSetScopedProblemsAreReportedOnce()
        {
            var first = createBeatmap("Easy", metadata: createMetadata("Artist A"));
            var second = createBeatmap("Hard", metadata: createMetadata("Artist B"));

            var fromFirst = run(0, i => i.Issue.Template is CheckInconsistentMetadata.IssueTemplateInconsistentOtherFields, first, second);
            var fromSecond = run(1, i => i.Issue.Template is CheckInconsistentMetadata.IssueTemplateInconsistentOtherFields, first, second);

            Assert.That(fromFirst, Has.Count.EqualTo(1));
            Assert.That(fromSecond, Has.Count.EqualTo(1));
            Assert.That(fromFirst[0].Scope.ToString(), Is.EqualTo(EditorStrings.CheckEntireBeatmapSet.ToString()));
        }

        private static string format(BeatmapVerifier.ScopedIssue issue) => $"{issue.Scope}:{issue.Issue}";

        private static List<BeatmapVerifier.ScopedIssue> run(
            int preferredIndex,
            Func<BeatmapVerifier.ScopedIssue, bool> match,
            params IBeatmap[] beatmaps)
        {
            var beatmapSet = new BeatmapSetInfo();

            foreach (var beatmap in beatmaps)
            {
                beatmap.BeatmapInfo.BeatmapSet = beatmapSet;
                beatmapSet.Beatmaps.Add(beatmap.BeatmapInfo);
            }

            var all = beatmaps
                      .Select(b => new BeatmapVerifierContext.VerifiedBeatmap(new TrackLoadedTestWorkingBeatmap(b), b))
                      .ToList();

            return BeatmapVerifier.RunForBeatmapSet(all, all[preferredIndex]).Where(match).ToList();
        }

        private static BeatmapMetadata createMetadata(string artist) => new BeatmapMetadata
        {
            Artist = artist,
            Title = "Title",
            Author = { Username = "Mapper" },
            Tags = "rock english",
        };

        private static IBeatmap createBeatmap(string difficultyName, params HitObject[] hitObjects)
            => createBeatmap(difficultyName, null, hitObjects);

        private static IBeatmap createBeatmap(string difficultyName, BeatmapMetadata? metadata, params HitObject[] hitObjects)
        {
            return new Beatmap<HitObject>
            {
                BeatmapInfo = new BeatmapInfo
                {
                    DifficultyName = difficultyName,
                    Ruleset = new OsuRuleset().RulesetInfo,
                    Metadata = metadata ?? new BeatmapMetadata
                    {
                        Artist = "Test Artist",
                        Title = "Test Title",
                        Author = { Username = "Test Mapper" },
                        Tags = "rock english",
                    },
                },
                HitObjects = hitObjects.ToList(),
            };
        }

        private class TrackLoadedTestWorkingBeatmap : TestWorkingBeatmap
        {
            private readonly Track track;

            public TrackLoadedTestWorkingBeatmap(IBeatmap beatmap)
                : base(beatmap)
            {
                track = new OsuTestScene.ClockBackedTestWorkingBeatmap.TrackVirtualManual(new FramedClock()) { Length = 60000 };
                LoadTrack();
            }

            protected override Track GetBeatmapTrack() => track;
        }
    }
}

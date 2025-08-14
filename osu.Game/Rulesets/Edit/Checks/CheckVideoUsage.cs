// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckVideoUsage : ICheck
    {
        public CheckMetadata Metadata => new CheckMetadata(CheckCategory.Resources, "Inconsistent video usage", CheckScope.BeatmapSet);

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateDifferentVideo(this),
            new IssueTemplateDifferentStartTime(this),
            new IssueTemplateMissingVideo(this)
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            var currentVideo = ResourcesCheckUtils.GetDifficultyVideo(context.CurrentDifficulty.Working);

            // If current difficulty has no video but any other does -> problem
            if (currentVideo == null)
            {
                foreach (var otherDifficulty in context.OtherDifficulties)
                {
                    if (ResourcesCheckUtils.GetDifficultyVideo(otherDifficulty.Working) != null)
                    {
                        yield return new IssueTemplateMissingVideo(this).Create(context.CurrentDifficulty.Playable.BeatmapInfo.DifficultyName);

                        break;
                    }
                }
            }

            // If current has a video, check for missing video on other difficulties and warn about different files vs current.
            if (currentVideo != null)
            {
                string referencePath = currentVideo.Path;

                foreach (var otherDifficulty in context.OtherDifficulties)
                {
                    var otherVideo = ResourcesCheckUtils.GetDifficultyVideo(otherDifficulty.Working);
                    string difficultyName = otherDifficulty.Playable.BeatmapInfo.DifficultyName;

                    // If other difficulty has no video -> problem
                    if (otherVideo == null)
                    {
                        yield return new IssueTemplateMissingVideo(this).Create(difficultyName);

                        continue;
                    }

                    // Different video used (relative to current) -> warning
                    if (!string.Equals(otherVideo.Path, referencePath, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return new IssueTemplateDifferentVideo(this).Create(difficultyName, referencePath, otherVideo.Path);
                    }
                }
            }

            // Pairwise check: for each video file used across all difficulties, ensure all start times match.
            // Build a list of all difficulties with a video present (including current).
            var allWithVideos = new List<(string DifficultyName, string Path, double StartTime)>();

            if (currentVideo != null)
                allWithVideos.Add((context.CurrentDifficulty.Playable.BeatmapInfo.DifficultyName, currentVideo.Path, currentVideo.StartTime));

            foreach (var other in context.OtherDifficulties)
            {
                var video = ResourcesCheckUtils.GetDifficultyVideo(other.Working);

                if (video != null)
                {
                    string name = other.Playable.BeatmapInfo.DifficultyName;
                    allWithVideos.Add((name, video.Path, video.StartTime));
                }
            }

            // Group by video path (case-insensitive) and compare start times pairwise within each group.
            foreach (var group in allWithVideos.GroupBy(v => v.Path, StringComparer.OrdinalIgnoreCase))
            {
                var list = group.ToList();

                for (int i = 0; i < list.Count; i++)
                {
                    for (int j = i + 1; j < list.Count; j++)
                    {
                        if (!list[i].StartTime.Equals(list[j].StartTime))
                        {
                            yield return new IssueTemplateDifferentStartTime(this).Create(
                                group.Key,
                                list[i].DifficultyName, list[i].StartTime,
                                list[j].DifficultyName, list[j].StartTime);
                        }
                    }
                }
            }
        }

        public class IssueTemplateDifferentVideo : IssueTemplate
        {
            public IssueTemplateDifferentVideo(ICheck check)
                : base(check, IssueType.Warning, "Video file differs from current difficulty in \"{0}\" (current: \"{1}\", other: \"{2}\"). Ensure this makes sense.")
            {
            }

            public Issue Create(string otherDifficulty, string currentPath, string otherPath)
                => new Issue(this, otherDifficulty, currentPath, otherPath);
        }

        public class IssueTemplateDifferentStartTime : IssueTemplate
        {
            public IssueTemplateDifferentStartTime(ICheck check)
                : base(check, IssueType.Problem, "Video start time differs for \"{0}\" between \"{1}\" ({2:0} ms) and \"{3}\" ({4:0} ms).")
            {
            }

            public Issue Create(string path, string difficultyA, double startA, string difficultyB, double startB)
                => new Issue(this, path, difficultyA, startA, difficultyB, startB);
        }

        public class IssueTemplateMissingVideo : IssueTemplate
        {
            public IssueTemplateMissingVideo(ICheck check)
                : base(check, IssueType.Problem, "Video is missing in \"{0}\".")
            {
            }

            public Issue Create(string otherDifficulty) => new Issue(this, otherDifficulty);
        }
    }
}

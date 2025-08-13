// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
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
                        yield return new IssueTemplateMissingVideo(this).Create(otherDifficulty.Playable.BeatmapInfo.DifficultyName);

                        break;
                    }
                }

                yield break;
            }

            string referencePath = currentVideo.Path;
            double referenceStart = currentVideo.StartTime;

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

                // Different video used -> warning
                if (!string.Equals(otherVideo.Path, referencePath, System.StringComparison.OrdinalIgnoreCase))
                {
                    yield return new IssueTemplateDifferentVideo(this).Create(difficultyName, referencePath, otherVideo.Path);

                    continue;
                }

                // Same video but different start times -> problem
                if (!referenceStart.Equals(otherVideo.StartTime))
                {
                    yield return new IssueTemplateDifferentStartTime(this).Create(referencePath, difficultyName, referenceStart, otherVideo.StartTime);
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
                : base(check, IssueType.Problem, "Video start time differs for \"{0}\" in \"{1}\" (current: {2:0} ms, other: {3:0} ms).")
            {
            }

            public Issue Create(string path, string otherDifficulty, double currentStartMs, double otherStartMs)
                => new Issue(this, path, otherDifficulty, currentStartMs, otherStartMs);
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

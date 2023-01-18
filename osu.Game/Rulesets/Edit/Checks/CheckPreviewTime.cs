// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckPreviewTime : ICheck
    {
        public CheckMetadata Metadata => new CheckMetadata(CheckCategory.Timing, "Check Preview Time Consistency");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplatePreviewTimeConflict(this),
            new IssueTemplateHasNoPreviewTime(this),
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            var diffList = context.Beatmap.BeatmapInfo.BeatmapSet?.Beatmaps ?? new List<BeatmapInfo>();
            int previewTime = context.Beatmap.BeatmapInfo.Metadata.PreviewTime;

            if (previewTime == -1)
            {
                yield return new IssueTemplateHasNoPreviewTime(this).Create();
            }

            foreach (var diff in diffList)
            {
                if (diff.Equals(context.Beatmap.BeatmapInfo))
                {
                    continue;
                }

                if (diff.Metadata.PreviewTime != previewTime)
                {
                    yield return new IssueTemplatePreviewTimeConflict(this).Create(diff.DifficultyName);
                }
            }
        }

        public class IssueTemplatePreviewTimeConflict : IssueTemplate
        {
            public IssueTemplatePreviewTimeConflict(ICheck check)
                : base(check, IssueType.Warning, "Audio preview time conflicts with {0} diff")
            {
            }

            public Issue Create(string diffName) => new Issue(this, diffName);
        }

        public class IssueTemplateHasNoPreviewTime : IssueTemplate
        {
            public IssueTemplateHasNoPreviewTime(ICheck check)
                : base(check, IssueType.Warning, "A preview point for this map is not set. Consider settings one from the Timing menu")
            {
            }

            public Issue Create() => new Issue(this);
        }
    }
}

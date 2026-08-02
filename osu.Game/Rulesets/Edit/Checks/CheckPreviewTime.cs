// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckPreviewTime : ICheck
    {
        public CheckMetadata Metadata => new CheckMetadata(CheckCategory.Timing, "Inconsistent or unset preview time");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplatePreviewTimeConflict(this),
            new IssueTemplateHasNoPreviewTime(this),
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            int previewTime = context.CurrentDifficulty.Playable.BeatmapInfo.Metadata.PreviewTime;

            if (previewTime == -1)
                yield return new IssueTemplateHasNoPreviewTime(this).Create();

            foreach (var beatmap in context.OtherDifficulties)
            {
                if (beatmap.Playable.BeatmapInfo.Metadata.PreviewTime != previewTime)
                    yield return new IssueTemplatePreviewTimeConflict(this).Create(beatmap.Playable.BeatmapInfo.DifficultyName, previewTime, beatmap.Playable.BeatmapInfo.Metadata.PreviewTime);
            }
        }

        public class IssueTemplatePreviewTimeConflict : IssueTemplate
        {
            public IssueTemplatePreviewTimeConflict(ICheck check)
                : base(check, IssueType.Problem, "Audio preview time ({1}) doesn't match the time specified in \"{0}\" ({2})")
            {
            }

            public Issue Create(string diffName, int originalTime, int conflictTime) =>
                // preview time should show (not set) when it is not set.
                new Issue(this, diffName,
                    originalTime != -1 ? $"{originalTime:N0} ms" : "not set",
                    conflictTime != -1 ? $"{conflictTime:N0} ms" : "not set");
        }

        public class IssueTemplateHasNoPreviewTime : IssueTemplate
        {
            public IssueTemplateHasNoPreviewTime(ICheck check)
                : base(check, IssueType.Problem, "A preview point for this map is not set. Consider setting one from the Timing menu.")
            {
            }

            public Issue Create() => new Issue(this);
        }
    }
}

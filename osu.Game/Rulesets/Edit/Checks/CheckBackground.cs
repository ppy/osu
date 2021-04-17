// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckBackground : ICheck
    {
        public CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.Resources, "Missing background");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateNoneSet(this),
            new IssueTemplateDoesNotExist(this)
        };

        public IEnumerable<Issue> Run(WorkingBeatmap workingBeatmap)
        {
            string backgroundFile = workingBeatmap.Beatmap.Metadata?.BackgroundFile;

            if (backgroundFile == null)
            {
                yield return new IssueTemplateNoneSet(this).Create();

                yield break;
            }

            // If the background is set, also make sure it still exists.
            var storagePath = workingBeatmap.Beatmap.BeatmapInfo.BeatmapSet.GetPathForFile(backgroundFile);
            if (storagePath != null)
                yield break;

            yield return new IssueTemplateDoesNotExist(this).Create(backgroundFile);
        }

        public class IssueTemplateNoneSet : IssueTemplate
        {
            public IssueTemplateNoneSet(ICheck check)
                : base(check, IssueType.Problem, "No background has been set.")
            {
            }

            public Issue Create() => new Issue(this);
        }

        public class IssueTemplateDoesNotExist : IssueTemplate
        {
            public IssueTemplateDoesNotExist(ICheck check)
                : base(check, IssueType.Problem, "The background file \"{0}\" does not exist.")
            {
            }

            public Issue Create(string filename) => new Issue(this, filename);
        }
    }
}

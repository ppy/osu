// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckBackground : ICheck
    {
        private readonly IssueTemplateNoneSet templateNoneSet;
        private readonly IssueTemplateDoesNotExist templateDoesNotExist;
        private readonly IssueTemplate[] templates;

        public CheckBackground()
        {
            templates = new IssueTemplate[]
            {
                templateNoneSet = new IssueTemplateNoneSet(this),
                templateDoesNotExist = new IssueTemplateDoesNotExist(this)
            };
        }

        public CheckMetadata Metadata { get; } = new CheckMetadata
        (
            category: CheckCategory.Resources,
            description: "Missing background."
        );

        public IEnumerable<IssueTemplate> PossibleTemplates => templates;

        public IEnumerable<Issue> Run(IBeatmap beatmap)
        {
            if (beatmap.Metadata.BackgroundFile == null)
            {
                yield return templateNoneSet.Create();

                yield break;
            }

            // If the background is set, also make sure it still exists.

            var set = beatmap.BeatmapInfo.BeatmapSet;
            var file = set.Files.FirstOrDefault(f => f.Filename == beatmap.Metadata.BackgroundFile);

            if (file != null)
                yield break;

            yield return templateDoesNotExist.Create(beatmap.Metadata.BackgroundFile);
        }

        private class IssueTemplateNoneSet : IssueTemplate
        {
            public IssueTemplateNoneSet(ICheck checkOrigin)
                : base(checkOrigin, IssueType.Problem, "No background has been set")
            {
            }

            public Issue Create() => new Issue(this);
        }

        private class IssueTemplateDoesNotExist : IssueTemplate
        {
            public IssueTemplateDoesNotExist(ICheck checkOrigin)
                : base(checkOrigin, IssueType.Problem, "The background file \"{0}\" does not exist.")
            {
            }

            public Issue Create(string filename) => new Issue(this, filename);
        }
    }
}

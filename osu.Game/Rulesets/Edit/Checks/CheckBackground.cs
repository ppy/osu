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
        public CheckMetadata Metadata { get; } = new CheckMetadata
        (
            category: CheckCategory.Resources,
            description: "Missing background."
        );

        public IEnumerable<IssueTemplate> PossibleTemplates => new[]
        {
            templateNoneSet,
            templateDoesNotExist
        };

        private readonly IssueTemplate templateNoneSet = new IssueTemplate
        (
            type: IssueType.Problem,
            unformattedMessage: "No background has been set."
        );

        private readonly IssueTemplate templateDoesNotExist = new IssueTemplate
        (
            type: IssueType.Problem,
            unformattedMessage: "The background file \"{0}\" is does not exist."
        );

        public IEnumerable<Issue> Run(IBeatmap beatmap)
        {
            if (beatmap.Metadata.BackgroundFile == null)
            {
                yield return new Issue(this, templateNoneSet);

                yield break;
            }

            // If the background is set, also make sure it still exists.

            var set = beatmap.BeatmapInfo.BeatmapSet;
            var file = set.Files.FirstOrDefault(f => f.Filename == beatmap.Metadata.BackgroundFile);

            if (file != null)
                yield break;

            yield return new Issue(this, templateDoesNotExist, beatmap.Metadata.BackgroundFile);
        }
    }
}

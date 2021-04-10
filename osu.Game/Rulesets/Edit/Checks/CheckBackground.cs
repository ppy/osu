// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckBackground : Check
    {
        public override CheckMetadata Metadata() => new CheckMetadata
        (
            category: CheckMetadata.CheckCategory.Resources,
            description: "Missing background."
        );

        public override IEnumerable<IssueTemplate> Templates() => new[]
        {
            templateNoneSet,
            templateDoesNotExist
        };

        private readonly IssueTemplate templateNoneSet = new IssueTemplate
        (
            type: IssueTemplate.IssueType.Problem,
            unformattedMessage: "No background has been set."
        );

        private readonly IssueTemplate templateDoesNotExist = new IssueTemplate
        (
            type: IssueTemplate.IssueType.Problem,
            unformattedMessage: "The background file \"{0}\" is does not exist."
        );

        public override IEnumerable<Issue> Run(IBeatmap beatmap)
        {
            if (beatmap.Metadata.BackgroundFile == null)
            {
                yield return new Issue(templateNoneSet);

                yield break;
            }

            // If the background is set, also make sure it still exists.

            var set = beatmap.BeatmapInfo.BeatmapSet;
            var file = set.Files.FirstOrDefault(f => f.Filename == beatmap.Metadata.BackgroundFile);

            if (file != null)
                yield break;

            yield return new Issue(templateDoesNotExist, beatmap.Metadata.BackgroundFile);
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckUnusedAudioAtEnd : ICheck
    {
        public CheckMetadata Metadata => new CheckMetadata(CheckCategory.Compose, "More than 20% unused audio at the end");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateUnusedAudioAtEnd(this),
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            double mappedLength = context.Beatmap.GetLastObjectTime();
            double trackLength = context.WorkingBeatmap.Track.Length;

            double mappedPercentage = Math.Round(mappedLength / trackLength * 100);

            if (mappedPercentage < 80)
            {
                double percentageLeft = Math.Abs(mappedPercentage - 100);
                yield return new IssueTemplateUnusedAudioAtEnd(this).Create(percentageLeft);
            }
        }

        public class IssueTemplateUnusedAudioAtEnd : IssueTemplate
        {
            public IssueTemplateUnusedAudioAtEnd(ICheck check)
                : base(check, IssueType.Warning, "Currently there is {0}% unused audio at the end. Ensure the outro significantly contributes to the song, otherwise cut the outro.")
            {
            }

            public Issue Create(double percentageLeft) => new Issue(this, percentageLeft);
        }
    }
}

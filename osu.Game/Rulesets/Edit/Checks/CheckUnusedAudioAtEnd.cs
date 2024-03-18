// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Objects;

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
            double mappedLength = context.Beatmap.HitObjects.Last().GetEndTime();
            double trackLength = context.WorkingBeatmap.Track.Length;

            double mappedPercentage = Math.Round(mappedLength / trackLength * 100);

            if (mappedPercentage < 80)
            {
                yield return new IssueTemplateUnusedAudioAtEnd(this).Create();
            }
        }

        public class IssueTemplateUnusedAudioAtEnd : IssueTemplate
        {
            public IssueTemplateUnusedAudioAtEnd(ICheck check)
                : base(check, IssueType.Problem, "There is more than 20% unused audio at the end.")
            {
            }

            public Issue Create() => new Issue(this);
        }
    }
}

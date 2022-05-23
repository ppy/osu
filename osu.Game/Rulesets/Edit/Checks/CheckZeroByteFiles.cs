// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using osu.Game.Extensions;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckZeroByteFiles : ICheck
    {
        public CheckMetadata Metadata => new CheckMetadata(CheckCategory.Files, "Zero-byte files");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateZeroBytes(this)
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            var beatmapSet = context.Beatmap.BeatmapInfo.BeatmapSet;

            if (beatmapSet != null)
            {
                foreach (var file in beatmapSet.Files)
                {
                    using (Stream data = context.WorkingBeatmap.GetStream(file.File.GetStoragePath()))
                    {
                        if (data?.Length == 0)
                            yield return new IssueTemplateZeroBytes(this).Create(file.Filename);
                    }
                }
            }
        }

        public class IssueTemplateZeroBytes : IssueTemplate
        {
            public IssueTemplateZeroBytes(ICheck check)
                : base(check, IssueType.Problem, "\"{0}\" is a 0-byte file.")
            {
            }

            public Issue Create(string filename) => new Issue(this, filename);
        }
    }
}

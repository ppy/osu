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
        public CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.Resources, "缺少背景");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateNoneSet(this),
            new IssueTemplateDoesNotExist(this)
        };

        public IEnumerable<Issue> Run(IBeatmap beatmap)
        {
            if (beatmap.Metadata.BackgroundFile == null)
            {
                yield return new IssueTemplateNoneSet(this).Create();

                yield break;
            }

            // If the background is set, also make sure it still exists.

            var set = beatmap.BeatmapInfo.BeatmapSet;
            var file = set.Files.FirstOrDefault(f => f.Filename == beatmap.Metadata.BackgroundFile);

            if (file != null)
                yield break;

            yield return new IssueTemplateDoesNotExist(this).Create(beatmap.Metadata.BackgroundFile);
        }

        public class IssueTemplateNoneSet : IssueTemplate
        {
            public IssueTemplateNoneSet(ICheck check)
                : base(check, IssueType.Problem, "谱面没有背景文件")
            {
            }

            public Issue Create() => new Issue(this);
        }

        public class IssueTemplateDoesNotExist : IssueTemplate
        {
            public IssueTemplateDoesNotExist(ICheck check)
                : base(check, IssueType.Problem, "未找到文件 \"{0}\"")
            {
            }

            public Issue Create(string filename) => new Issue(this, filename);
        }
    }
}

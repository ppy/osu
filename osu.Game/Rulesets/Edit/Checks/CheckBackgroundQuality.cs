// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckBackgroundQuality : ICheck
    {
        // These are the requirements as stated in the Ranking Criteria.
        // See https://osu.ppy.sh/wiki/en/Ranking_Criteria#rules.5
        private const int min_width = 160;
        private const int max_width = 2560;
        private const int min_height = 120;
        private const int max_height = 1440;
        private const double max_filesize_mb = 2.5d;

        // It's usually possible to find a higher resolution of the same image if lower than these.
        private const int low_width = 960;
        private const int low_height = 540;

        public CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.Resources, "Too high or low background resolution");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateTooHighResolution(this),
            new IssueTemplateTooLowResolution(this),
            new IssueTemplateTooUncompressed(this)
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            string backgroundFile = context.Beatmap.Metadata.BackgroundFile;
            if (string.IsNullOrEmpty(backgroundFile))
                yield break;

            var texture = context.WorkingBeatmap.GetBackground();
            if (texture == null)
                yield break;

            if (texture.Width > max_width || texture.Height > max_height)
                yield return new IssueTemplateTooHighResolution(this).Create(texture.Width, texture.Height);

            if (texture.Width < min_width || texture.Height < min_height)
                yield return new IssueTemplateTooLowResolution(this).Create(texture.Width, texture.Height);
            else if (texture.Width < low_width || texture.Height < low_height)
                yield return new IssueTemplateLowResolution(this).Create(texture.Width, texture.Height);

            string? storagePath = context.Beatmap.BeatmapInfo.BeatmapSet?.GetPathForFile(backgroundFile);

            using (Stream stream = context.WorkingBeatmap.GetStream(storagePath))
            {
                double filesizeMb = stream.Length / (1024d * 1024d);

                if (filesizeMb > max_filesize_mb)
                    yield return new IssueTemplateTooUncompressed(this).Create(filesizeMb);
            }
        }

        public class IssueTemplateTooHighResolution : IssueTemplate
        {
            public IssueTemplateTooHighResolution(ICheck check)
                : base(check, IssueType.Problem, "The background resolution ({0} x {1}) exceeds {2} x {3}.")
            {
            }

            public Issue Create(double width, double height) => new Issue(this, width, height, max_width, max_height);
        }

        public class IssueTemplateTooLowResolution : IssueTemplate
        {
            public IssueTemplateTooLowResolution(ICheck check)
                : base(check, IssueType.Problem, "The background resolution ({0} x {1}) is lower than {2} x {3}.")
            {
            }

            public Issue Create(double width, double height) => new Issue(this, width, height, min_width, min_height);
        }

        public class IssueTemplateLowResolution : IssueTemplate
        {
            public IssueTemplateLowResolution(ICheck check)
                : base(check, IssueType.Warning, "The background resolution ({0} x {1}) is lower than {2} x {3}.")
            {
            }

            public Issue Create(double width, double height) => new Issue(this, width, height, low_width, low_height);
        }

        public class IssueTemplateTooUncompressed : IssueTemplate
        {
            public IssueTemplateTooUncompressed(ICheck check)
                : base(check, IssueType.Problem, "The background filesize ({0:0.##} MB) exceeds {1} MB.")
            {
            }

            public Issue Create(double actualMb) => new Issue(this, actualMb, max_filesize_mb);
        }
    }
}

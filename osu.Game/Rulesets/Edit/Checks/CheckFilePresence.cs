// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit.Checks
{
    public abstract class CheckFilePresence : ICheck
    {
        protected abstract CheckCategory Category { get; }
        protected abstract string TypeOfFile { get; }
        protected abstract string GetFilename(IBeatmap beatmap);

        public CheckMetadata Metadata => new CheckMetadata(Category, $"Missing {TypeOfFile}");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateNoneSet(this),
            new IssueTemplateDoesNotExist(this)
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            string filename = GetFilename(context.Beatmap);

            if (string.IsNullOrEmpty(filename))
            {
                yield return new IssueTemplateNoneSet(this).Create(TypeOfFile);

                yield break;
            }

            // If the file is set, also make sure it still exists.
            string storagePath = context.Beatmap.BeatmapInfo.BeatmapSet.GetPathForFile(filename);
            if (storagePath != null)
                yield break;

            yield return new IssueTemplateDoesNotExist(this).Create(TypeOfFile, filename);
        }

        public class IssueTemplateNoneSet : IssueTemplate
        {
            public IssueTemplateNoneSet(ICheck check)
                : base(check, IssueType.Problem, "No {0} has been set.")
            {
            }

            public Issue Create(string typeOfFile) => new Issue(this, typeOfFile);
        }

        public class IssueTemplateDoesNotExist : IssueTemplate
        {
            public IssueTemplateDoesNotExist(ICheck check)
                : base(check, IssueType.Problem, "The {0} file \"{1}\" does not exist.")
            {
            }

            public Issue Create(string typeOfFile, string filename) => new Issue(this, typeOfFile, filename);
        }
    }
}

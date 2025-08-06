// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using osu.Game.Overlays.BeatmapListing;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckMissingGenreLanguage : ICheck
    {
        public CheckMetadata Metadata => new CheckMetadata(CheckCategory.Metadata, "Missing Genre/Language Tags", CheckScope.BeatmapSet);

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateMissingGenre(this),
            new IssueTemplateMissingLanguage(this),
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            var metadata = context.Beatmap.BeatmapInfo.Metadata;

            string tags = metadata.Tags.ToLowerInvariant();

            if (!hasTags<SearchGenre>(tags))
                yield return new IssueTemplateMissingGenre(this).Create();

            if (!hasTags<SearchLanguage>(tags))
                yield return new IssueTemplateMissingLanguage(this).Create();
        }

        private bool hasTags<T>(string tags)
            where T : struct, Enum
        {
            foreach (var value in Enum.GetValues<T>())
            {
                string description = getGenreLanguageString(value);
                string[] words = description.ToLowerInvariant().Split(' ');

                if (words.All(tags.Contains))
                    return true;
            }

            return false;
        }

        // "Video Game" and "Hip Hop" are multiple words that are properly formatted in the enum's description attribute,
        // so we need to use that and fall back to the enum's string value for the rest.
        private string getGenreLanguageString(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? value.ToString();
        }

        public class IssueTemplateMissingGenre : IssueTemplate
        {
            public IssueTemplateMissingGenre(ICheck check)
                : base(check, IssueType.Problem, "Missing genre tag (\"rock\", \"pop\", \"electronic\", etc), ignore if none fit.")
            {
            }

            public Issue Create() => new Issue(this);
        }

        public class IssueTemplateMissingLanguage : IssueTemplate
        {
            public IssueTemplateMissingLanguage(ICheck check)
                : base(check, IssueType.Problem, "Missing language tag (\"english\", \"japanese\", \"instrumental\", etc), ignore if none fit.")
            {
            }

            public Issue Create() => new Issue(this);
        }
    }
}

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

            bool genreFound = false;
            bool languageFound = false;

            foreach (SearchGenre genre in Enum.GetValues(typeof(SearchGenre)))
            {
                string genreString = getGenreLanguageString(genre);

                if (containsAllWords(genreString, tags))
                {
                    genreFound = true;
                    break;
                }
            }

            foreach (SearchLanguage language in Enum.GetValues(typeof(SearchLanguage)))
            {
                string languageString = getGenreLanguageString(language);

                if (containsAllWords(languageString, tags))
                {
                    languageFound = true;
                    break;
                }
            }

            if (!genreFound)
                yield return new IssueTemplateMissingGenre(this).Create();

            if (!languageFound)
                yield return new IssueTemplateMissingLanguage(this).Create();
        }

        private static bool containsAllWords(string description, string tags)
        {
            string[] words = description.ToLowerInvariant().Split(' ');
            return words.All(tags.Contains);
        }

        // "Video Game" and "Hip Hop" are multiple words that are properly formatted in the enum's description attribute,
        // so we need to use that and fall back to the enum's string value for the rest.
        private static string getGenreLanguageString(Enum value)
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

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Text.RegularExpressions;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckTitleMarkers : ICheck
    {
        public CheckMetadata Metadata => new CheckMetadata(CheckCategory.Metadata, "Checks for incorrect formats of (TV Size) / (Game Ver.) / (Short Ver.) / (Cut Ver.) / (Sped Up Ver.) / etc in title.");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateIncorrectMarker(this),
        };

        public IEnumerable<MarkerCheck> MarkerChecks = [
            new MarkerCheck("(TV Size)", @"(?i)(tv (size|ver))"),
            new MarkerCheck("(Game Ver.)", @"(?i)(game (size|ver))"),
            new MarkerCheck("(Short Ver.)", @"(?i)(short (size|ver))"),
            new MarkerCheck("(Cut Ver.)", @"(?i)(?<!& )(cut (size|ver))"),
            new MarkerCheck("(Sped Up Ver.)", @"(?i)(?<!& )(sped|speed) ?up ver"),
            new MarkerCheck("(Nightcore Mix)", @"(?i)(?<!& )(nightcore|night core) (ver|mix)"),
            new MarkerCheck("(Sped Up & Cut Ver.)", @"(?i)(sped|speed) ?up (ver)? ?& cut (size|ver)"),
            new MarkerCheck("(Nightcore & Cut Ver.)", @"(?i)(nightcore|night core) (ver|mix)? ?& cut (size|ver)"),
        ];

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            string romanisedTitle = context.Beatmap.Metadata.Title;
            string unicodeTitle = context.Beatmap.Metadata.TitleUnicode;

            foreach (var check in MarkerChecks)
            {
                bool hasRomanisedTitle = unicodeTitle != romanisedTitle;

                if (check.AnyRegex.IsMatch(romanisedTitle) && !check.ExactRegex.IsMatch(romanisedTitle))
                {
                    yield return new IssueTemplateIncorrectMarker(this).Create(hasRomanisedTitle ? "Romanised title" : "Title", check.CorrectMarkerFormat);
                }

                if (hasRomanisedTitle && check.AnyRegex.IsMatch(unicodeTitle) && !check.ExactRegex.IsMatch(unicodeTitle))
                {
                    yield return new IssueTemplateIncorrectMarker(this).Create("Title", check.CorrectMarkerFormat);
                }
            }
        }

        public class MarkerCheck
        {
            public string CorrectMarkerFormat;
            public Regex ExactRegex;
            public Regex AnyRegex;

            public MarkerCheck(string exact, string anyRegex)
            {
                CorrectMarkerFormat = exact;
                ExactRegex = new Regex(Regex.Escape(exact));
                AnyRegex = new Regex(anyRegex);
            }
        }

        public class IssueTemplateIncorrectMarker : IssueTemplate
        {
            public IssueTemplateIncorrectMarker(ICheck check)
                : base(check, IssueType.Problem, "{0} field has a incorrect format of marker {1}")
            {
            }

            public Issue Create(string titleField, string correctMarkerFormat) => new Issue(this, titleField, correctMarkerFormat);
        }
    }
}
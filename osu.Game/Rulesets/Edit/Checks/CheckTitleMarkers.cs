// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckTitleMarkers : ICheck
    {
        public CheckMetadata Metadata => new CheckMetadata(CheckCategory.Metadata, "Checks for incorrect formats of (TV Size) / (Game Ver.) / (Short Ver.) / (Cut Ver.) / (Sped Up Ver.) / etc in title.", CheckScope.BeatmapSet);

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateIncorrectMarker(this),
        };

        private readonly IEnumerable<MarkerCheck> markerChecks =
        [
            new MarkerCheck(@"(TV Size)", @"(?i)(tv (size|ver))"),
            new MarkerCheck(@"(Game Ver.)", @"(?i)(game (size|ver))"),
            new MarkerCheck(@"(Short Ver.)", @"(?i)(short (size|ver))"),
            new MarkerCheck(@"(Cut Ver.)", @"(?i)(?<!& )(cut (size|ver))"),
            new MarkerCheck(@"(Sped Up Ver.)", @"(?i)(?<!& )(sped|speed) ?up ver"),
            new MarkerCheck(@"(Nightcore Mix)", @"(?i)(?<!& )(nightcore|night core) (ver|mix)"),
            new MarkerCheck(@"(Sped Up & Cut Ver.)", @"(?i)(sped|speed) ?up (ver)? ?& cut (size|ver)"),
            new MarkerCheck(@"(Nightcore & Cut Ver.)", @"(?i)(nightcore|night core) (ver|mix)? ?& cut (size|ver)"),
        ];

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            string romanisedTitle = context.CurrentDifficulty.Playable.Metadata.Title;
            string unicodeTitle = context.CurrentDifficulty.Playable.Metadata.TitleUnicode;

            foreach (var check in markerChecks)
            {
                bool hasRomanisedTitle = unicodeTitle != romanisedTitle;

                if (check.AnyRegex.IsMatch(unicodeTitle) && !unicodeTitle.Contains(check.CorrectMarkerFormat, StringComparison.Ordinal))
                    yield return new IssueTemplateIncorrectMarker(this).Create("Title", check.CorrectMarkerFormat);

                if (hasRomanisedTitle && check.AnyRegex.IsMatch(romanisedTitle) && !romanisedTitle.Contains(check.CorrectMarkerFormat, StringComparison.Ordinal))
                    yield return new IssueTemplateIncorrectMarker(this).Create("Romanised title", check.CorrectMarkerFormat);
            }
        }

        private class MarkerCheck
        {
            public readonly string CorrectMarkerFormat;
            public readonly Regex AnyRegex;

            public MarkerCheck(string exact, string anyRegex)
            {
                CorrectMarkerFormat = exact;
                AnyRegex = new Regex(anyRegex, RegexOptions.Compiled);
            }
        }

        public class IssueTemplateIncorrectMarker : IssueTemplate
        {
            public IssueTemplateIncorrectMarker(ICheck check)
                : base(check, IssueType.Problem, "{0} field has an incorrect format of marker {1}")
            {
            }

            public Issue Create(string titleField, string correctMarkerFormat) => new Issue(this, titleField, correctMarkerFormat);
        }
    }
}

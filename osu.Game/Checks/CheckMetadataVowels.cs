// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Screens.Edit.Verify;
using osu.Game.Screens.Edit.Verify.Components;

namespace osu.Game.Checks
{
    public class CheckMetadataVowels : BeatmapCheck
    {
        private static readonly char[] vowels = { 'a', 'e', 'i', 'o', 'u' };

        public override CheckMetadata Metadata() => new CheckMetadata
        (
            category: CheckMetadata.CheckCategory.Metadata,
            description: "Metadata fields contain vowels"
        );

        public override IEnumerable<IssueTemplate> Templates() => new[]
        {
            templateArtistHasVowels
        };

        private IssueTemplate templateArtistHasVowels = new IssueTemplate
        (
            type: IssueTemplate.IssueType.Warning,
            unformattedMessage: "The {0} field \"{1}\" contains the vowel(s) {2}."
        );

        public override IEnumerable<Issue> Run(IBeatmap beatmap)
        {
            foreach (var issue in GetVowelIssues("artist", beatmap.Metadata.Artist))
                yield return issue;

            foreach (var issue in GetVowelIssues("unicode artist", beatmap.Metadata.ArtistUnicode))
                yield return issue;

            foreach (var issue in GetVowelIssues("title", beatmap.Metadata.Title))
                yield return issue;

            foreach (var issue in GetVowelIssues("unicode title", beatmap.Metadata.TitleUnicode))
                yield return issue;
        }

        private IEnumerable<Issue> GetVowelIssues(string fieldName, string fieldValue)
        {
            if (fieldValue == null)
                // Unicode fields can be null if same as respective romanized fields.
                yield break;

            List<char> matches = vowels.Where(c => fieldValue.ToLower().Contains(c)).ToList();

            if (!matches.Any())
                yield break;

            yield return new Issue(
                templateArtistHasVowels,
                fieldName, fieldValue, string.Join(", ", matches)
            );
        }
    }
}

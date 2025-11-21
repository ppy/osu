// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckInconsistentMetadata : ICheck
    {
        public CheckMetadata Metadata => new CheckMetadata(CheckCategory.Metadata, "Inconsistent metadata", CheckScope.BeatmapSet);

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateInconsistentTags(this),
            new IssueTemplateInconsistentOtherFields(this)
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            if (context.AllDifficulties.Count() <= 1)
                yield break;

            var referenceBeatmap = context.CurrentDifficulty.Playable;
            var referenceMetadata = referenceBeatmap.Metadata;

            // Define metadata fields to check
            var fieldsToCheck = new (string fieldName, Func<BeatmapMetadata, string> fieldSelector)[]
            {
                ("artist", m => m.Artist),
                ("unicode artist", m => m.ArtistUnicode),
                ("title", m => m.Title),
                ("unicode title", m => m.TitleUnicode),
                ("source", m => m.Source),
                ("creator", m => m.Author.Username)
            };

            foreach (var beatmap in context.OtherDifficulties)
            {
                var currentMetadata = beatmap.Playable.Metadata;

                // Check each metadata field for inconsistencies
                foreach ((string fieldName, var fieldSelector) in fieldsToCheck)
                {
                    string referenceField = fieldSelector(referenceMetadata);
                    string currentField = fieldSelector(currentMetadata);

                    if (referenceField != currentField)
                    {
                        yield return new IssueTemplateInconsistentOtherFields(this).Create(
                            fieldName,
                            referenceBeatmap.BeatmapInfo.DifficultyName,
                            beatmap.Playable.BeatmapInfo.DifficultyName,
                            referenceField,
                            currentField
                        );
                    }
                }

                // Special handling for tags
                if (referenceMetadata.Tags != currentMetadata.Tags)
                {
                    var differenceTags = referenceMetadata.Tags.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
                    differenceTags.SymmetricExceptWith(currentMetadata.Tags.Split(' ', StringSplitOptions.RemoveEmptyEntries));

                    string difference = string.Join(" ", differenceTags);

                    if (!string.IsNullOrEmpty(difference))
                    {
                        yield return new IssueTemplateInconsistentTags(this).Create(
                            referenceBeatmap.BeatmapInfo.DifficultyName,
                            beatmap.Playable.BeatmapInfo.DifficultyName,
                            difference
                        );
                    }
                }
            }
        }

        public class IssueTemplateInconsistentTags : IssueTemplate
        {
            public IssueTemplateInconsistentTags(ICheck check)
                : base(check, IssueType.Problem, "Inconsistent tags between \"{0}\" and \"{1}\", difference being \"{2}\".")
            {
            }

            public Issue Create(string referenceDifficulty, string currentDifficulty, string difference)
                => new Issue(this, referenceDifficulty, currentDifficulty, difference);
        }

        public class IssueTemplateInconsistentOtherFields : IssueTemplate
        {
            public IssueTemplateInconsistentOtherFields(ICheck check)
                : base(check, IssueType.Problem, "Inconsistent {0} fields between \"{1}\" and \"{2}\"; \"{3}\" and \"{4}\" respectively.")
            {
            }

            public Issue Create(string fieldName, string referenceDifficulty, string currentDifficulty, string referenceValue, string currentValue)
                => new Issue(this, fieldName, referenceDifficulty, currentDifficulty, referenceValue, currentValue);
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Globalization;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckInconsistentSettings : ICheck
    {
        public CheckMetadata Metadata => new CheckMetadata(CheckCategory.Settings, "Inconsistent settings", CheckScope.BeatmapSet);

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateInconsistentSetting(this, IssueType.Warning),
            new IssueTemplateInconsistentSetting(this, IssueType.Negligible),
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            var difficulties = context.BeatmapsetDifficulties;

            if (difficulties.Count <= 1)
                yield break;

            var referenceBeatmap = context.Beatmap;

            // Define fields to check
            var fieldsToCheck = new (IssueType issueType, string fieldName, Func<IBeatmap, string> fieldSelector)[]
            {
                (IssueType.Warning, "Audio lead-in", b => b.AudioLeadIn.ToString(CultureInfo.InvariantCulture)),
                (IssueType.Warning, "Countdown", b => b.Countdown.ToString()),
                (IssueType.Warning, "Countdown offset", b => b.CountdownOffset.ToString()),
                (IssueType.Warning, "Epilepsy warning", b => b.EpilepsyWarning.ToString()),
                (IssueType.Warning, "Letterbox during breaks", b => b.LetterboxInBreaks.ToString()),
                (IssueType.Warning, "Samples match playback rate", b => b.SamplesMatchPlaybackRate.ToString()),
                (IssueType.Warning, "Widescreen support", b => b.WidescreenStoryboard.ToString()),
                (IssueType.Negligible, "Tick Rate", b => b.Difficulty.SliderTickRate.ToString(CultureInfo.InvariantCulture)),
            };

            foreach (var beatmap in difficulties)
            {
                if (beatmap == referenceBeatmap)
                    continue;

                // Check each setting for inconsistencies
                foreach ((var issueType, string fieldName, var fieldSelector) in fieldsToCheck)
                {
                    string referenceField = fieldSelector(referenceBeatmap);
                    string currentField = fieldSelector(beatmap);

                    if (referenceField != currentField)
                    {
                        yield return new IssueTemplateInconsistentSetting(this, issueType).Create(
                            fieldName,
                            referenceBeatmap.BeatmapInfo.DifficultyName,
                            beatmap.BeatmapInfo.DifficultyName,
                            referenceField,
                            currentField
                        );
                    }
                }
            }
        }

        public class IssueTemplateInconsistentSetting : IssueTemplate
        {
            public IssueTemplateInconsistentSetting(ICheck check, IssueType issueType)
                : base(check, issueType, "Inconsistent \"{0}\" setting between \"{1}\" and \"{2}\"; \"{3}\" and \"{4}\" respectively.")
            {
            }

            public Issue Create(string fieldName, string referenceDifficulty, string currentDifficulty, string referenceValue, string currentValue)
                => new Issue(this, fieldName, referenceDifficulty, currentDifficulty, referenceValue, currentValue);
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
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
            new IssueTemplateInconsistentSetting(this, IssueType.Negligible)
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            if (context.AllDifficulties.Count() <= 1)
                return [];

            var referenceBeatmap = context.CurrentDifficulty.Playable;

            bool hasStoryboard = ResourcesCheckUtils.HasAnyStoryboardElementPresent(context.CurrentDifficulty.Working);

            var issues = new List<Issue>();

            // Define fields to check
            checkIssue(IssueType.Warning, "Audio lead-in", b => b.AudioLeadIn);
            checkIssue(IssueType.Warning, "Countdown", b => b.Countdown);
            checkIssue(IssueType.Warning, "Countdown offset", b => b.CountdownOffset);
            checkIssue(IssueType.Warning, "Epilepsy warning", b => b.EpilepsyWarning);
            checkIssue(IssueType.Warning, "Letterbox during breaks", b => b.LetterboxInBreaks);
            checkIssue(IssueType.Warning, "Samples match playback rate", b => b.SamplesMatchPlaybackRate);

            if (hasStoryboard)
                checkIssue(IssueType.Warning, "Widescreen support", b => b.WidescreenStoryboard);

            checkIssue(IssueType.Negligible, "Tick Rate", b => b.Difficulty.SliderTickRate);
            return issues;

            void checkIssue<T>(IssueType issueType, string fieldName, Func<IBeatmap, T> fieldSelector)
                where T : notnull // ideally this'd be `T : IEquatable<T>` but `Enum` doesn't implement it...
            {
                var referenceValue = fieldSelector(referenceBeatmap);

                foreach (var beatmap in context.OtherDifficulties)
                {
                    var currentValue = fieldSelector(beatmap.Playable);

                    if (!EqualityComparer<T>.Default.Equals(currentValue, referenceValue))
                    {
                        issues.Add(new IssueTemplateInconsistentSetting(this, issueType).Create(
                            fieldName,
                            referenceBeatmap.BeatmapInfo.DifficultyName,
                            beatmap.Playable.BeatmapInfo.DifficultyName,
                            referenceValue.ToString() ?? string.Empty,
                            currentValue.ToString() ?? string.Empty
                        ));
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

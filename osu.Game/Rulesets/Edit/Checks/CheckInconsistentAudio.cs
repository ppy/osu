// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckInconsistentAudio : ICheck
    {
        public CheckMetadata Metadata => new CheckMetadata(CheckCategory.Audio, "Inconsistent audio files", CheckScope.BeatmapSet);

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateInconsistentAudio(this)
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            if (context.AllDifficulties.Count() <= 1)
                yield break;

            var referenceBeatmap = context.CurrentDifficulty.Playable;
            string referenceAudioFile = referenceBeatmap.Metadata.AudioFile;

            foreach (var beatmap in context.OtherDifficulties)
            {
                string currentAudioFile = beatmap.Playable.Metadata.AudioFile;

                if (referenceAudioFile != currentAudioFile)
                {
                    yield return new IssueTemplateInconsistentAudio(this).Create(
                        string.IsNullOrEmpty(referenceAudioFile) ? "not set" : referenceAudioFile,
                        beatmap.Playable.BeatmapInfo.DifficultyName,
                        string.IsNullOrEmpty(currentAudioFile) ? "not set" : currentAudioFile
                    );
                }
            }
        }

        public class IssueTemplateInconsistentAudio : IssueTemplate
        {
            public IssueTemplateInconsistentAudio(ICheck check)
                : base(check, IssueType.Problem, "Inconsistent audio file between this difficulty ({0}) and \"{1}\" ({2}).")
            {
            }

            public Issue Create(string referenceAudio, string otherDifficulty, string otherAudio)
                => new Issue(this, referenceAudio, otherDifficulty, otherAudio);
        }
    }
}

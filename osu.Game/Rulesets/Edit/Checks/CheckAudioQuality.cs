// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckAudioQuality : ICheck
    {
        // This is a requirement as stated in the Ranking Criteria.
        // See https://osu.ppy.sh/wiki/en/Ranking_Criteria#rules.4
        private const int max_bitrate = 192;

        // "A song's audio file /.../ must be of reasonable quality. Try to find the highest quality source file available"
        // There not existing a version with a bitrate of 128 kbps or higher is extremely rare.
        private const int min_bitrate = 128;

        public CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.Audio, "Too high or low audio bitrate");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateTooHighBitrate(this),
            new IssueTemplateTooLowBitrate(this),
            new IssueTemplateNoBitrate(this)
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            string audioFile = context.Beatmap.Metadata.AudioFile;
            if (string.IsNullOrEmpty(audioFile))
                yield break;

            var track = context.WorkingBeatmap.Track;

            if (track?.Bitrate == null || track.Bitrate.Value == 0)
                yield return new IssueTemplateNoBitrate(this).Create();
            else if (track.Bitrate.Value > max_bitrate)
                yield return new IssueTemplateTooHighBitrate(this).Create(track.Bitrate.Value);
            else if (track.Bitrate.Value < min_bitrate)
                yield return new IssueTemplateTooLowBitrate(this).Create(track.Bitrate.Value);
        }

        public class IssueTemplateTooHighBitrate : IssueTemplate
        {
            public IssueTemplateTooHighBitrate(ICheck check)
                : base(check, IssueType.Problem, "The audio bitrate ({0} kbps) exceeds {1} kbps.")
            {
            }

            public Issue Create(int bitrate) => new Issue(this, bitrate, max_bitrate);
        }

        public class IssueTemplateTooLowBitrate : IssueTemplate
        {
            public IssueTemplateTooLowBitrate(ICheck check)
                : base(check, IssueType.Problem, "The audio bitrate ({0} kbps) is lower than {1} kbps.")
            {
            }

            public Issue Create(int bitrate) => new Issue(this, bitrate, min_bitrate);
        }

        public class IssueTemplateNoBitrate : IssueTemplate
        {
            public IssueTemplateNoBitrate(ICheck check)
                : base(check, IssueType.Error, "The audio bitrate could not be retrieved.")
            {
            }

            public Issue Create() => new Issue(this);
        }
    }
}

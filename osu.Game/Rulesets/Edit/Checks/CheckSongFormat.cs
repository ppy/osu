// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using ManagedBass;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckSongFormat : ICheck
    {
        public CheckMetadata Metadata => new CheckMetadata(CheckCategory.Audio, "Checks for song formats.", CheckScope.BeatmapSet);

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateFormatUnsupported(this),
            new IssueTemplateIncorrectFormat(this),
        };

        private IEnumerable<ChannelType> allowedFormats => new[]
        {
            ChannelType.MP3,
            ChannelType.OGG,
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            var beatmapSet = context.CurrentDifficulty.Playable.BeatmapInfo.BeatmapSet;
            var audioFile = beatmapSet?.GetFile(context.CurrentDifficulty.Playable.Metadata.AudioFile);

            if (beatmapSet == null) yield break;
            if (audioFile == null) yield break;

            var audioFormat = AudioCheckUtils.GetAudioFormatFromFile(context, context.CurrentDifficulty.Playable.Metadata.AudioFile);

            // If the format is not supported by BASS
            if (audioFormat == 0)
            {
                yield return new IssueTemplateFormatUnsupported(this).Create(audioFile.Filename);

                yield break;
            }

            if (!allowedFormats.Contains(audioFormat))
                yield return new IssueTemplateIncorrectFormat(this).Create(audioFile.Filename);
        }

        public class IssueTemplateFormatUnsupported : IssueTemplate
        {
            public IssueTemplateFormatUnsupported(ICheck check)
                : base(check, IssueType.Problem, "\"{0}\" may be corrupt or using a unsupported audio format. Use mp3 or ogg for the song's audio.")
            {
            }

            public Issue Create(string file) => new Issue(this, file);
        }

        public class IssueTemplateIncorrectFormat : IssueTemplate
        {
            public IssueTemplateIncorrectFormat(ICheck check)
                : base(check, IssueType.Problem, "\"{0}\" is using an incorrect format. Use mp3 or ogg for the song's audio.")
            {
            }

            public Issue Create(string file) => new Issue(this, file);
        }
    }
}

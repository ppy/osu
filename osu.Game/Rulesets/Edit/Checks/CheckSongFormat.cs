// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using ManagedBass;
using osu.Framework.Audio.Callbacks;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckSongFormat : ICheck
    {
        public CheckMetadata Metadata => new CheckMetadata(CheckCategory.Audio, "Checks for song formats.");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateFormatUnsupported(this),
            new IssueTemplateIncorrectFormat(this),
        };

        private IEnumerable<ChannelType> allowedFormats => new ChannelType[]
        {
            ChannelType.MP3,
            ChannelType.OGG,
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            var beatmapSet = context.Beatmap.BeatmapInfo.BeatmapSet;
            var audioFile = beatmapSet?.GetFile(context.Beatmap.Metadata.AudioFile);

            if (beatmapSet == null) yield break;
            if (audioFile == null) yield break;

            using (Stream data = context.WorkingBeatmap.GetStream(audioFile.File.GetStoragePath()))
            {
                if (data == null || data.Length <= 0) yield break;

                var fileCallbacks = new FileCallbacks(new DataStreamFileProcedures(data));
                int decodeStream = Bass.CreateStream(StreamSystem.NoBuffer, BassFlags.Decode, fileCallbacks.Callbacks, fileCallbacks.Handle);

                // If the format is not supported by BASS
                if (decodeStream == 0)
                {
                    yield return new IssueTemplateFormatUnsupported(this).Create(audioFile.Filename);

                    yield break;
                }

                var audioInfo = Bass.ChannelGetInfo(decodeStream);

                if (!allowedFormats.Contains(audioInfo.ChannelType))
                    yield return new IssueTemplateIncorrectFormat(this).Create(audioFile.Filename, audioInfo.ChannelType.ToString());

                Bass.StreamFree(decodeStream);
            }
        }

        public class IssueTemplateFormatUnsupported : IssueTemplate
        {
            public IssueTemplateFormatUnsupported(ICheck check)
                : base(check, IssueType.Problem, "\"{0}\" may be corrupt or using a unsupported audio format; Use mp3 or ogg for the song's audio.")
            {
            }

            public Issue Create(string file) => new Issue(this, file);
        }

        public class IssueTemplateIncorrectFormat : IssueTemplate
        {
            public IssueTemplateIncorrectFormat(ICheck check)
                : base(check, IssueType.Problem, "\"{0}\" is using a incorrect format ({1}); Use mp3 or ogg for the song's audio.")
            {
            }

            public Issue Create(string file, string format) => new Issue(this, file, format);
        }
    }
}

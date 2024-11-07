// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using ManagedBass;
using osu.Framework.Audio.Callbacks;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckHitsoundsFormat : ICheck
    {
        public CheckMetadata Metadata => new CheckMetadata(CheckCategory.Audio, "Checks for hitsound formats.");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateFormatUnsupported(this),
            new IssueTemplateIncorrectFormat(this),
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            var beatmapSet = context.Beatmap.BeatmapInfo.BeatmapSet;
            var audioFile = beatmapSet?.GetFile(context.Beatmap.Metadata.AudioFile);

            if (beatmapSet == null) yield break;

            foreach (var file in beatmapSet.Files)
            {
                if (audioFile != null && ReferenceEquals(file.File, audioFile.File)) continue;

                using (Stream data = context.WorkingBeatmap.GetStream(file.File.GetStoragePath()))
                {
                    if (data == null)
                        continue;

                    if (!AudioCheckUtils.HasAudioExtension(file.Filename) || !probablyHasAudioData(data))
                        continue;

                    var fileCallbacks = new FileCallbacks(new DataStreamFileProcedures(data));
                    int decodeStream = Bass.CreateStream(StreamSystem.NoBuffer, BassFlags.Decode, fileCallbacks.Callbacks, fileCallbacks.Handle);

                    // If the format is not supported by BASS
                    if (decodeStream == 0)
                    {
                        yield return new IssueTemplateFormatUnsupported(this).Create(file.Filename);

                        continue;
                    }

                    var audioInfo = Bass.ChannelGetInfo(decodeStream);

                    if ((audioInfo.ChannelType & ChannelType.Wave) == 0 && audioInfo.ChannelType != ChannelType.OGG)
                        yield return new IssueTemplateIncorrectFormat(this).Create(file.Filename);

                    Bass.StreamFree(decodeStream);
                }
            }
        }

        private bool probablyHasAudioData(Stream data) => data.Length > 100;

        public class IssueTemplateFormatUnsupported : IssueTemplate
        {
            public IssueTemplateFormatUnsupported(ICheck check)
                : base(check, IssueType.Problem, "\"{0}\" may be corrupt or using a unsupported audio format. Use wav or ogg for hitsounds.")
            {
            }

            public Issue Create(string file) => new Issue(this, file);
        }

        public class IssueTemplateIncorrectFormat : IssueTemplate
        {
            public IssueTemplateIncorrectFormat(ICheck check)
                : base(check, IssueType.Problem, "\"{0}\" is using a incorrect format. Use wav or ogg for hitsounds.")
            {
            }

            public Issue Create(string file) => new Issue(this, file);
        }
    }
}

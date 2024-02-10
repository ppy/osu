// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using ManagedBass;
using osu.Framework.Audio.Callbacks;
using osu.Game.Extensions;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckTooShortAudioFiles : ICheck
    {
        private const int ms_threshold = 25;
        private const int min_bytes_threshold = 100;

        public CheckMetadata Metadata => new CheckMetadata(CheckCategory.Audio, "Too short audio files");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateTooShort(this),
            new IssueTemplateBadFormat(this)
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            var beatmapSet = context.Beatmap.BeatmapInfo.BeatmapSet;

            if (beatmapSet != null)
            {
                foreach (var file in beatmapSet.Files)
                {
                    using (Stream data = context.WorkingBeatmap.GetStream(file.File.GetStoragePath()))
                    {
                        if (data == null)
                            continue;

                        var fileCallbacks = new FileCallbacks(new DataStreamFileProcedures(data));
                        int decodeStream = Bass.CreateStream(StreamSystem.NoBuffer, BassFlags.Decode | BassFlags.Prescan, fileCallbacks.Callbacks, fileCallbacks.Handle);

                        if (decodeStream == 0)
                        {
                            // If the file is not likely to be properly parsed by Bass, we don't produce Error issues about it.
                            // Image files and audio files devoid of audio data both fail, for example, but neither would be issues in this check.
                            if (AudioCheckUtils.HasAudioExtension(file.Filename) && probablyHasAudioData(data))
                                yield return new IssueTemplateBadFormat(this).Create(file.Filename);

                            continue;
                        }

                        long length = Bass.ChannelGetLength(decodeStream);
                        double ms = Bass.ChannelBytes2Seconds(decodeStream, length) * 1000;

                        // Extremely short audio files do not play on some soundcards, resulting in nothing being heard in-game for some users.
                        if (ms > 0 && ms < ms_threshold)
                            yield return new IssueTemplateTooShort(this).Create(file.Filename, ms);
                    }
                }
            }
        }

        private bool probablyHasAudioData(Stream data) => data.Length > min_bytes_threshold;

        public class IssueTemplateTooShort : IssueTemplate
        {
            public IssueTemplateTooShort(ICheck check)
                : base(check, IssueType.Problem, "\"{0}\" is too short ({1:0} ms), should be at least {2:0} ms.")
            {
            }

            public Issue Create(string filename, double ms) => new Issue(this, filename, ms, ms_threshold);
        }

        public class IssueTemplateBadFormat : IssueTemplate
        {
            public IssueTemplateBadFormat(ICheck check)
                : base(check, IssueType.Error, "Could not check whether \"{0}\" is too short (code \"{1}\").")
            {
            }

            public Issue Create(string filename) => new Issue(this, filename, Bass.LastError);
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using ManagedBass;
using osu.Framework.Audio.Callbacks;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Edit.Checks.Components
{
    public static class AudioCheckUtils
    {
        public static bool HasAudioExtension(string filename) => SupportedExtensions.AUDIO_EXTENSIONS.Contains(Path.GetExtension(filename).ToLowerInvariant());

        /// <summary>
        /// Gets the audio format (ChannelType) from a stream using BASS.
        /// </summary>
        /// <param name="data">The audio file stream.</param>
        /// <returns>The ChannelType of the audio, or <see cref="ChannelType.Unknown"/> if detection fails.</returns>
        public static ChannelType GetAudioFormat(Stream data)
        {
            if (data.Length <= 0)
                return ChannelType.Unknown;

            using (var fileCallbacks = new FileCallbacks(new DataStreamFileProcedures(data)))
            {
                int decodeStream = Bass.CreateStream(StreamSystem.NoBuffer, BassFlags.Decode, fileCallbacks.Callbacks, fileCallbacks.Handle);
                if (decodeStream == 0)
                    return ChannelType.Unknown;

                var audioInfo = Bass.ChannelGetInfo(decodeStream);
                Bass.StreamFree(decodeStream);

                return audioInfo.ChannelType;
            }
        }

        /// <summary>
        /// Gets the audio format for a specific file in a beatmapset.
        /// </summary>
        /// <param name="context">The beatmap verifier context.</param>
        /// <param name="filename">The filename to check.</param>
        /// <returns>The ChannelType of the audio file, or <see cref="ChannelType.Unknown"/> if detection fails.</returns>
        public static ChannelType GetAudioFormatFromFile(BeatmapVerifierContext context, string filename)
        {
            var beatmapSet = context.CurrentDifficulty.Playable.BeatmapInfo.BeatmapSet;
            var audioFile = beatmapSet?.GetFile(filename);

            if (beatmapSet == null || audioFile == null)
                return ChannelType.Unknown;

            using (Stream data = context.CurrentDifficulty.Working.GetStream(audioFile.File.GetStoragePath()))
                return GetAudioFormat(data);
        }
    }
}

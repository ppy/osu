// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Audio.Track;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Database;
using Realms;

namespace osu.Game.Audio
{
    /// <summary>
    /// Audio Normalization Data
    /// </summary>
    public class AudioNormalization : EmbeddedObject, IEquatable<AudioNormalization>
    {
        /// <summary>
        /// The target level for audio normalization
        /// https://en.wikipedia.org/wiki/EBU_R_128
        /// </summary>
        public const int TARGET_LEVEL = -14;

        /// <summary>
        /// The integrated (average) loudness of the audio
        /// </summary>
        public float? IntegratedLoudness { get; init; }

        public AudioNormalization()
        {
        }

        /// <summary>
        /// Get the audio normalization for a beatmap. The loudness normalization value is stored in the object under <see cref="IntegratedLoudness"/>
        /// </summary>
        /// <param name="beatmapInfo">The <see cref="BeatmapInfo"/> of the beatmap</param>
        /// <param name="beatmapSetInfo">The parent <see cref="BeatmapSetInfo"/> of the <see cref="BeatmapInfo"/> supplied</param>
        /// <param name="realmFileStore">The <see cref="RealmFileStore"/> to use in getting the full path of the audio file</param>
        public AudioNormalization(BeatmapInfo beatmapInfo, BeatmapSetInfo beatmapSetInfo, RealmFileStore realmFileStore)
        {
            string audiofile = beatmapInfo.Metadata.AudioFile;

            if (string.IsNullOrEmpty(audiofile))
            {
                Logger.Log("Audio file not found for " + beatmapInfo.Metadata.Title, LoggingTarget.Runtime, LogLevel.Error);
                return;
            }

            string? filepath = beatmapSetInfo.GetPathForFile(audiofile);

            if (string.IsNullOrEmpty(filepath))
            {
                Logger.Log("File path not found for " + audiofile, LoggingTarget.Runtime, LogLevel.Error);
                return;
            }

            using TrackLoudness loudness = new TrackLoudness(realmFileStore.Storage.GetStream(filepath));

            float? integratedLoudness = loudness.GetIntegratedLoudness();

            if (integratedLoudness == null)
            {
                Logger.Log("Failed to get loudness level for " + audiofile, LoggingTarget.Runtime, LogLevel.Error);
                return;
            }

            IntegratedLoudness = integratedLoudness;
        }

        /// <summary>
        /// Populate the audio normalization for every <see cref="BeatmapInfo"/> in a <see cref="BeatmapSetInfo"/>.
        /// </summary>
        /// <remarks>
        /// This takes a <see cref="BeatmapInfo"/> and applies the same normalization to every <see cref="BeatmapInfo"/> in the <see cref="BeatmapSetInfo"/> that has the same audio file as the given <see cref="BeatmapInfo"/> to avoid having to calculate the loudness for an audio file multiple times
        /// </remarks>
        /// <param name="beatmapInfo">The <see cref="BeatmapInfo"/> to clone from</param>
        /// <param name="beatmapSetInfo">The <see cref="BeatmapSetInfo"/> to populate</param>
        public void PopulateSet(BeatmapInfo beatmapInfo, BeatmapSetInfo beatmapSetInfo)
        {
            foreach (BeatmapInfo beatmap in beatmapSetInfo.Beatmaps)
            {
                if (beatmap.AudioNormalization == null && beatmap.AudioEquals(beatmapInfo))
                {
                    beatmap.AudioNormalization = new AudioNormalization
                    {
                        IntegratedLoudness = IntegratedLoudness
                    };
                }
            }
        }

        /// <summary>
        /// A volume offset that can be applied upon audio to reach <see cref="TARGET_LEVEL"/> (converted from integrated loudness).
        /// </summary>
        public double? IntegratedLoudnessInVolumeOffset => IntegratedLoudness != null ? TrackLoudness.ConvertToVolumeOffset(TARGET_LEVEL, (float)IntegratedLoudness) : null;

        /// <inheritdoc />
        public bool Equals(AudioNormalization? other) => other?.IntegratedLoudness != null && IntegratedLoudness != null && Math.Abs((float)(IntegratedLoudness - other.IntegratedLoudness)) < 0.0000001;
    }
}

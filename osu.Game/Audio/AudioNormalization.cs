// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;
using osu.Game.Database;
using Realms;

namespace osu.Game.Audio
{
    public class AudioNormalization : EmbeddedObject, IAudioNormalization, IEquatable<AudioNormalization>
    {
        /// <summary>
        /// The target level for audio normalization
        /// https://en.wikipedia.org/wiki/EBU_R_128
        /// </summary>
        public const int TARGET_LEVEL = -14;

        /// <summary>
        /// The integrated (average) loudness of the audio
        /// </summary>
        public float IntegratedLoudness { get; init; }

        public AudioNormalization()
        {
        }

        public AudioNormalization(BeatmapInfo beatmapInfo, BeatmapSetInfo beatmapSetInfo, RealmFileStore realmFileStore)
        {
            // Set default values
            IntegratedLoudness = 0;

            string audiofile = beatmapInfo.Metadata.AudioFile;

            if (string.IsNullOrEmpty(audiofile)) return;

            string? filepath = beatmapSetInfo.GetPathForFile(audiofile);

            if (string.IsNullOrEmpty(filepath)) return;

            filepath = realmFileStore.Storage.GetFullPath(filepath);

            BassAudioNormalization loudnessDetection = new BassAudioNormalization(filepath);
            IntegratedLoudness = loudnessDetection.IntegratedLoudness;
        }

        public bool IsDefault()
        {
            return IntegratedLoudness == 0;
        }

        internal BeatmapSetInfo PopulateSet(BeatmapInfo beatmapInfo, BeatmapSetInfo? beatmapSetInfo)
        {
            if (beatmapSetInfo == null) return beatmapSetInfo!;

            foreach (BeatmapInfo beatmap in beatmapSetInfo.Beatmaps)
            {
                if ((beatmap.AudioNormalization == null || beatmap.AudioNormalization.IsDefault()) && beatmap.AudioEquals(beatmapInfo))
                {
                    beatmap.AudioNormalization = new AudioNormalization
                    {
                        IntegratedLoudness = IntegratedLoudness
                    };
                }
            }

            return beatmapSetInfo;
        }

        /// <summary>
        /// Convert integrated loudness to a volume offset
        /// </summary>
        /// <param name="integratedLoudness"></param>
        /// <returns></returns>
        public static float IntegratedLoudnessToVolumeOffset(float integratedLoudness) => (float)Math.Pow(10, (TARGET_LEVEL - integratedLoudness) / 20);

        public bool Equals(IAudioNormalization? other) => other is AudioNormalization audioNormalization && Equals(audioNormalization);

        public bool Equals(AudioNormalization? other) => other != null && Math.Abs(IntegratedLoudness - other.IntegratedLoudness) < 0.0000001;
    }
}

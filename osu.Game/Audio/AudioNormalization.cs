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
        /// The adjustment needed to the volume to reach the target level (in bass volume language).
        /// https://www.un4seen.com/doc/#bass/BASS_FX_VOLUME_PARAM.html
        /// TODO: Remove this in favor of using IntegratedLoudness and doing the math in the audio engine rather than here.
        /// </summary>
        public float VolumeOffset { get; set; }

        /// <summary>
        /// The integrated (average) loudness of the audio
        /// </summary>
        public float IntegratedLoudness { get; set; }

        public AudioNormalization()
        {
        }

        public AudioNormalization(float volumeOffset, float integratedLoudness)
        {
            VolumeOffset = volumeOffset;
            IntegratedLoudness = integratedLoudness;
        }

        public AudioNormalization(BeatmapInfo beatmapInfo, BeatmapSetInfo beatmapSetInfo, RealmFileStore realmFileStore)
        {
            // Set default values
            VolumeOffset = 0;
            IntegratedLoudness = 0;

            string audiofile = beatmapInfo.Metadata.AudioFile;

            if (string.IsNullOrEmpty(audiofile)) return;

            string? filepath = beatmapSetInfo.GetPathForFile(audiofile);

            if (string.IsNullOrEmpty(filepath)) return;

            filepath = realmFileStore.Storage.GetFullPath(filepath);

            BassAudioNormalization loudnessDetection = new BassAudioNormalization(filepath);
            VolumeOffset = loudnessDetection.VolumeOffset;
            IntegratedLoudness = loudnessDetection.IntegratedLoudness;
        }

        public bool IsDefault()
        {
            return VolumeOffset == 0 && IntegratedLoudness == 0;
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
                        VolumeOffset = VolumeOffset,
                        IntegratedLoudness = IntegratedLoudness
                    };
                }
            }

            return beatmapSetInfo;
        }

        public bool Equals(IAudioNormalization? other) => other is AudioNormalization audioNormalization && Equals(audioNormalization);

        public bool Equals(AudioNormalization? other)
        {
            return VolumeOffset == other?.VolumeOffset && IntegratedLoudness == other.IntegratedLoudness;
        }
    }
}

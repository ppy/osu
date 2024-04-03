// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Beatmaps;
using osu.Game.Database;
using Realms;

namespace osu.Game.Audio
{
    public class AudioNormalization : EmbeddedObject, IAudioNormalization, IEquatable<IAudioNormalization>, IEquatable<AudioNormalization>
    {
        /// <summary>
        /// The adjustment needed to the volume to reach the target level (in bass volume language).
        /// https://www.un4seen.com/doc/#bass/BASS_FX_VOLUME_PARAM.html
        /// </summary>
        public float VolumeOffset { get; set; }

        /// <summary>
        /// The integrated (average) loudness of the audio
        /// </summary>
        public float IntegratedLoudness { get; set; }

        private AudioNormalization()
        {
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

        public bool isDefault()
        {
            return VolumeOffset == 0 && IntegratedLoudness == 0;
        }

        internal BeatmapSetInfo PopulateSet(BeatmapInfo beatmapInfo, BeatmapSetInfo beatmapSetInfo)
        {
            if (!beatmapSetInfo.IsNotNull()) return beatmapSetInfo;

            foreach (BeatmapInfo beatmap in beatmapSetInfo.Beatmaps)
            {
                if ((beatmap.AudioNormalization.IsNull() || beatmap.AudioNormalization.isDefault()) && beatmap.AudioEquals(beatmapInfo))
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

        public bool Equals(IAudioNormalization? other)
        {
            throw new NotImplementedException();
        }

        public bool Equals(AudioNormalization? other)
        {
            throw new NotImplementedException();
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using ManagedBass;
using ManagedBass.Fx;
using osu.Framework.Audio.Mixing;
using osu.Framework.Logging;
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
            string audiofile = beatmapInfo.Metadata.AudioFile;

            if (string.IsNullOrEmpty(audiofile)) return;

            string? filepath = beatmapSetInfo.GetPathForFile(audiofile);

            if (string.IsNullOrEmpty(filepath)) return;

            filepath = realmFileStore.Storage.GetFullPath(filepath);

            BassAudioNormalization loudnessDetection = new BassAudioNormalization(filepath);
            float integratedLoudness = loudnessDetection.IntegratedLoudness;

            if (Math.Abs(integratedLoudness - 1) < 0.0000001)
            {
                Logger.Log("Failed to get loudness level for " + audiofile, LoggingTarget.Runtime, LogLevel.Error);
                return;
            }

            IntegratedLoudness = integratedLoudness;
        }

        /// <summary>
        /// Populate the audio normalization for all beatmaps in a set.
        /// Essentially, this takes a beatmap and applies the same normalization to all beatmaps in the set that have the same audio file to avoid having to calculate the loudness for an audio file multiple times
        /// </summary>
        /// <param name="beatmapInfo">The BeatmapInfo to clone from</param>
        /// <param name="beatmapSetInfo">The BeatmapSetInfo to populate</param>
        public void PopulateSet(BeatmapInfo beatmapInfo, BeatmapSetInfo? beatmapSetInfo)
        {
            if (beatmapSetInfo == null) return;

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
        /// Convert integrated loudness to a volume offset
        /// </summary>
        /// <param name="integratedLoudness">The integrated loudness value</param>
        /// <returns>The volume offset needed to reach the target level</returns>
        public static float IntegratedLoudnessToVolumeOffset(float integratedLoudness) => (float)Math.Pow(10, (TARGET_LEVEL - integratedLoudness) / 20);

        /// <summary>
        /// Add the audio normalization effect to the mixer
        /// </summary>
        /// <param name="beatmapInfo">The BeatmapInfo to find the loudness of</param>
        /// <param name="mixer">The mixer to apply the effect on</param>
        public static void AddAudioNormalization(BeatmapInfo beatmapInfo, IAudioMixer mixer)
        {
            AudioNormalization? audioNormalizationModule = beatmapInfo.AudioNormalization;

            VolumeParameters volumeParameters = new VolumeParameters
            {
                fTarget = audioNormalizationModule?.IntegratedLoudness != null ? IntegratedLoudnessToVolumeOffset(audioNormalizationModule.IntegratedLoudness) : 0.8f,
                fCurrent = -1.0f,
                fTime = 0.3f,
                lCurve = 1,
                lChannel = FXChannelFlags.All
            };

            addFx(volumeParameters, mixer);

            Logger.Log("Normalization Status: " + (audioNormalizationModule != null ? $"on ({Math.Round(IntegratedLoudnessToVolumeOffset(audioNormalizationModule.IntegratedLoudness) * 100)}%)" : "off"));
        }

        private static void addFx(IEffectParameter effectParameter, IAudioMixer mixer)
        {
            IEffectParameter? effect = mixer.Effects.SingleOrDefault(e => e.FXType == effectParameter.FXType);

            if (effect != null)
            {
                int i = mixer.Effects.IndexOf(effect);
                mixer.Effects[i] = effectParameter;
            }
            else
            {
                mixer.Effects.Add(effectParameter);
            }
        }

        public bool Equals(IAudioNormalization? other) => other is AudioNormalization audioNormalization && Equals(audioNormalization);

        public bool Equals(AudioNormalization? other) => other != null && Math.Abs(IntegratedLoudness - other.IntegratedLoudness) < 0.0000001;
    }
}

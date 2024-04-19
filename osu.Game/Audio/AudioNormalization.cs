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
    /// <summary>
    /// Audio Normalization Manager
    /// </summary>
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

        /// <summary>
        /// Get the audio normalization for a beatmap. The loudness normalization value is stored in the object under <see cref="IntegratedLoudness"/>
        /// </summary>
        /// <param name="beatmapInfo">The <see cref="BeatmapInfo"/> of the beatmap</param>
        /// <param name="beatmapSetInfo">The parent <see cref="BeatmapSetInfo"/> of the <see cref="BeatmapInfo"/> supplied</param>
        /// <param name="realmFileStore">The <see cref="RealmFileStore"/> to use in getting the full path of the audio file</param>
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
        /// Populate the audio normalization for every <see cref="BeatmapInfo"/> in a <see cref="BeatmapSetInfo"/>.
        /// </summary>
        /// <remarks>
        /// This takes a <see cref="BeatmapInfo"/> and applies the same normalization to every <see cref="BeatmapInfo"/> in the <see cref="BeatmapSetInfo"/> that has the same audio file as the given <see cref="BeatmapInfo"/> to avoid having to calculate the loudness for an audio file multiple times
        /// </remarks>
        /// <param name="beatmapInfo">The <see cref="BeatmapInfo"/> to clone from</param>
        /// <param name="beatmapSetInfo">The <see cref="BeatmapSetInfo"/> to populate</param>
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
        /// <returns>The volume offset needed to reach <see cref="TARGET_LEVEL"/></returns>
        public static float IntegratedLoudnessToVolumeOffset(float integratedLoudness) => (float)Math.Pow(10, (TARGET_LEVEL - integratedLoudness) / 20);

        /// <summary>
        /// Get the loudness values of a <see cref="BeatmapInfo"/> and apply the audio normalization effect to an <see cref="IAudioMixer"/>
        /// </summary>
        /// <param name="beatmapInfo">The <see cref="BeatmapInfo"/> to get the loudness values of</param>
        /// <param name="mixer">The <see cref="IAudioMixer"/> to apply the effect on</param>
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

        /// <summary>
        /// Add an effect to a <see cref="IAudioMixer"/>, replacing it if it already exists
        /// </summary>
        /// <param name="effectParameter">The <see cref="IEffectParameter"/> to add to the <paramref name="mixer"/></param>
        /// <param name="mixer">The <see cref="IAudioMixer"/> to add the effect to</param>
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

        /// <inheritdoc />
        public bool Equals(IAudioNormalization? other) => other is AudioNormalization audioNormalization && Equals(audioNormalization);

        /// <inheritdoc />
        public bool Equals(AudioNormalization? other) => other != null && Math.Abs(IntegratedLoudness - other.IntegratedLoudness) < 0.0000001;
    }
}

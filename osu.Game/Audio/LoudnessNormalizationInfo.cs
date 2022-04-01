// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using JetBrains.Annotations;
using osu.Framework.Audio.Callbacks;
using osu.Framework.Audio.Track;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Stores;
using osu.Game.Utils;
using Realms;

namespace osu.Game.Audio
{
    [ExcludeFromDynamicCompile]
    public class LoudnessNormalizationInfo : EmbeddedObject, ILoudnessNormalizationInfo, IEquatable<LoudnessNormalizationInfo>, IDeepCloneable<LoudnessNormalizationInfo>
    {
        public float TrackGain { get; set; }
        public float PeakAmplitude { get; set; }

        [UsedImplicitly]
        public LoudnessNormalizationInfo()
        {
        }

        public LoudnessNormalizationInfo(float peakAmp, float gain)
        {
            TrackGain = gain;
            PeakAmplitude = peakAmp;
        }

        public LoudnessNormalizationInfo GenerateLoudnessNormalizationInfo(BeatmapInfo info, BeatmapSetInfo setInfo, RealmFileStore storage)
        {
            string audiofile = info.Metadata.AudioFile;

            if (!string.IsNullOrEmpty(audiofile))
            {
                string filePath = setInfo.GetPathForFile(info.Metadata.AudioFile);
                if (!string.IsNullOrEmpty(audiofile))
                    filePath = storage.Storage.GetFullPath(filePath);

                EbUr128LoudnessNormalization loudnessNormalization = new EbUr128LoudnessNormalization(filePath);
                PeakAmplitude = (float)loudnessNormalization.PeakAmp;
                TrackGain = (float)loudnessNormalization.Gain;

                return this;
            }
            else
            {
                return new LoudnessNormalizationInfo
                {
                    TrackGain = 0,
                    PeakAmplitude = 0,
                };
            }
        }

        internal BeatmapSetInfo PopulateSet(BeatmapInfo beatmapInfo, BeatmapSetInfo bSetInfo)
        {
            if (bSetInfo != null)
            {
                foreach (BeatmapInfo beatmap in bSetInfo.Beatmaps)
                {
                    if ((beatmap.LoudnessNormalizationInfo == null || beatmap.LoudnessNormalizationInfo.IsDefault()) && beatmap.AudioEquals(beatmapInfo))
                    {
                        beatmap.LoudnessNormalizationInfo = DeepClone();
                    }
                }
            }

            return bSetInfo;
        }

        public bool Equals(LoudnessNormalizationInfo other)
        {
            return TrackGain == other?.TrackGain && PeakAmplitude == other.PeakAmplitude;
        }

        public bool IsDefault()
        {
            return TrackGain == 0 && PeakAmplitude == 0;
        }

        public bool Equals(ILoudnessNormalizationInfo other) => other is LoudnessNormalizationInfo b && Equals(b);

        public LoudnessNormalizationInfo DeepClone()
        {
            return new LoudnessNormalizationInfo
            {
                TrackGain = TrackGain,
                PeakAmplitude = PeakAmplitude,
            };
        }
    }
}

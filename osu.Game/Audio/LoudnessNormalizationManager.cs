// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using ManagedBass;
using ManagedBass.Fx;
using osu.Framework.Audio;
using osu.Framework.Audio.Mixing;
using osu.Framework.Audio.Track;
using osu.Game.Beatmaps;
using osu.Game.Stores;

namespace osu.Game.Audio
{
    public class LoudnessNormalizationManager
    {
        private readonly ITrackStore trackStore;
        private readonly AudioMixer audioMixer;
        private readonly RealmFileStore storage;

        public LoudnessNormalizationManager(ITrackStore trackStore, AudioManager audioManager, RealmFileStore storage)
        {
            this.trackStore = trackStore;
            audioMixer = audioManager.TrackMixer;
            this.storage = storage;
        }

        public void AddLoudnessNormalization(ILoudnessNormalizationInfo info)
        {
            if (Math.Pow(10, (info.TrackGain / 20)) * info.PeakAmplitude >= 1)
            {
                CompressorParameters compParams = new CompressorParameters
                {
                    fAttack = 0.01f,
                    fGain = 0,
                    fRatio = 20,
                    fRelease = 200,
                    fThreshold = -20
                };
                addFx(compParams);
            }

            GainParameters gainParameters = new GainParameters
            {
                fCurrent = 1,
                fTarget = (float)Math.Pow(10, (info.TrackGain / 20)), //inverse of the loudness calculation as per ReplayGain 1.0 specs
                fTime = 0,
            };
            addFx(gainParameters);
        }

        private void addFx(IEffectParameter effectParameter)
        {
            IEffectParameter effect = audioMixer.Effects.SingleOrDefault(e => e.FXType == effectParameter.FXType);

            if (effect != null)
            {
                int i = audioMixer.Effects.IndexOf(effect);
                audioMixer.Effects[i] = effectParameter;
            }
            else
            {
                audioMixer.Effects.Add(effectParameter);
            }
        }

        public LoudnessNormalizationInfo GenerateLoudnessNormalizationInfo(BeatmapInfo info, BeatmapSetInfo setInfo)
        {
            string audiofile = info.Metadata.AudioFile;

            if (!string.IsNullOrEmpty(audiofile))
            {
                string filePath = "";

                try
                {
                    filePath = setInfo.GetPathForFile(info.Metadata.AudioFile);
                    if (!string.IsNullOrEmpty(audiofile))
                        filePath = storage.Storage.GetFullPath(filePath);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }

                EbUr128LoudnessNormalization loudnessNormalization = new EbUr128LoudnessNormalization(trackStore, filePath);
                LoudnessNormalizationInfo loudnessNormalizationInfo = new LoudnessNormalizationInfo((float)loudnessNormalization.PeakAmp, (float)loudnessNormalization.Gain);

                return loudnessNormalizationInfo;
            }
            else
            {
                return new LoudnessNormalizationInfo
                {
                    ID = Guid.NewGuid(),
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
                    if (beatmap.ReplayGainInfo.IsDefault() && beatmap.AudioEquals(beatmapInfo))
                    {
                        beatmap.ReplayGainInfo = beatmapInfo.ReplayGainInfo;
                    }
                }
            }

            return bSetInfo;
        }
    }
}

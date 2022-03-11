using System;
using System.IO;
using osu.Game.Beatmaps;
using ManagedBass;
using osu.Framework.Audio.Track;
using System.Data;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Audio.Callbacks;
using System.Threading.Tasks;
using ManagedBass.Fx;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Mixing;
using osu.Framework.Platform;
using osu.Game.Stores;

namespace osu.Game.Audio
{
    public class ReplayGainManager
    {
        private ITrackStore trackStore;
        private AudioMixer audioMixer;
        private RealmFileStore storage;

        public ReplayGainManager(ITrackStore trackStore, AudioManager audioManager, RealmFileStore storage)
        {
            this.trackStore = trackStore;
            audioMixer = audioManager.TrackMixer;
            this.storage = storage;
        }

        public void AddReplayGain(IReplayGainInfo info)
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
                AddFx(compParams);
            }

            GainParameters gainParameters = new GainParameters
            {
                fCurrent = 1,
                fTarget = (float)Math.Pow(10, (info.TrackGain / 20)), //inverse of the loudness calculation as per ReplayGain 1.0 specs
                fTime = 0,
            };
            AddFx(gainParameters);
        }

        public void AddFx(IEffectParameter effectParameter)
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

        public ReplayGainInfo generateReplayGainInfo(BeatmapInfo info, BeatmapSetInfo setInfo)
        {
            string audiofile = info?.Metadata?.AudioFile;
            if (audiofile != null && audiofile != "")
            {
                string filePath = "";
                try
                {
                    filePath = setInfo.GetPathForFile(info?.Metadata?.AudioFile);
                    if (filePath != null)
                        filePath = storage.Storage.GetFullPath(filePath);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }

                ReplayGainImplementation replayGainImplementation = new ReplayGainImplementation(trackStore, filePath);
                ReplayGainInfo replayGain = new ReplayGainInfo((float)replayGainImplementation.PeakAmp, (float)replayGainImplementation.Gain);

                return replayGain;
            }
            else
            {
                return new ReplayGainInfo()
                {
                    ID = Guid.NewGuid(),
                    TrackGain = 0,
                    PeakAmplitude = 0,
                };
            }
        }

        internal BeatmapSetInfo PopulateSet(BeatmapInfo beatmapInfo, BeatmapSetInfo bSetInfo)
        {
            if(bSetInfo != null)
            {
                foreach (BeatmapInfo beatmap in bSetInfo.Beatmaps)
                {
                    if ((beatmap.ReplayGainInfo == null || beatmap.ReplayGainInfo.isDefault()) && beatmap.AudioEquals(beatmapInfo))
                    {
                        beatmap.ReplayGainInfo = beatmapInfo.ReplayGainInfo;
                    }
                }
            }

            return bSetInfo;
        }
    }
}

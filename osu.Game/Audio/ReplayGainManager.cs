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

namespace osu.Game.Audio
{
    public class ReplayGainManager
    {
        private ITrackStore trackStore;
        private AudioMixer audioMixer;

        public ReplayGainManager(ITrackStore trackStore, AudioManager audioManager)
        {
            this.trackStore = trackStore;
            audioMixer = audioManager.TrackMixer;
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
            string filePath = "";
            try
            {
                filePath = setInfo.GetPathForFile(info?.Metadata?.AudioFile);
                string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"osu", @"files");
                filePath = Path.Combine(folderPath, filePath);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            ReplayGainImplementation replayGainImplementation = new ReplayGainImplementation(trackStore, filePath);
            ReplayGainInfo replayGain = new ReplayGainInfo((float)replayGainImplementation.PeakAmp, (float)replayGainImplementation.Gain);

            return replayGain;
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

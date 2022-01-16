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

namespace osu.Game.Audio
{
    public class ReplayGainManager
    {
        public const float CURR_REPLAYGAIN_VER = 0.1f;
        private ITrackStore trackStore;
        private ReplayGainStore replayGainStore;

        public ReplayGainManager(ReplayGainStore replayGainStore, ITrackStore trackStore)
        {
            this.trackStore = trackStore;
            this.replayGainStore = replayGainStore;
        }

        public ReplayGainInfo GetInfo(int ID)
        {
            ReplayGainInfo replayGainInfo = replayGainStore.ConsumableItems.Where(s => s.ID == ID).FirstOrDefault();

            return replayGainInfo;
        }

        public Task saveReplayGainInfo(ReplayGainInfo replayGainInfo, BeatmapInfo beatmap)
        {
            replayGainStore.Add(replayGainInfo, beatmap);
            return Task.CompletedTask;
        }

        public void AddReplayGain(ReplayGainInfo info)
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
                replayGainStore.AddFx(compParams);
            }

            GainParameters gainParameters = new GainParameters
            {
                fCurrent = 1,
                fTarget = (float)Math.Pow(10, (info.TrackGain / 20)), //inverse of the loudness calculation as per ReplayGain 1.0 specs
                fTime = 0,
            };
            replayGainStore.AddFx(gainParameters);
        }

        public ReplayGainInfo generateReplayGainInfo(BeatmapInfo info, BeatmapSetInfo setInfo)
        {
            ReplayGainInfo replayGain = new ReplayGainInfo();
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
            replayGain.PeakAmplitude = (float)replayGainImplementation.PeakAmp;
            replayGain.TrackGain = (float)replayGainImplementation.Gain;
            replayGain.Version = CURR_REPLAYGAIN_VER;
            return replayGain;
        }

        internal BeatmapSetInfo PopulateSet(BeatmapInfo beatmapInfo, BeatmapSetInfo bSetInfo)
        {
            if(bSetInfo != null)
            {
                foreach (BeatmapInfo beatmap in bSetInfo.Beatmaps)
                {
                    if (beatmap.ReplayGainInfoID == 0 && beatmap.AudioEquals(beatmapInfo))
                    {
                        beatmap.ReplayGainInfoID = beatmapInfo.ReplayGainInfoID;
                        beatmap.ReplayGainInfo = beatmapInfo.ReplayGainInfo;
                    }
                }
            }

            return bSetInfo;
        }
    }
}

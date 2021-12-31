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

namespace osu.Game.Audio
{
    public class AudioTest
    {
        public const float CURR_REPLAYGAIN_VER = 0.1f;

        private string filePath = "";
        private ITrackStore trackStore;
        private FileCallbacks fileCallbacks;
        private ReplayGainStore replayGainStore;

        public AudioTest(ReplayGainStore replayGainStore, ITrackStore trackStore)
        {
            this.trackStore = trackStore;
            this.replayGainStore = replayGainStore;
        }

        public ReplayGainInfo GetInfo(int ID)
        {
            ReplayGainInfo replayGainInfo = replayGainStore.ConsumableItems.Where(s => s.ID == ID).FirstOrDefault();

            return replayGainInfo;
        }

        public void changeTrack(BeatmapInfo info, BeatmapSetInfo setInfo)
        {
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

        public void getAudioFileStream(ITrackStore storeParameter)
        {
            trackStore = storeParameter;
            if(trackStore != null && File.Exists(filePath))
            {
                try
                {
                    Stream fileData = trackStore.GetStream(filePath);
                    fileData.Seek(0, SeekOrigin.Begin);
                    fileCallbacks = new FileCallbacks(new DataStreamFileProcedures(fileData));
                    int decodeStream = Bass.CreateStream(StreamSystem.NoBuffer, BassFlags.Decode | BassFlags.Float, fileCallbacks.Callbacks, fileCallbacks.Handle);
                    Bass.ChannelGetInfo(decodeStream, out ChannelInfo info);
                    long length = Bass.ChannelGetLength(decodeStream);
                    int samplesPerPoint = (int)(info.Frequency * 0.001f * info.Channels);
                    int bytesPerPoint = samplesPerPoint * TrackBass.BYTES_PER_SAMPLE;
                    int bytesPerIteration = bytesPerPoint * 100000;
                    float[] sampleBuffer = new float[bytesPerIteration / TrackBass.BYTES_PER_SAMPLE];
                    length = Bass.ChannelGetData(decodeStream, sampleBuffer, bytesPerIteration);
                    
                    if (decodeStream == 0 || length < 0)
                    {
                        string error = Bass.LastError.ToString();
                    }
                    else
                    {
                        testGetData(sampleBuffer);
                    }
                    fileData.Close();
                }
                catch(Exception e)
                {
                    Debug.WriteLine(e);
                    return;
                }

                //replayGainInfo = generateReplayGainInfo(11, 11, 0);
                //saveReplayGainInfo();
            }
        }

        public ReplayGainInfo generateReplayGainInfo(float trackGain, float peakAmplitude, float version)
        {
            ReplayGainInfo replayGain = new ReplayGainInfo
            {
                TrackGain = trackGain,
                PeakAmplitude = peakAmplitude,
                Version = version,
                DeletePending = false,
            };
            return replayGain;
        }

        public ReplayGainInfo generateReplayGainInfo(BeatmapInfo info, BeatmapSetInfo setInfo)
        {
            ReplayGainInfo replayGain = new ReplayGainInfo();

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

        private bool testGetData(float[] data)
        {
            return true;
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
                    }
                }
            }

            return bSetInfo;
        }
    }
}

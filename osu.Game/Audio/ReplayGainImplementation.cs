using ManagedBass;
using ManagedBass.Fx;
using osu.Framework.Audio.Callbacks;
using osu.Framework.Audio.Track;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace osu.Game.Audio
{
    public class ReplayGainImplementation
    {
        private const double reference_loudness = -24.082881927490234;

        public double PeakAmp { get; private set; }
        public double Gain { get; private set; }
        public ReplayGainImplementation(ITrackStore storeParameter, string filePath)
        {
            using (Stream fileData = storeParameter.GetStream(filePath))
            {
                fileData.Seek(0, SeekOrigin.Begin);
                FileCallbacks fileCallbacks = new FileCallbacks(new DataStreamFileProcedures(fileData));
                calculateValues(fileCallbacks);
            }
        }

        private void calculateValues(FileCallbacks fileCallbacks)
        {
            //load the track and read it's info
            int decodeStream = Bass.CreateStream(StreamSystem.NoBuffer, BassFlags.Decode | BassFlags.Float, fileCallbacks.Callbacks, fileCallbacks.Handle);
            Bass.ChannelGetInfo(decodeStream, out ChannelInfo info);
            long length;

            //50 ms window to calculate rms
            int saplesPerWindow = (int)(info.Frequency * 0.05f * info.Channels);

            //create a 50ms buffer and read the first segment of the track
            float[] sampleBuffer = new float[saplesPerWindow];
            length = Bass.ChannelGetData(decodeStream, sampleBuffer, saplesPerWindow * TrackBass.BYTES_PER_SAMPLE);

            List<double> listRms = new List<double>();
            double min = 1;
            double max = -1;

            //array to apply the yulewalk filter
            double[] pastX = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            double[] pastZ = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            //yulewalk filter coeffs
            double[] yuleA;
            double[] yuleB;

            //array for the high-pass filter
            double[] pastZlow = { 0, 0 };
            double[] pastY = { 0, 0 };

            //high-pass coeffs
            double[] highPassA;
            double[] highPassB;

            if (info.Frequency <= 46050)
            {

                yuleA = new double[]{ 3.47845948550071, -6.36317777566148, 8.54751527471874,
                -9.47693607801280, 8.81498681370155, -6.85401540936998,
                4.39470996079559, -2.19611684890774, 0.75104302451432,
                -0.13149317958808 };

                yuleB = new double[]{ 0.05418656406430, -0.02911007808948, -0.00848709379851,
                -0.00851165645469, -0.00834990904936, 0.02245293253339,
                -0.02596338512915, 0.01624864962975, -0.00240879051584,
                0.00674613682247, -0.00187763777362 };

                highPassA = new double[] { 1.96977855582618, -0.97022847566350 };
                highPassB = new double[] { 0.98500175787242, -1.97000351574484, 0.98500175787242 };

            }
            else
            {

                yuleA = new double[]{ 3.84664617118067, -7.81501653005538, 11.34170355132042,
                -13.05504219327545, 12.28759895145294, -9.48293806319790,
                5.87257861775999, -2.75465861874613, 0.86984376593551,
                -0.13919314567432 };

                yuleB = new double[]{ 0.03857599435200, -0.02160367184185, -0.00123395316851,
                -0.00009291677959, -0.01655260341619, 0.02161526843274,
                -0.02074045215285, 0.00594298065125, 0.00306428023191,
                0.00012025322027, 0.00288463683916 };

                highPassA = new double[] { 1.97223372919527, -0.97261396931306 };
                highPassB = new double[] { 0.98621192462708, -1.97242384925416, 0.98621192462708 };

            }

            //read the full track
            while (length > 0)
            {
                double squared = 0;

                foreach (float sample in sampleBuffer)
                {
                    //apply the yulewalk filter to the sample
                    double yuleSample = yuleB[0] * sample;

                    for (int i = pastX.Length - 1; i >= 0; i--)
                    {
                        yuleSample += yuleB[i + 1] * pastX[i] + yuleA[i] * pastZ[i];

                        if (i == 0)
                        {
                            pastX[i] = sample;
                            pastZ[i] = yuleSample;
                        }
                        else
                        {
                            pastX[i] = pastX[i - 1];
                            pastZ[i] = pastZ[i - 1];
                        }
                    }

                    //apply the high-pass filter to the sample
                    double tempsample = highPassB[0] * yuleSample;

                    for (int i = pastY.Length - 1; i >= 0; i--)
                    {

                        tempsample += highPassB[i + 1] * pastZlow[i] + highPassA[i] * pastY[i];

                        if (i == 0)
                        {
                            pastZlow[i] = yuleSample;
                            pastY[i] = tempsample;
                        }
                        else
                        {
                            pastZlow[i] = pastZlow[i - 1];
                            pastY[i] = pastY[i - 1];
                        }
                    }

                    squared += tempsample * tempsample; //for the rms calc
                    if (sample > max)
                    {
                        max = sample;
                    }
                    if (sample < min)
                    {
                        min = sample;
                    }
                }

                //rms for the 50ms segment
                double temp = Math.Sqrt(squared / sampleBuffer.Length);
                if (temp != 0) listRms.Add(temp);

                //read next segment
                length = Bass.ChannelGetData(decodeStream, sampleBuffer, saplesPerWindow * TrackBass.BYTES_PER_SAMPLE);
            }

            Bass.StreamFree(decodeStream);

            //peak to peak range and peak amplitude
            double range = max - min;
            PeakAmp = max > Math.Abs(min) ? max : Math.Abs(min);

            double[] dbRms = new double[listRms.Count];

            //db conversion for the loudness selection (ReplayGain 1.0)
            for (int i = 0; i < listRms.Count; i++)
            {
                dbRms[i] = 20 * Math.Log10(2 * listRms[i] / range);
            }

            //find the 95th percentile as perceived loudness (ReplayGain 1.0)
            Array.Sort(dbRms);
            int index = (int)(0.95f * dbRms.Length);
            double loudness = dbRms[index];

            Gain = reference_loudness - loudness;
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using ManagedBass;
using osu.Framework.Audio.Callbacks;
using osu.Framework.Audio.Track;

namespace osu.Game.Audio
{
    public class ReplayGainImplementation
    {
        private const double reference_loudness = -24.082881927490234;

        public double PeakAmp { get; private set; }
        public double Gain { get; private set; }

        public ReplayGainImplementation(ITrackStore storeParameter, string filePath)
        {
            if (filePath != null)
            {
                using (Stream fileData = storeParameter.GetStream(filePath))
                {
                    fileData.Seek(0, SeekOrigin.Begin);
                    FileCallbacks fileCallbacks = new FileCallbacks(new DataStreamFileProcedures(fileData));
                    calculateValues(fileCallbacks);
                }
            }
            else
            {
                PeakAmp = 0;
                Gain = 0;
            }
        }

        private void calculateValues(FileCallbacks fileCallbacks)
        {
            //load the track and read it's info
            int decodeStream = Bass.CreateStream(StreamSystem.NoBuffer, BassFlags.Decode | BassFlags.Float, fileCallbacks.Callbacks, fileCallbacks.Handle);
            Bass.ChannelGetInfo(decodeStream, out ChannelInfo info);

            //50 ms window to calculate rms
            int saplesPerWindow = (int)(info.Frequency * 0.05f * info.Channels);
            int bytesPerWindow = saplesPerWindow * TrackBass.BYTES_PER_SAMPLE;

            //create a 50ms buffer and read the first segment of the track
            float[] sampleBuffer = new float[saplesPerWindow];
            long length = Bass.ChannelGetData(decodeStream, sampleBuffer, saplesPerWindow * TrackBass.BYTES_PER_SAMPLE);

            List<double> listRms = new List<double>();
            double min = 1;
            double max = -1;

            //Variables to apply the yulewalk filter
            double pastX0 = 0;
            double pastX1 = 0;
            double pastX2 = 0;
            double pastX3 = 0;
            double pastX4 = 0;
            double pastX5 = 0;
            double pastX6 = 0;
            double pastX7 = 0;
            double pastX8 = 0;
            double pastX9 = 0;

            double pastZ0 = 0;
            double pastZ1 = 0;
            double pastZ2 = 0;
            double pastZ3 = 0;
            double pastZ4 = 0;
            double pastZ5 = 0;
            double pastZ6 = 0;
            double pastZ7 = 0;
            double pastZ8 = 0;
            double pastZ9 = 0;

            //yulewalk filter coeffs
            double[] yuleA;
            double[] yuleB;

            //Variables for the high-pass filter
            double pastZlow0 = 0;
            double pastZlow1 = 0;

            double pastY0 = 0;
            double pastY1 = 0;

            //high-pass coeffs
            double[] highPassA;
            double[] highPassB;

            if (info.Frequency <= 46050)
            {
                yuleA = new[]
                {
                    3.47845948550071, -6.36317777566148, 8.54751527471874,
                    -9.47693607801280, 8.81498681370155, -6.85401540936998,
                    4.39470996079559, -2.19611684890774, 0.75104302451432,
                    -0.13149317958808
                };

                yuleB = new[]
                {
                    0.05418656406430, -0.02911007808948, -0.00848709379851,
                    -0.00851165645469, -0.00834990904936, 0.02245293253339,
                    -0.02596338512915, 0.01624864962975, -0.00240879051584,
                    0.00674613682247, -0.00187763777362
                };

                highPassA = new[]
                {
                    1.96977855582618, -0.97022847566350
                };

                highPassB = new[]
                {
                    0.98500175787242, -1.97000351574484, 0.98500175787242
                };
            }
            else
            {
                yuleA = new[]
                {
                    3.84664617118067, -7.81501653005538, 11.34170355132042,
                    -13.05504219327545, 12.28759895145294, -9.48293806319790,
                    5.87257861775999, -2.75465861874613, 0.86984376593551,
                    -0.13919314567432
                };

                yuleB = new[]
                {
                    0.03857599435200, -0.02160367184185, -0.00123395316851,
                    -0.00009291677959, -0.01655260341619, 0.02161526843274,
                    -0.02074045215285, 0.00594298065125, 0.00306428023191,
                    0.00012025322027, 0.00288463683916
                };

                highPassA = new[]
                {
                    1.97223372919527, -0.97261396931306
                };

                highPassB = new[]
                {
                    0.98621192462708, -1.97242384925416, 0.98621192462708
                };
            }

            //read the full track
            while (length > 0)
            {
                double squared = 0;

                for (int s = 0; s < sampleBuffer.Length; s++)
                {
                    double yuleSample = yuleB[0] * sampleBuffer[s] + yuleB[1] * pastX0 + yuleB[2] * pastX1 + yuleB[3] * pastX2 + yuleB[4] * pastX3 + yuleB[5] * pastX4 + yuleB[6] * pastX5 + yuleB[7] * pastX6 + yuleB[8] * pastX7 + yuleB[9] * pastX8 + yuleB[10] * pastX9 + yuleA[0] * pastZ0 + yuleA[1] * pastZ1 + yuleA[2] * pastZ2 + yuleA[3] * pastZ3 + yuleA[4] * pastZ4 + yuleA[5] * pastZ5 + yuleA[6] * pastZ6 + yuleA[7] * pastZ7 + yuleA[8] * pastZ8 + yuleA[9] * pastZ9;

                    pastX9 = pastX8;
                    pastZ9 = pastZ8;
                    pastX8 = pastX7;
                    pastZ8 = pastZ7;
                    pastX7 = pastX6;
                    pastZ7 = pastZ6;
                    pastX6 = pastX5;
                    pastZ6 = pastZ5;
                    pastX5 = pastX4;
                    pastZ5 = pastZ4;
                    pastX4 = pastX3;
                    pastZ4 = pastZ3;
                    pastX3 = pastX2;
                    pastZ3 = pastZ2;
                    pastX2 = pastX1;
                    pastZ2 = pastZ1;
                    pastX1 = pastX0;
                    pastZ1 = pastZ0;
                    pastX0 = sampleBuffer[s];
                    pastZ0 = yuleSample;

                    //apply the high-pass filter to the sample
                    double tempsample = highPassB[0] * yuleSample + highPassB[1] * pastZlow0 + highPassB[2] * pastZlow1 + highPassA[0] * pastY0 + highPassA[1] * pastY1;

                    pastZlow1 = pastZlow0;
                    pastY1 = pastY0;
                    pastZlow0 = yuleSample;
                    pastY0 = tempsample;

                    squared += tempsample * tempsample; //for the rms calc

                    if (sampleBuffer[s] > max)
                    {
                        max = sampleBuffer[s];
                    }

                    if (sampleBuffer[s] < min)
                    {
                        min = sampleBuffer[s];
                    }
                }

                //rms for the 50ms segment
                double temp = Math.Sqrt(squared / sampleBuffer.Length);

                if (temp != 0) listRms.Add(temp);

                //read next segment
                length = Bass.ChannelGetData(decodeStream, sampleBuffer, bytesPerWindow);
            }

            Bass.StreamFree(decodeStream);

            if (listRms.Count != 0)
            {
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
            else
            {
                PeakAmp = 0;
                Gain = 0;
            }
        }
    }
}

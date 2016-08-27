//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.IO;
using System.Runtime.InteropServices;
using ManagedBass;
using ManagedBass.Fx;
using osu.Framework.Configuration;
using osu.Framework.IO;
using OpenTK;

namespace osu.Framework.Audio.Track
{
    public class AudioTrackBass : AudioTrack
    {
        private float initialFrequency;

        private int audioStreamPrefilter;

        private AsyncBufferStream dataStream;

        public bool Looping { get; private set; }

        /// <summary>
        /// Should this track only be used for preview purposes? This suggests it has not yet been fully loaded.
        /// </summary>
        public bool Preview { get; private set; }

        /// <summary>
        /// The handle for this track, if there is one.
        /// </summary>
        private int activeStream;

        //must keep a reference to this else it will be garbage collected early.
        private DataStreamFileProcedures procs;

        public AudioTrackBass(Stream data, bool quick = false, bool loop = false)
        {
            Preview = quick;
            Looping = loop;

            BassFlags flags = Preview ? 0 : (BassFlags.Decode | BassFlags.Prescan);

            if (data == null)
                throw new Exception(@"Data couldn't be loaded!");
            else
            {
                //encapsulate incoming stream with async buffer if it isn't already.
                dataStream = data as AsyncBufferStream;
                if (dataStream == null) dataStream = new AsyncBufferStream(data, quick ? 8 : -1);

                procs = new DataStreamFileProcedures(dataStream);

                audioStreamPrefilter = Bass.CreateStream(StreamSystem.NoBuffer, flags, procs.BassProcedures, IntPtr.Zero);
            }

            if (Preview)
                activeStream = audioStreamPrefilter;
            else
            {
                activeStream = BassFx.TempoCreate(audioStreamPrefilter, BassFlags.Decode);
                activeStream = BassFx.ReverseCreate(activeStream, 5f, BassFlags.Default);
                
                Bass.ChannelSetAttribute(activeStream, ChannelAttribute.TempoUseQuickAlgorithm, 1);
                Bass.ChannelSetAttribute(activeStream, ChannelAttribute.TempoOverlapMilliseconds, 4);
                Bass.ChannelSetAttribute(activeStream, ChannelAttribute.TempoSequenceMilliseconds, 30);
            }

            Length = (Bass.ChannelBytes2Seconds(activeStream, Bass.ChannelGetLength(activeStream)) * 1000);
            Bass.ChannelGetAttribute(activeStream, ChannelAttribute.Frequency, out initialFrequency);
        }

        public override void Reset()
        {
            Stop();
            Seek(0);
            Volume.Value = 1;
            base.Reset();
        }

        protected override void Dispose(bool disposing)
        {
            if (activeStream != 0) Bass.ChannelStop(activeStream);

            if (audioStreamPrefilter != 0) Bass.StreamFree(audioStreamPrefilter);

            activeStream = 0;
            audioStreamPrefilter = 0;

            dataStream?.Dispose();
            dataStream = null;

            base.Dispose(disposing);
        }

        public override bool IsDummyDevice => false;

        public override void Stop()
        {
            if (IsRunning)
                togglePause();
        }

        private bool togglePause()
        {
            //if (IsDisposed) return false;

            if (PlaybackState.Playing == Bass.ChannelIsActive(activeStream))
            {
                Bass.ChannelPause(activeStream);
                return true;
            }
            else
            {
                Bass.ChannelPlay(activeStream, false);
                return false;
            }
        }

        int direction = 0;

        private void setDirection(bool reverse)
        {
            int newDirection = reverse ? -1 : 1;

            if (direction == newDirection) return;

            direction = newDirection;

            Bass.ChannelSetAttribute(activeStream, ChannelAttribute.ReverseDirection, direction);
        }

        public override void Start()
        {
            Update();
            Bass.ChannelPlay(activeStream);
        }

        public override bool Seek(double seek)
        {
            double clamped = MathHelper.Clamp(seek, 0, Length);

            if (clamped != CurrentTime)
            {
                long pos = Bass.ChannelSeconds2Bytes(activeStream, clamped / 1000d);
                Bass.ChannelSetPosition(activeStream, pos);
            }

            return clamped == seek;
        }

        public override double CurrentTime => Bass.ChannelBytes2Seconds(activeStream, Bass.ChannelGetPosition(activeStream)) * 1000;

        public override bool IsRunning => Bass.ChannelIsActive(activeStream) == PlaybackState.Playing;

        protected override void OnStateChanged(object sender, EventArgs e)
        {
            base.OnStateChanged(sender, e);

            setDirection(FrequencyCalculated.Value < 0);

            Bass.ChannelSetAttribute(activeStream, ChannelAttribute.Volume, VolumeCalculated);
            Bass.ChannelSetAttribute(activeStream, ChannelAttribute.Pan, BalanceCalculated);
            Bass.ChannelSetAttribute(activeStream, ChannelAttribute.Frequency, bassFreq);
            Bass.ChannelSetAttribute(activeStream, ChannelAttribute.Tempo, (Math.Abs(Tempo) - 1) * 100);
        }

        int bassFreq => (int)MathHelper.Clamp(Math.Abs(initialFrequency * FrequencyCalculated), 100, 100000);

        public override double Rate => bassFreq / initialFrequency * Tempo * direction;

        public override int? Bitrate => (int)Bass.ChannelGetAttribute(activeStream, ChannelAttribute.Bitrate);

        private class DataStreamFileProcedures
        {
            private byte[] readBuffer = new byte[32768];

            private AsyncBufferStream dataStream;

            public FileProcedures BassProcedures => new FileProcedures()
            {
                Close = ac_Close,
                Length = ac_Length,
                Read = ac_Read,
                Seek = ac_Seek
            };

            public DataStreamFileProcedures(AsyncBufferStream data)
            {
                dataStream = data;
            }

            void ac_Close(IntPtr user)
            {
                //manually handle closing of stream
            }

            long ac_Length(IntPtr user)
            {
                if (dataStream == null) return 0;

                try
                {
                    return dataStream.Length;
                }
                catch
                {
                }

                return 0;
            }

            int ac_Read(IntPtr buffer, int length, IntPtr user)
            {
                if (dataStream == null) return 0;

                try
                {
                    if (length > readBuffer.Length)
                        readBuffer = new byte[length];

                    if (!dataStream.CanRead)
                        return 0;

                    int readBytes = dataStream.Read(readBuffer, 0, length);
                    Marshal.Copy(readBuffer, 0, buffer, readBytes);
                    return readBytes;
                }
                catch
                {
                }

                return 0;

            }

            bool ac_Seek(long offset, IntPtr user)
            {
                if (dataStream == null) return false;

                try
                {
                    return dataStream.Seek(offset, SeekOrigin.Begin) == offset;
                }
                catch
                {
                }
                return false;
            }
        }
    }
}

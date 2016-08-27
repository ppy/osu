//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Timing;

namespace osu.Framework.Audio.Track
{
    public abstract class AudioTrack : AdjustableAudioComponent, IAdjustableClock, IHasCompletedState, IUpdateable
    {
        /// <summary>
        /// Is this track capable of producing audio?
        /// </summary>
        public virtual bool IsDummyDevice => true;

        /// <summary>
        /// The speed of track playback. Does not affect pitch, but will reduce playback quality due to skipped frames.
        /// </summary>
        public readonly BindableDouble Tempo = new BindableDouble(1);

        public AudioTrack()
        {
            Tempo.ValueChanged += InvalidateState;
        }

        /// <summary>
        /// Reset this track to a logical default state.
        /// </summary>
        public virtual void Reset()
        {
            Frequency.Value = 1;

            Stop();
            Seek(0);
        }

        /// <summary>
        /// Current position in milliseconds.
        /// </summary>
        public abstract double CurrentTime { get; }

        /// <summary>
        /// Lenth of the track in milliseconds.
        /// </summary>
        public double Length { get; protected set; }

        public virtual int? Bitrate => null;

        /// <summary>
        /// Seek to a new position.
        /// </summary>
        /// <param name="seek">New position in milliseconds</param>
        /// <returns>Whether the seek was successful.</returns>
        public abstract bool Seek(double seek);

        public abstract void Start();

        public abstract void Stop();

        public abstract bool IsRunning { get; }

        /// <summary>
        /// Overall playback rate (1 is 100%, -1 is reversed at 100%).
        /// </summary>
        public virtual double Rate => Frequency * Tempo;

        public bool IsReversed => Rate < 0;

        /// <summary>
        /// todo: implement
        /// </summary>
        public bool HasCompleted => IsDisposed;
    }

}

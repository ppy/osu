// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Graphics.Containers
{
    /// <summary>
    /// A container which fires a callback when a new beat is reached.
    /// Consumes a parent <see cref="IBeatSyncProvider"/>.
    /// </summary>
    /// <remarks>
    /// This container does not set its own clock to the source used for beat matching.
    /// This means that if the beat source clock is playing faster or slower, animations may unexpectedly overlap.
    /// Make sure this container's Clock is also set to the expected source (or within a parent element which provides this).
    ///
    /// This container will also trigger beat events when the beat matching clock is paused at <see cref="TimingControlPoint.DEFAULT"/>'s BPM.
    /// </remarks>
    public partial class BeatSyncedContainer : Container
    {
        private int lastBeat;

        private TimingControlPoint? lastTimingPoint { get; set; }

        protected bool IsKiaiTime { get; private set; }

        /// <summary>
        /// The amount of time before a beat we should fire <see cref="OnNewBeat(int, TimingControlPoint, EffectControlPoint, ChannelAmplitudes)"/>.
        /// This allows for adding easing to animations that may be synchronised to the beat.
        /// </summary>
        protected double EarlyActivationMilliseconds;

        /// <summary>
        /// While this container automatically applied an animation delay (meaning any animations inside a <see cref="OnNewBeat"/> implementation will
        /// always be correctly timed), the event itself can potentially fire away from the related beat.
        ///
        /// By setting this to false, cases where the event is to be fired more than <see cref="MISTIMED_ALLOWANCE"/> from the related beat will be skipped.
        /// </summary>
        protected bool AllowMistimedEventFiring = true;

        /// <summary>
        /// The maximum deviance from the actual beat that an <see cref="OnNewBeat"/> can fire when <see cref="AllowMistimedEventFiring"/> is set to false.
        /// </summary>
        public const double MISTIMED_ALLOWANCE = 16;

        /// <summary>
        /// The time in milliseconds until the next beat.
        /// </summary>
        public double TimeUntilNextBeat { get; private set; }

        /// <summary>
        /// The time in milliseconds since the last beat
        /// </summary>
        public double TimeSinceLastBeat { get; private set; }

        /// <summary>
        /// How many beats per beatlength to trigger. Defaults to 1.
        /// </summary>
        public int Divisor { get; set; } = 1;

        /// <summary>
        /// An optional minimum beat length. Any beat length below this will be multiplied by two until valid.
        /// </summary>
        public double MinimumBeatLength { get; set; }

        /// <summary>
        /// Whether this container is currently tracking a beat sync provider.
        /// </summary>
        protected bool IsBeatSyncedWithTrack { get; private set; }

        [Resolved]
        protected IBeatSyncProvider BeatSyncSource { get; private set; } = null!;

        protected virtual void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
        }

        protected override void Update()
        {
            TimingControlPoint timingPoint;
            EffectControlPoint effectPoint;

            IsBeatSyncedWithTrack = BeatSyncSource.Clock.IsRunning;

            double currentTrackTime;

            if (IsBeatSyncedWithTrack)
            {
                currentTrackTime = BeatSyncSource.Clock.CurrentTime + EarlyActivationMilliseconds;

                timingPoint = BeatSyncSource.ControlPoints?.TimingPointAt(currentTrackTime) ?? TimingControlPoint.DEFAULT;
                effectPoint = BeatSyncSource.ControlPoints?.EffectPointAt(currentTrackTime) ?? EffectControlPoint.DEFAULT;
            }
            else
            {
                // this may be the case where the beat syncing clock has been paused.
                // we still want to show an idle animation, so use this container's time instead.
                currentTrackTime = Clock.CurrentTime + EarlyActivationMilliseconds;

                timingPoint = TimingControlPoint.DEFAULT;
                effectPoint = EffectControlPoint.DEFAULT;
            }

            double beatLength = timingPoint.BeatLength / Divisor;

            // ensure the assumed latency in beatmaps is the same at different playback rates
            double time = timingPoint.Time + 20 - (20 * Clock.Rate);

            while (beatLength < MinimumBeatLength)
                beatLength *= 2;

            int beatIndex = (int)((currentTrackTime - time) / beatLength) - (timingPoint.OmitFirstBarLine ? 1 : 0);

            // The beats before the start of the first control point are off by 1, this should do the trick
            if (currentTrackTime < time)
                beatIndex--;

            TimeUntilNextBeat = (time - currentTrackTime) % beatLength;
            if (TimeUntilNextBeat <= 0)
                TimeUntilNextBeat += beatLength;

            TimeSinceLastBeat = beatLength - TimeUntilNextBeat;

            if (ReferenceEquals(timingPoint, lastTimingPoint) && beatIndex == lastBeat)
                return;

            // as this event is sometimes used for sound triggers where `BeginDelayedSequence` has no effect, avoid firing it if too far away from the beat.
            // this can happen after a seek operation.
            if (AllowMistimedEventFiring || Math.Abs(TimeSinceLastBeat) < MISTIMED_ALLOWANCE)
            {
                using (BeginDelayedSequence(-TimeSinceLastBeat))
                    OnNewBeat(beatIndex, timingPoint, effectPoint, BeatSyncSource.CurrentAmplitudes);
            }

            lastBeat = beatIndex;
            lastTimingPoint = timingPoint;

            IsKiaiTime = effectPoint.KiaiMode;
        }
    }
}

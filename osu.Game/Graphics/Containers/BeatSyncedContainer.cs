// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Screens.Play;

namespace osu.Game.Graphics.Containers
{
    /// <summary>
    /// A container which fires a callback when a new beat is reached.
    /// Consumes a parent <see cref="GameplayClock"/> or <see cref="Beatmap"/> (whichever is first available).
    /// </summary>
    /// <remarks>
    /// This container does not set its own clock to the source used for beat matching.
    /// This means that if the beat source clock is playing faster or slower, animations may unexpectedly overlap.
    /// Make sure this container's Clock is also set to the expected source (or within a parent element which provides this).
    ///
    /// This container will also trigger beat events when the beat matching clock is paused at <see cref="TimingControlPoint.DEFAULT"/>'s BPM.
    /// </remarks>
    public class BeatSyncedContainer : Container
    {
        private int lastBeat;
        private TimingControlPoint lastTimingPoint;

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
        /// Whether this container is currently tracking a beatmap's timing data.
        /// </summary>
        protected bool IsBeatSyncedWithTrack { get; private set; }

        protected virtual void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
        }

        [Resolved]
        protected IBindable<WorkingBeatmap> Beatmap { get; private set; }

        [Resolved(canBeNull: true)]
        protected GameplayClock GameplayClock { get; private set; }

        protected IClock BeatSyncClock
        {
            get
            {
                if (GameplayClock != null)
                    return GameplayClock;

                if (Beatmap.Value.TrackLoaded)
                    return Beatmap.Value.Track;

                return null;
            }
        }

        protected override void Update()
        {
            ITrack track = null;
            IBeatmap beatmap = null;

            TimingControlPoint timingPoint;
            EffectControlPoint effectPoint;

            IClock clock = BeatSyncClock;

            if (clock == null)
                return;

            double currentTrackTime = clock.CurrentTime + EarlyActivationMilliseconds;

            if (Beatmap.Value.TrackLoaded && Beatmap.Value.BeatmapLoaded)
            {
                track = Beatmap.Value.Track;
                beatmap = Beatmap.Value.Beatmap;
            }

            IsBeatSyncedWithTrack = beatmap != null && clock.IsRunning && track?.Length > 0;

            if (IsBeatSyncedWithTrack)
            {
                Debug.Assert(beatmap != null);

                timingPoint = beatmap.ControlPointInfo.TimingPointAt(currentTrackTime);
                effectPoint = beatmap.ControlPointInfo.EffectPointAt(currentTrackTime);
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

            while (beatLength < MinimumBeatLength)
                beatLength *= 2;

            int beatIndex = (int)((currentTrackTime - timingPoint.Time) / beatLength) - (effectPoint.OmitFirstBarLine ? 1 : 0);

            // The beats before the start of the first control point are off by 1, this should do the trick
            if (currentTrackTime < timingPoint.Time)
                beatIndex--;

            TimeUntilNextBeat = (timingPoint.Time - currentTrackTime) % beatLength;
            if (TimeUntilNextBeat <= 0)
                TimeUntilNextBeat += beatLength;

            TimeSinceLastBeat = beatLength - TimeUntilNextBeat;

            if (timingPoint == lastTimingPoint && beatIndex == lastBeat)
                return;

            // as this event is sometimes used for sound triggers where `BeginDelayedSequence` has no effect, avoid firing it if too far away from the beat.
            // this can happen after a seek operation.
            if (AllowMistimedEventFiring || Math.Abs(TimeSinceLastBeat) < MISTIMED_ALLOWANCE)
            {
                using (BeginDelayedSequence(-TimeSinceLastBeat))
                    OnNewBeat(beatIndex, timingPoint, effectPoint, track?.CurrentAmplitudes ?? ChannelAmplitudes.Empty);
            }

            lastBeat = beatIndex;
            lastTimingPoint = timingPoint;
        }
    }
}

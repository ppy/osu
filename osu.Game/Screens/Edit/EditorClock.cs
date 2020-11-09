// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Timing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Screens.Edit
{
    /// <summary>
    /// A decoupled clock which adds editor-specific functionality, such as snapping to a user-defined beat divisor.
    /// </summary>
    public class EditorClock : Component, IFrameBasedClock, IAdjustableClock, ISourceChangeableClock
    {
        public IBindable<Track> Track => track;

        private readonly Bindable<Track> track = new Bindable<Track>();

        public double TrackLength => track.Value?.Length ?? 60000;

        public ControlPointInfo ControlPointInfo;

        private readonly BindableBeatDivisor beatDivisor;

        private readonly DecoupleableInterpolatingFramedClock underlyingClock;

        public IBindable<bool> SeekingOrStopped => seekingOrStopped;

        private readonly Bindable<bool> seekingOrStopped = new Bindable<bool>(true);

        public EditorClock(WorkingBeatmap beatmap, BindableBeatDivisor beatDivisor)
            : this(beatmap.Beatmap.ControlPointInfo, beatmap.Track.Length, beatDivisor)
        {
        }

        public EditorClock(ControlPointInfo controlPointInfo, double trackLength, BindableBeatDivisor beatDivisor)
        {
            this.beatDivisor = beatDivisor;

            ControlPointInfo = controlPointInfo;

            underlyingClock = new DecoupleableInterpolatingFramedClock();
        }

        public EditorClock()
            : this(new ControlPointInfo(), 1000, new BindableBeatDivisor())
        {
        }

        /// <summary>
        /// Seek to the closest snappable beat from a time.
        /// </summary>
        /// <param name="position">The raw position which should be seeked around.</param>
        /// <returns>Whether the seek could be performed.</returns>
        public bool SeekSnapped(double position)
        {
            var timingPoint = ControlPointInfo.TimingPointAt(position);
            double beatSnapLength = timingPoint.BeatLength / beatDivisor.Value;

            // We will be snapping to beats within the timing point
            position -= timingPoint.Time;

            // Determine the index from the current timing point of the closest beat to position
            int closestBeat = (int)Math.Round(position / beatSnapLength);
            position = timingPoint.Time + closestBeat * beatSnapLength;

            // Depending on beatSnapLength, we may snap to a beat that is beyond timingPoint's end time, but we want to instead snap to
            // the next timing point's start time
            var nextTimingPoint = ControlPointInfo.TimingPoints.FirstOrDefault(t => t.Time > timingPoint.Time);
            if (position > nextTimingPoint?.Time)
                position = nextTimingPoint.Time;

            return Seek(position);
        }

        /// <summary>
        /// Seeks backwards by one beat length.
        /// </summary>
        /// <param name="snapped">Whether to snap to the closest beat after seeking.</param>
        /// <param name="amount">The relative amount (magnitude) which should be seeked.</param>
        public void SeekBackward(bool snapped = false, double amount = 1) => seek(-1, snapped, amount + (IsRunning ? 1.5 : 0));

        /// <summary>
        /// Seeks forwards by one beat length.
        /// </summary>
        /// <param name="snapped">Whether to snap to the closest beat after seeking.</param>
        /// <param name="amount">The relative amount (magnitude) which should be seeked.</param>
        public void SeekForward(bool snapped = false, double amount = 1) => seek(1, snapped, amount);

        private void seek(int direction, bool snapped, double amount = 1)
        {
            double current = CurrentTimeAccurate;

            if (amount <= 0) throw new ArgumentException("Value should be greater than zero", nameof(amount));

            var timingPoint = ControlPointInfo.TimingPointAt(current);

            if (direction < 0 && timingPoint.Time == current)
                // When going backwards and we're at the boundary of two timing points, we compute the seek distance with the timing point which we are seeking into
                timingPoint = ControlPointInfo.TimingPointAt(current - 1);

            double seekAmount = timingPoint.BeatLength / beatDivisor.Value * amount;
            double seekTime = current + seekAmount * direction;

            if (!snapped || ControlPointInfo.TimingPoints.Count == 0)
            {
                SeekTo(seekTime);
                return;
            }

            // We will be snapping to beats within timingPoint
            seekTime -= timingPoint.Time;

            // Determine the index from timingPoint of the closest beat to seekTime, accounting for scrolling direction
            int closestBeat;
            if (direction > 0)
                closestBeat = (int)Math.Floor(seekTime / seekAmount);
            else
                closestBeat = (int)Math.Ceiling(seekTime / seekAmount);

            seekTime = timingPoint.Time + closestBeat * seekAmount;

            // limit forward seeking to only up to the next timing point's start time.
            var nextTimingPoint = ControlPointInfo.TimingPoints.FirstOrDefault(t => t.Time > timingPoint.Time);
            if (seekTime > nextTimingPoint?.Time)
                seekTime = nextTimingPoint.Time;

            // Due to the rounding above, we may end up on the current beat. This will effectively cause 0 seeking to happen, but we don't want this.
            // Instead, we'll go to the next beat in the direction when this is the case
            if (Precision.AlmostEquals(current, seekTime, 0.5f))
            {
                closestBeat += direction > 0 ? 1 : -1;
                seekTime = timingPoint.Time + closestBeat * seekAmount;
            }

            if (seekTime < timingPoint.Time && timingPoint != ControlPointInfo.TimingPoints.First())
                seekTime = timingPoint.Time;

            // Ensure the sought point is within the boundaries
            seekTime = Math.Clamp(seekTime, 0, TrackLength);
            SeekTo(seekTime);
        }

        /// <summary>
        /// The current time of this clock, include any active transform seeks performed via <see cref="SeekTo"/>.
        /// </summary>
        public double CurrentTimeAccurate =>
            Transforms.OfType<TransformSeek>().FirstOrDefault()?.EndValue ?? CurrentTime;

        public double CurrentTime => underlyingClock.CurrentTime;

        public void Reset()
        {
            ClearTransforms();
            underlyingClock.Reset();
        }

        public void Start()
        {
            ClearTransforms();
            underlyingClock.Start();
        }

        public void Stop()
        {
            seekingOrStopped.Value = true;
            underlyingClock.Stop();
        }

        public bool Seek(double position)
        {
            seekingOrStopped.Value = true;

            ClearTransforms();
            return underlyingClock.Seek(position);
        }

        public void ResetSpeedAdjustments() => underlyingClock.ResetSpeedAdjustments();

        double IAdjustableClock.Rate
        {
            get => underlyingClock.Rate;
            set => underlyingClock.Rate = value;
        }

        double IClock.Rate => underlyingClock.Rate;

        public bool IsRunning => underlyingClock.IsRunning;

        public void ProcessFrame() => underlyingClock.ProcessFrame();

        public double ElapsedFrameTime => underlyingClock.ElapsedFrameTime;

        public double FramesPerSecond => underlyingClock.FramesPerSecond;

        public FrameTimeInfo TimeInfo => underlyingClock.TimeInfo;

        public void ChangeSource(IClock source)
        {
            track.Value = source as Track;
            underlyingClock.ChangeSource(source);
        }

        public IClock Source => underlyingClock.Source;

        public bool IsCoupled
        {
            get => underlyingClock.IsCoupled;
            set => underlyingClock.IsCoupled = value;
        }

        private const double transform_time = 300;

        protected override void Update()
        {
            base.Update();

            updateSeekingState();
        }

        private void updateSeekingState()
        {
            if (seekingOrStopped.Value)
            {
                if (track.Value?.IsRunning != true)
                {
                    // seeking in the editor can happen while the track isn't running.
                    // in this case we always want to expose ourselves as seeking (to avoid sample playback).
                    return;
                }

                // we are either running a seek tween or doing an immediate seek.
                // in the case of an immediate seek the seeking bool will be set to false after one update.
                // this allows for silencing hit sounds and the likes.
                seekingOrStopped.Value = Transforms.Any();
            }
        }

        public void SeekTo(double seekDestination)
        {
            seekingOrStopped.Value = true;

            if (IsRunning)
                Seek(seekDestination);
            else
                transformSeekTo(seekDestination, transform_time, Easing.OutQuint);
        }

        private void transformSeekTo(double seek, double duration = 0, Easing easing = Easing.None)
            => this.TransformTo(this.PopulateTransform(new TransformSeek(), seek, duration, easing));

        private double currentTime
        {
            get => underlyingClock.CurrentTime;
            set => underlyingClock.Seek(value);
        }

        private class TransformSeek : Transform<double, EditorClock>
        {
            public override string TargetMember => nameof(currentTime);

            protected override void Apply(EditorClock clock, double time) => clock.currentTime = valueAt(time);

            private double valueAt(double time)
            {
                if (time < StartTime) return StartValue;
                if (time >= EndTime) return EndValue;

                return Interpolation.ValueAt(time, StartValue, EndValue, StartTime, EndTime, Easing);
            }

            protected override void ReadIntoStartValue(EditorClock clock) => StartValue = clock.currentTime;
        }
    }
}

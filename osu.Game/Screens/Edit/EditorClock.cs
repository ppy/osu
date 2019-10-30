// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.MathUtils;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osuTK;

namespace osu.Game.Screens.Edit
{
    /// <summary>
    /// A decoupled clock which adds editor-specific functionality, such as snapping to a user-defined beat divisor.
    /// </summary>
    public class EditorClock : DecoupleableInterpolatingFramedClock
    {
        public readonly double TrackLength;

        public ControlPointInfo ControlPointInfo;

        private readonly BindableBeatDivisor beatDivisor;

        public EditorClock(WorkingBeatmap beatmap, BindableBeatDivisor beatDivisor)
        {
            this.beatDivisor = beatDivisor;

            ControlPointInfo = beatmap.Beatmap.ControlPointInfo;
            TrackLength = beatmap.Track.Length;
        }

        public EditorClock(ControlPointInfo controlPointInfo, double trackLength, BindableBeatDivisor beatDivisor)
        {
            this.beatDivisor = beatDivisor;

            ControlPointInfo = controlPointInfo;
            TrackLength = trackLength;
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
        public void SeekBackward(bool snapped = false, double amount = 1) => seek(-1, snapped, amount);

        /// <summary>
        /// Seeks forwards by one beat length.
        /// </summary>
        /// <param name="snapped">Whether to snap to the closest beat after seeking.</param>
        /// <param name="amount">The relative amount (magnitude) which should be seeked.</param>
        public void SeekForward(bool snapped = false, double amount = 1) => seek(1, snapped, amount);

        private void seek(int direction, bool snapped, double amount = 1)
        {
            if (amount <= 0) throw new ArgumentException("Value should be greater than zero", nameof(amount));

            var timingPoint = ControlPointInfo.TimingPointAt(CurrentTime);

            if (direction < 0 && timingPoint.Time == CurrentTime)
                // When going backwards and we're at the boundary of two timing points, we compute the seek distance with the timing point which we are seeking into
                timingPoint = ControlPointInfo.TimingPointAt(CurrentTime - 1);

            double seekAmount = timingPoint.BeatLength / beatDivisor.Value * amount;
            double seekTime = CurrentTime + seekAmount * direction;

            if (!snapped || ControlPointInfo.TimingPoints.Count == 0)
            {
                Seek(seekTime);
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

            // Due to the rounding above, we may end up on the current beat. This will effectively cause 0 seeking to happen, but we don't want this.
            // Instead, we'll go to the next beat in the direction when this is the case
            if (Precision.AlmostEquals(CurrentTime, seekTime))
            {
                closestBeat += direction > 0 ? 1 : -1;
                seekTime = timingPoint.Time + closestBeat * seekAmount;
            }

            if (seekTime < timingPoint.Time && timingPoint != ControlPointInfo.TimingPoints.First())
                seekTime = timingPoint.Time;

            var nextTimingPoint = ControlPointInfo.TimingPoints.FirstOrDefault(t => t.Time > timingPoint.Time);
            if (seekTime > nextTimingPoint?.Time)
                seekTime = nextTimingPoint.Time;

            // Ensure the sought point is within the boundaries
            seekTime = MathHelper.Clamp(seekTime, 0, TrackLength);
            Seek(seekTime);
        }
    }
}

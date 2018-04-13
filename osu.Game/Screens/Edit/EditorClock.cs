﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.MathUtils;
using osu.Framework.Timing;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Screens.Edit.Screens.Compose;

namespace osu.Game.Screens.Edit
{
    /// <summary>
    /// A decoupled clock which adds editor-specific functionality, such as snapping to a user-defined beat divisor.
    /// </summary>
    public class EditorClock : DecoupleableInterpolatingFramedClock
    {
        public ControlPointInfo ControlPointInfo;

        private readonly BindableBeatDivisor beatDivisor;

        public EditorClock(ControlPointInfo controlPointInfo, BindableBeatDivisor beatDivisor)
        {
            this.beatDivisor = beatDivisor;

            ControlPointInfo = controlPointInfo;
        }

        /// <summary>
        /// Seek to the closest snappable beat from a time.
        /// </summary>
        /// <param name="position">The raw position which should be seeked around.</param>
        /// <returns>Whether the seek could be performed.</returns>
        public bool SeekSnapped(double position)
        {
            var timingPoint = ControlPointInfo.TimingPointAt(position);
            double beatSnapLength = timingPoint.BeatLength / beatDivisor;

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
        public void SeekBackward(bool snapped = false) => seek(-1, snapped);

        /// <summary>
        /// Seeks forwards by one beat length.
        /// </summary>
        /// <param name="snapped">Whether to snap to the closest beat after seeking.</param>
        public void SeekForward(bool snapped = false) => seek(1, snapped);

        private void seek(int direction, bool snapped)
        {
            var timingPoint = ControlPointInfo.TimingPointAt(CurrentTime);
            if (direction < 0 && timingPoint.Time == CurrentTime)
            {
                // When going backwards and we're at the boundary of two timing points, we compute the seek distance with the timing point which we are seeking into
                int activeIndex = ControlPointInfo.TimingPoints.IndexOf(timingPoint);
                while (activeIndex > 0 && CurrentTime == timingPoint.Time)
                    timingPoint = ControlPointInfo.TimingPoints[--activeIndex];
            }

            double seekAmount = timingPoint.BeatLength / beatDivisor;
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

            Seek(seekTime);
        }
    }
}

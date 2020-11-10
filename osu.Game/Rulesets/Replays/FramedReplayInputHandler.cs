// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using osu.Game.Input.Handlers;
using osu.Game.Replays;

namespace osu.Game.Rulesets.Replays
{
    /// <summary>
    /// The ReplayHandler will take a replay and handle the propagation of updates to the input stack.
    /// It handles logic of any frames which *must* be executed.
    /// </summary>
    public abstract class FramedReplayInputHandler<TFrame> : ReplayInputHandler
        where TFrame : ReplayFrame
    {
        private readonly Replay replay;

        protected List<ReplayFrame> Frames => replay.Frames;

        public TFrame CurrentFrame
        {
            get
            {
                if (!HasFrames || !currentFrameIndex.HasValue)
                    return null;

                return (TFrame)Frames[currentFrameIndex.Value];
            }
        }

        public TFrame NextFrame
        {
            get
            {
                if (!HasFrames)
                    return null;

                if (!currentFrameIndex.HasValue)
                    return currentDirection > 0 ? (TFrame)Frames[0] : null;

                int nextFrame = clampedNextFrameIndex;

                if (nextFrame == currentFrameIndex.Value)
                    return null;

                return (TFrame)Frames[clampedNextFrameIndex];
            }
        }

        private int? currentFrameIndex;

        private int clampedNextFrameIndex =>
            currentFrameIndex.HasValue ? Math.Clamp(currentFrameIndex.Value + currentDirection, 0, Frames.Count - 1) : 0;

        protected FramedReplayInputHandler(Replay replay)
        {
            this.replay = replay;
        }

        private const double sixty_frame_time = 1000.0 / 60;

        protected virtual double AllowedImportantTimeSpan => sixty_frame_time * 1.2;

        protected double? CurrentTime { get; private set; }

        private int currentDirection = 1;

        /// <summary>
        /// When set, we will ensure frames executed by nested drawables are frame-accurate to replay data.
        /// Disabling this can make replay playback smoother (useful for autoplay, currently).
        /// </summary>
        public bool FrameAccuratePlayback;

        public bool HasFrames => Frames.Count > 0;

        private bool inImportantSection
        {
            get
            {
                if (!HasFrames || !FrameAccuratePlayback)
                    return false;

                var frame = currentDirection > 0 ? CurrentFrame : NextFrame;

                if (frame == null)
                    return false;

                return IsImportant(frame) && // a button is in a pressed state
                       Math.Abs(CurrentTime - NextFrame?.Time ?? 0) <= AllowedImportantTimeSpan; // the next frame is within an allowable time span
            }
        }

        protected virtual bool IsImportant([NotNull] TFrame frame) => false;

        /// <summary>
        /// Update the current frame based on an incoming time value.
        /// There are cases where we return a "must-use" time value that is different from the input.
        /// This is to ensure accurate playback of replay data.
        /// </summary>
        /// <param name="time">The time which we should use for finding the current frame.</param>
        /// <returns>The usable time value. If null, we should not advance time as we do not have enough data.</returns>
        public override double? SetFrameFromTime(double time)
        {
            updateDirection(time);

            Debug.Assert(currentDirection != 0);

            if (!HasFrames)
            {
                // in the case all frames are received, allow time to progress regardless.
                if (replay.HasReceivedAllFrames)
                    return CurrentTime = time;

                return null;
            }

            TFrame next = NextFrame;

            // if we have a next frame, check if it is before or at the current time in playback, and advance time to it if so.
            if (next != null)
            {
                int compare = time.CompareTo(next.Time);

                if (compare == 0 || compare == currentDirection)
                {
                    currentFrameIndex = clampedNextFrameIndex;
                    return CurrentTime = CurrentFrame.Time;
                }
            }

            // at this point, the frame index can't be advanced.
            // even so, we may be able to propose the clock progresses forward due to being at an extent of the replay,
            // or moving towards the next valid frame (ie. interpolating in a non-important section).

            // the exception is if currently in an important section, which is respected above all.
            if (inImportantSection)
            {
                Debug.Assert(next != null || !replay.HasReceivedAllFrames);
                return null;
            }

            // if a next frame does exist, allow interpolation.
            if (next != null)
                return CurrentTime = time;

            // if all frames have been received, allow playing beyond extents.
            if (replay.HasReceivedAllFrames)
                return CurrentTime = time;

            // if not all frames are received but we are before the first frame, allow playing.
            if (time < Frames[0].Time)
                return CurrentTime = time;

            // in the case we have no next frames and haven't received enough frame data, block.
            return null;
        }

        private void updateDirection(double time)
        {
            if (!CurrentTime.HasValue)
            {
                currentDirection = 1;
            }
            else
            {
                currentDirection = time.CompareTo(CurrentTime);
                if (currentDirection == 0) currentDirection = 1;
            }
        }
    }
}

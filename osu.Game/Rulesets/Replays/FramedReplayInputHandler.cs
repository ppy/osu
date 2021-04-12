// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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
        /// <summary>
        /// Whether we have at least one replay frame.
        /// </summary>
        public bool HasFrames => Frames.Count != 0;

        /// <summary>
        /// Whether we are waiting for new frames to be received.
        /// </summary>
        public bool WaitingForFrame => !replay.HasReceivedAllFrames && currentFrameIndex == Frames.Count - 1;

        /// <summary>
        /// The current frame of the replay.
        /// The current time is always between the start and the end time of the current frame.
        /// </summary>
        /// <exception cref="InvalidOperationException">The replay is empty.</exception>
        public TFrame CurrentFrame
        {
            get
            {
                if (!HasFrames)
                    throw new InvalidOperationException($"Attempted to get {nameof(CurrentFrame)} of an empty replay");

                return (TFrame)Frames[Math.Max(0, currentFrameIndex)];
            }
        }

        /// <summary>
        /// The next frame of the replay.
        /// The start time is always greater or equals to the start time of <see cref="CurrentFrame"/> regardless of the seeking direction.
        /// If it is before the first frame of the replay or the after the last frame of the replay, <see cref="CurrentFrame"/> and <see cref="NextFrame"/> agree.
        /// </summary>
        /// <exception cref="InvalidOperationException">The replay is empty.</exception>
        public TFrame NextFrame
        {
            get
            {
                if (!HasFrames)
                    throw new InvalidOperationException($"Attempted to get {nameof(NextFrame)} of an empty replay");

                return (TFrame)Frames[Math.Min(currentFrameIndex + 1, Frames.Count - 1)];
            }
        }

        /// <summary>
        /// When set, we will ensure frames executed by nested drawables are frame-accurate to replay data.
        /// Disabling this can make replay playback smoother (useful for autoplay, currently).
        /// </summary>
        public bool FrameAccuratePlayback;

        // This input handler should be enabled only if there is at least one replay frame.
        public override bool IsActive => HasFrames;

        // Can make it non-null but that is a breaking change.
        protected double? CurrentTime { get; private set; }

        protected virtual double AllowedImportantTimeSpan => sixty_frame_time * 1.2;

        protected List<ReplayFrame> Frames => replay.Frames;

        private readonly Replay replay;

        private int currentFrameIndex;

        private const double sixty_frame_time = 1000.0 / 60;

        protected FramedReplayInputHandler(Replay replay)
        {
            // This replay frame ordering should be enforced on the Replay type
            replay.Frames.Sort((x, y) => x.Time.CompareTo(y.Time));

            this.replay = replay;
            currentFrameIndex = -1;
            CurrentTime = double.NegativeInfinity;
        }

        private bool inImportantSection
        {
            get
            {
                if (!HasFrames || !FrameAccuratePlayback)
                    return false;

                return IsImportant(CurrentFrame) && // a button is in a pressed state
                       Math.Abs(CurrentTime - NextFrame.Time ?? 0) <= AllowedImportantTimeSpan; // the next frame is within an allowable time span
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
            if (!HasFrames)
            {
                // In the case all frames are received, allow time to progress regardless.
                if (replay.HasReceivedAllFrames)
                    return CurrentTime = time;

                return null;
            }

            double frameStart = getFrameTime(currentFrameIndex);
            double frameEnd = getFrameTime(currentFrameIndex + 1);

            // If the proposed time is after the current frame end time, we progress forwards.
            // If the proposed time is before the current frame start time, and we are at the frame boundary, we progress backwards.
            if (frameEnd <= time)
            {
                time = frameEnd;
                currentFrameIndex++;
            }
            else if (time < frameStart && CurrentTime == frameStart)
                currentFrameIndex--;

            frameStart = getFrameTime(currentFrameIndex);
            frameEnd = getFrameTime(currentFrameIndex + 1);

            // Pause until more frames are arrived.
            if (WaitingForFrame && frameStart < time)
            {
                CurrentTime = frameStart;
                return null;
            }

            CurrentTime = Math.Clamp(time, frameStart, frameEnd);

            // In an important section, a mid-frame time cannot be used and a null is returned instead.
            return inImportantSection && frameStart < time && time < frameEnd ? null : CurrentTime;
        }

        private double getFrameTime(int index)
        {
            if (index < 0)
                return double.NegativeInfinity;
            if (index >= Frames.Count)
                return double.PositiveInfinity;

            return Frames[index].Time;
        }
    }
}

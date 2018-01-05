// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Input;
using osu.Framework.MathUtils;
using osu.Game.Input.Handlers;
using OpenTK;
using OpenTK.Input;
using KeyboardState = osu.Framework.Input.KeyboardState;
using MouseState = osu.Framework.Input.MouseState;

namespace osu.Game.Rulesets.Replays
{
    /// <summary>
    /// The ReplayHandler will take a replay and handle the propagation of updates to the input stack.
    /// It handles logic of any frames which *must* be executed.
    /// </summary>
    public abstract class FramedReplayInputHandler : ReplayInputHandler
    {
        private readonly Replay replay;

        protected List<ReplayFrame> Frames => replay.Frames;

        public ReplayFrame CurrentFrame => !hasFrames ? null : Frames[currentFrameIndex];
        public ReplayFrame NextFrame => !hasFrames ? null : Frames[nextFrameIndex];

        private int currentFrameIndex;

        private int nextFrameIndex => MathHelper.Clamp(currentFrameIndex + (currentDirection > 0 ? 1 : -1), 0, Frames.Count - 1);

        protected FramedReplayInputHandler(Replay replay)
        {
            this.replay = replay;
        }

        private bool advanceFrame()
        {
            int newFrame = nextFrameIndex;

            //ensure we aren't at an extent.
            if (newFrame == currentFrameIndex) return false;

            currentFrameIndex = newFrame;
            return true;
        }

        public void SetPosition(Vector2 pos)
        {
        }

        protected Vector2? Position
        {
            get
            {
                if (!hasFrames)
                    return null;

                return Interpolation.ValueAt(currentTime, CurrentFrame.Position, NextFrame.Position, CurrentFrame.Time, NextFrame.Time);
            }
        }

        public override List<InputState> GetPendingStates() => new List<InputState>();

        public bool AtLastFrame => currentFrameIndex == Frames.Count - 1;
        public bool AtFirstFrame => currentFrameIndex == 0;

        public Vector2 Size => new Vector2(512, 384);

        private const double sixty_frame_time = 1000.0 / 60;

        private double currentTime;
        private int currentDirection;

        /// <summary>
        /// When set, we will ensure frames executed by nested drawables are frame-accurate to replay data.
        /// Disabling this can make replay playback smoother (useful for autoplay, currently).
        /// </summary>
        public bool FrameAccuratePlayback = true;

        private bool hasFrames => Frames.Count > 0;

        private bool inImportantSection =>
            FrameAccuratePlayback &&
            //a button is in a pressed state
            ((currentDirection > 0 ? CurrentFrame : NextFrame)?.IsImportant ?? false) &&
            //the next frame is within an allowable time span
            Math.Abs(currentTime - NextFrame?.Time ?? 0) <= sixty_frame_time * 1.2;

        /// <summary>
        /// Update the current frame based on an incoming time value.
        /// There are cases where we return a "must-use" time value that is different from the input.
        /// This is to ensure accurate playback of replay data.
        /// </summary>
        /// <param name="time">The time which we should use for finding the current frame.</param>
        /// <returns>The usable time value. If null, we should not advance time as we do not have enough data.</returns>
        public override double? SetFrameFromTime(double time)
        {
            currentDirection = time.CompareTo(currentTime);
            if (currentDirection == 0) currentDirection = 1;

            if (hasFrames)
            {
                //if we changed frames, we want to execute once *exactly* on the frame's time.
                if (currentDirection == time.CompareTo(NextFrame.Time) && advanceFrame())
                {
                    // If going backwards, we need to execute once _before_ the frame time to reverse any judgements
                    // that would occur as a result of this frame in forward playback
                    if (currentDirection == -1)
                        return currentTime = CurrentFrame.Time - 1;
                    return currentTime = CurrentFrame.Time;
                }

                //if we didn't change frames, we need to ensure we are allowed to run frames in between, else return null.
                if (inImportantSection)
                    return null;
            }

            return currentTime = time;
        }

        protected class ReplayMouseState : MouseState
        {
            public ReplayMouseState(Vector2 position)
            {
                Position = position;
            }
        }

        protected class ReplayKeyboardState : KeyboardState
        {
            public ReplayKeyboardState(List<Key> keys)
            {
                Keys = keys;
            }
        }
    }
}

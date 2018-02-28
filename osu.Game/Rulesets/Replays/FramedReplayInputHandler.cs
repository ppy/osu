// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Input;
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
    public abstract class FramedReplayInputHandler<TFrame> : ReplayInputHandler
        where TFrame : ReplayFrame
    {
        private readonly Replay replay;

        protected List<ReplayFrame> Frames => replay.Frames;

        public TFrame CurrentFrame => !HasFrames ? null : (TFrame)Frames[currentFrameIndex];
        public TFrame NextFrame => !HasFrames ? null : (TFrame)Frames[nextFrameIndex];

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

        public override List<InputState> GetPendingStates() => new List<InputState>();

        public bool AtLastFrame => currentFrameIndex == Frames.Count - 1;
        public bool AtFirstFrame => currentFrameIndex == 0;

        private const double sixty_frame_time = 1000.0 / 60;

        protected double CurrentTime { get; private set; }
        private int currentDirection;

        /// <summary>
        /// When set, we will ensure frames executed by nested drawables are frame-accurate to replay data.
        /// Disabling this can make replay playback smoother (useful for autoplay, currently).
        /// </summary>
        public bool FrameAccuratePlayback = true;

        protected bool HasFrames => Frames.Count > 0;

        private bool inImportantSection =>
            HasFrames && FrameAccuratePlayback &&
            //a button is in a pressed state
            IsImportant(currentDirection > 0 ? CurrentFrame : NextFrame) &&
            //the next frame is within an allowable time span
            Math.Abs(CurrentTime - NextFrame?.Time ?? 0) <= sixty_frame_time * 1.2;

        protected virtual bool IsImportant(TFrame frame) => false;

        /// <summary>
        /// Update the current frame based on an incoming time value.
        /// There are cases where we return a "must-use" time value that is different from the input.
        /// This is to ensure accurate playback of replay data.
        /// </summary>
        /// <param name="time">The time which we should use for finding the current frame.</param>
        /// <returns>The usable time value. If null, we should not advance time as we do not have enough data.</returns>
        public override double? SetFrameFromTime(double time)
        {
            currentDirection = time.CompareTo(CurrentTime);
            if (currentDirection == 0) currentDirection = 1;

            if (HasFrames)
            {
                // check if the next frame is in the "future" for the current playback direction
                if (currentDirection != time.CompareTo(NextFrame.Time))
                {
                    // if we didn't change frames, we need to ensure we are allowed to run frames in between, else return null.
                    if (inImportantSection)
                        return null;
                }
                else if (advanceFrame())
                {
                    // If going backwards, we need to execute once _before_ the frame time to reverse any judgements
                    // that would occur as a result of this frame in forward playback
                    if (currentDirection == -1)
                        return CurrentTime = CurrentFrame.Time - 1;
                    return CurrentTime = CurrentFrame.Time;
                }
            }

            return CurrentTime = time;
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

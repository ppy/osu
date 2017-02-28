// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using osu.Framework.Input.Handlers;
using osu.Framework.MathUtils;
using osu.Framework.Platform;
using OpenTK;
using osu.Framework.Input;
using osu.Game.IO.Legacy;
using OpenTK.Input;
using KeyboardState = osu.Framework.Input.KeyboardState;
using MouseState = osu.Framework.Input.MouseState;

namespace osu.Game.Input.Handlers
{
    /// <summary>
    /// The ReplayHandler will take a replay and handle the propagation of updates to the input stack.
    /// It handles logic of any frames which *must* be executed.
    /// </summary>
    public class LegacyReplayInputHandler : InputHandler
    {
        public Func<Vector2, Vector2> ToScreenSpace { private get; set; }

        private readonly List<LegacyReplayFrame> replayContent;
        int currentFrameIndex;

        public LegacyReplayFrame CurrentFrame => !hasFrames ? null : replayContent[currentFrameIndex];
        public LegacyReplayFrame NextFrame => !hasFrames ? null : replayContent[MathHelper.Clamp(currentDirection > 0 ? currentFrameIndex + 1 : currentFrameIndex - 1, 0, replayContent.Count - 1)];

        public override bool Initialize(GameHost host) => true;

        public override bool IsActive => true;

        public override int Priority => 0;

        public LegacyReplayInputHandler(List<LegacyReplayFrame> replayContent)
        {
            this.replayContent = replayContent;
        }

        private bool nextFrame()
        {
            int newFrame = MathHelper.Clamp(currentFrameIndex + (currentDirection > 0 ? 1 : -1), 0, replayContent.Count - 1);

            //ensure we aren't at an extent.
            if (newFrame == currentFrameIndex) return false;

            currentFrameIndex = newFrame;
            return true;
        }

        public void SetPosition(Vector2 pos)
        {
        }

        private Vector2? position
        {
            get
            {
                if (!hasFrames)
                    return null;

                if (AtLastFrame)
                    return CurrentFrame.Position;

                return Interpolation.ValueAt(currentTime, CurrentFrame.Position, NextFrame.Position, CurrentFrame.Time, NextFrame.Time);
            }
        }

        public override List<InputState> GetPendingStates()
        {
            return new List<InputState>
            {
                new InputState
                {
                    Mouse = new ReplayMouseState(
                        ToScreenSpace(position ?? Vector2.Zero),
                        new List<MouseState.ButtonState>
                        {
                            new MouseState.ButtonState(MouseButton.Left) { State = CurrentFrame?.MouseLeft ?? false },
                            new MouseState.ButtonState(MouseButton.Right) { State = CurrentFrame?.MouseRight ?? false },
                        }
                    ),
                    Keyboard = new ReplayKeyboardState(new List<Key>())
                }
            };
        }

        public bool AtLastFrame => currentFrameIndex == replayContent.Count - 1;
        public bool AtFirstFrame => currentFrameIndex == 0;

        public Vector2 Size => new Vector2(512, 384);

        private const double sixty_frame_time = 1000 / 60;

        double currentTime;
        int currentDirection;

        /// <summary>
        /// When set, we will ensure frames executed by nested drawables are frame-accurate to replay data.
        /// Disabling this can make replay playback smoother (useful for autoplay, currently).
        /// </summary>
        public bool FrameAccuratePlayback = true;

        private bool hasFrames => replayContent.Count > 0;

        bool inImportantSection =>
            FrameAccuratePlayback &&
            //a button is in a pressed state
            (currentDirection > 0 ? CurrentFrame : NextFrame)?.ButtonState > LegacyButtonState.None &&
            //the next frame is within an allowable time span
            Math.Abs(currentTime - NextFrame?.Time ?? 0) <= sixty_frame_time * 1.2;

        /// <summary>
        /// Update the current frame based on an incoming time value.
        /// There are cases where we return a "must-use" time value that is different from the input.
        /// This is to ensure accurate playback of replay data.
        /// </summary>
        /// <param name="time">The time which we should use for finding the current frame.</param>
        /// <returns>The usable time value. If null, we shouldn't be running components reliant on this data.</returns>
        public double? SetFrameFromTime(double time)
        {
            currentDirection = time.CompareTo(currentTime);
            if (currentDirection == 0) currentDirection = 1;

            if (hasFrames)
            {
                //if we changed frames, we want to execute once *exactly* on the frame's time.
                if (currentDirection == time.CompareTo(NextFrame.Time) && nextFrame())
                    return currentTime = CurrentFrame.Time;

                //if we didn't change frames, we need to ensure we are allowed to run frames in between, else return null.
                if (inImportantSection)
                    return null;
            }

            return currentTime = time;
        }

        private class ReplayMouseState : MouseState
        {
            public ReplayMouseState(Vector2 position, List<ButtonState> list)
            {
                Position = position;
                ButtonStates = list;
            }
        }

        private class ReplayKeyboardState : KeyboardState
        {
            public ReplayKeyboardState(List<Key> keys)
            {
                Keys = keys;
            }
        }

        [Flags]
        public enum LegacyButtonState
        {
            None = 0,
            Left1 = 1,
            Right1 = 2,
            Left2 = 4,
            Right2 = 8,
            Smoke = 16
        }

        public class LegacyReplayFrame
        {
            public Vector2 Position => new Vector2(MouseX, MouseY);

            public float MouseX;
            public float MouseY;
            public bool MouseLeft;
            public bool MouseRight;
            public bool MouseLeft1;
            public bool MouseRight1;
            public bool MouseLeft2;
            public bool MouseRight2;
            public LegacyButtonState ButtonState;
            public double Time;

            public LegacyReplayFrame(double time, float posX, float posY, LegacyButtonState buttonState)
            {
                MouseX = posX;
                MouseY = posY;
                ButtonState = buttonState;
                SetButtonStates(buttonState);
                Time = time;
            }

            public void SetButtonStates(LegacyButtonState buttonState)
            {
                ButtonState = buttonState;
                MouseLeft = (buttonState & (LegacyButtonState.Left1 | LegacyButtonState.Left2)) > 0;
                MouseLeft1 = (buttonState & LegacyButtonState.Left1) > 0;
                MouseLeft2 = (buttonState & LegacyButtonState.Left2) > 0;
                MouseRight = (buttonState & (LegacyButtonState.Right1 | LegacyButtonState.Right2)) > 0;
                MouseRight1 = (buttonState & LegacyButtonState.Right1) > 0;
                MouseRight2 = (buttonState & LegacyButtonState.Right2) > 0;
            }

            public LegacyReplayFrame(Stream s) : this(new SerializationReader(s))
            {
            }

            public LegacyReplayFrame(SerializationReader sr)
            {
                ButtonState = (LegacyButtonState)sr.ReadByte();
                SetButtonStates(ButtonState);

                byte bt = sr.ReadByte();
                if (bt > 0)//Handle Pre-Taiko compatible replays.
                    SetButtonStates(LegacyButtonState.Right1);

                MouseX = sr.ReadSingle();
                MouseY = sr.ReadSingle();
                Time = sr.ReadInt32();
            }

            public void ReadFromStream(SerializationReader sr)
            {
                throw new System.NotImplementedException();
            }

            public void WriteToStream(SerializationWriter sw)
            {
                sw.Write((byte)ButtonState);
                sw.Write((byte)0);
                sw.Write(MouseX);
                sw.Write(MouseY);
                sw.Write(Time);
            }

            public override string ToString()
            {
                return $"{Time}\t({MouseX},{MouseY})\t{MouseLeft}\t{MouseRight}\t{MouseLeft1}\t{MouseRight1}\t{MouseLeft2}\t{MouseRight2}\t{ButtonState}";
            }
        }
    }
}
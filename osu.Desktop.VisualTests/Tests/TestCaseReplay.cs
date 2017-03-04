// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using osu.Framework.Allocation;
using osu.Framework.Screens.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using OpenTK;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Input.Handlers;
using osu.Framework.MathUtils;
using osu.Framework.Platform;
using osu.Game.Beatmaps.IO;
using osu.Game.Database;
using osu.Game.Input.Handlers;
using osu.Game.IO.Legacy;
using osu.Game.Modes;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Osu.Objects;
using osu.Game.Screens.Play;
using OpenTK.Graphics;
using OpenTK.Input;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Readers;
using KeyboardState = osu.Framework.Input.KeyboardState;
using MouseState = osu.Framework.Input.MouseState;

namespace osu.Desktop.VisualTests.Tests
{
    public class ScoreDatabase
    {
        private readonly Storage storage;

        private const string replay_folder = @"replays";

        public ScoreDatabase(Storage storage)
        {
            this.storage = storage;
        }

        public Score ReadReplayFile(string replayFilename)
        {
            Score score;

            using (Stream s = storage.GetStream(Path.Combine(replay_folder, replayFilename)))
            using (SerializationReader sr = new SerializationReader(s))
            {
                var ruleset = Ruleset.GetRuleset((PlayMode)sr.ReadByte());
                var processor = ruleset.CreateScoreProcessor();

                score = processor.GetScore();



                /* score.Pass = true;*/
                var version =  sr.ReadInt32();
                /* score.FileChecksum = */ sr.ReadString();
                /* score.PlayerName = */ sr.ReadString();
                /* var localScoreChecksum = */ sr.ReadString();
                /* score.Count300 = */ sr.ReadUInt16();
                /* score.Count100 = */ sr.ReadUInt16();
                /* score.Count50 = */ sr.ReadUInt16();
                /* score.CountGeki = */ sr.ReadUInt16();
                /* score.CountKatu = */ sr.ReadUInt16();
                /* score.CountMiss = */ sr.ReadUInt16();
                score.TotalScore = sr.ReadInt32();
                score.MaxCombo = sr.ReadUInt16();
                /* score.Perfect = */ sr.ReadBoolean();
                /* score.EnabledMods = (Mods)*/ sr.ReadInt32();
                /* score.HpGraphString = */ sr.ReadString();
                /* score.Date = */ sr.ReadDateTime();

                var compressedReplay = sr.ReadByteArray();

                if (version >= 20140721)
                    /*OnlineId =*/ sr.ReadInt64();
                else if (version >= 20121008)
                    /*OnlineId =*/ sr.ReadInt32();

                //new ASCIIEncoding().GetString(SevenZipHelper.Decompress(ReplayCompressed)));

                score.Replay = new LegacyReplay();

                //float lastTime = 0;
                //foreach (var l in File.ReadAllText(@"C:\Users\Dean\Desktop\2157025197").Split(','))
                //{
                //    var split = l.Split('|');

                //    if (split.Length < 4 || float.Parse(split[0]) < 0) continue;

                //    lastTime += float.Parse(split[0]);

                //    list.Add(new LegacyReplay.LegacyReplayInputHandler.LegacyReplayFrame(
                //        lastTime,
                //        float.Parse(split[1]),
                //        384 - float.Parse(split[2]),
                //        (LegacyReplay.LegacyReplayInputHandler.LegacyButtonState)int.Parse(split[3])
                //        ));
                //}
            }

            return score;
        }
    }

    class TestCaseReplay : TestCasePlayer
    {
        private WorkingBeatmap beatmap;

        private InputHandler replay;

        private Func<Stream> getReplayStream;
        private ScoreDatabase scoreDatabase;

        public override string Name => @"Replay";

        public override string Description => @"Testing replay playback.";

        [BackgroundDependencyLoader]
        private void load(Storage storage)
        {
            scoreDatabase = new ScoreDatabase(storage);
        }

        protected override Player CreatePlayer(WorkingBeatmap beatmap)
        {
            var player = base.CreatePlayer(beatmap);
            player.ReplayInputHandler = scoreDatabase.ReadReplayFile(@"Tao - O2i3 - Ooi [Game Edit] [Advanced] (2016-08-08) Osu.osr").Replay.GetInputHandler();
            return player;
        }
    }

    public class LegacyReplay : Replay
    {
        private new List<LegacyReplayFrame> frames = new List<LegacyReplayFrame>();

        public override ReplayInputHandler GetInputHandler() => new LegacyReplayInputHandler(frames);

        /// <summary>
        /// The ReplayHandler will take a replay and handle the propagation of updates to the input stack.
        /// It handles logic of any frames which *must* be executed.
        /// </summary>
        public class LegacyReplayInputHandler : ReplayInputHandler
        {
            private readonly List<LegacyReplayFrame> replayContent;
            int currentFrameIndex;

            public LegacyReplayFrame CurrentFrame => !hasFrames ? null : replayContent[currentFrameIndex];
            public LegacyReplayFrame NextFrame => !hasFrames ? null : replayContent[MathHelper.Clamp(currentDirection > 0 ? currentFrameIndex + 1 : currentFrameIndex - 1, 0, replayContent.Count - 1)];

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
                                new MouseState.ButtonState(MouseButton.Left)
                                {
                                    State = CurrentFrame?.MouseLeft ?? false
                                },
                                new MouseState.ButtonState(MouseButton.Right)
                                {
                                    State = CurrentFrame?.MouseRight ?? false
                                },
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
            public override double? SetFrameFromTime(double time)
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

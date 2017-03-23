// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics.Transforms;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Modes.Osu.Objects;
using osu.Game.Modes.Osu.Objects.Drawables;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Game.Modes.Objects.Types;

namespace osu.Game.Modes.Osu
{
    public class OsuAutoReplay : LegacyReplay
    {
        private static readonly Vector2 spinner_centre = new Vector2(256, 192);

        private const float spin_radius = 50;

        private readonly Beatmap<OsuHitObject> beatmap;

        public OsuAutoReplay(Beatmap<OsuHitObject> beatmap)
        {
            this.beatmap = beatmap;

            createAutoReplay();
        }

        internal class LegacyReplayFrameComparer : IComparer<LegacyReplayFrame>
        {
            public int Compare(LegacyReplayFrame f1, LegacyReplayFrame f2)
            {
                return f1.Time.CompareTo(f2.Time);
            }
        }

        private static readonly IComparer<LegacyReplayFrame> replay_frame_comparer = new LegacyReplayFrameComparer();

        private int findInsertionIndex(LegacyReplayFrame frame)
        {
            int index = Frames.BinarySearch(frame, replay_frame_comparer);

            if (index < 0)
            {
                index = ~index;
            }
            else
            {
                // Go to the first index which is actually bigger
                while (index < Frames.Count && frame.Time == Frames[index].Time)
                {
                    ++index;
                }
            }

            return index;
        }

        private void addFrameToReplay(LegacyReplayFrame frame) => Frames.Insert(findInsertionIndex(frame), frame);

        private static Vector2 circlePosition(double t, double radius) => new Vector2((float)(Math.Cos(t) * radius), (float)(Math.Sin(t) * radius));

        private double applyModsToTime(double v) => v;
        private double applyModsToRate(double v) => v;

        public bool DelayedMovements; // ModManager.CheckActive(Mods.Relax2);

        private void createAutoReplay()
        {
            int buttonIndex = 0;

            EasingTypes preferredEasing = DelayedMovements ? EasingTypes.InOutCubic : EasingTypes.Out;

            addFrameToReplay(new LegacyReplayFrame(-100000, 256, 500, LegacyButtonState.None));
            addFrameToReplay(new LegacyReplayFrame(beatmap.HitObjects[0].StartTime - 1500, 256, 500, LegacyButtonState.None));
            addFrameToReplay(new LegacyReplayFrame(beatmap.HitObjects[0].StartTime - 1000, 256, 192, LegacyButtonState.None));

            // We are using ApplyModsToRate and not ApplyModsToTime to counteract the speed up / slow down from HalfTime / DoubleTime so that we remain at a constant framerate of 60 fps.
            float frameDelay = (float)applyModsToRate(1000.0 / 60.0);

            // Already superhuman, but still somewhat realistic
            int reactionTime = (int)applyModsToRate(100);


            for (int i = 0; i < beatmap.HitObjects.Count; i++)
            {
                OsuHitObject h = beatmap.HitObjects[i];

                //if (h.EndTime < InputManager.ReplayStartTime)
                //{
                //    h.IsHit = true;
                //    continue;
                //}

                int endDelay = h is Spinner ? 1 : 0;

                if (DelayedMovements && i > 0)
                {
                    OsuHitObject last = beatmap.HitObjects[i - 1];

                    double endTime = (last as IHasEndTime)?.EndTime ?? last.StartTime;

                    //Make the cursor stay at a hitObject as long as possible (mainly for autopilot).
                    if (h.StartTime - h.HitWindowFor(OsuScoreResult.Miss) > endTime + h.HitWindowFor(OsuScoreResult.Hit50) + 50)
                    {
                        if (!(last is Spinner) && h.StartTime - endTime < 1000) addFrameToReplay(new LegacyReplayFrame(endTime + h.HitWindowFor(OsuScoreResult.Hit50), last.EndPosition.X, last.EndPosition.Y, LegacyButtonState.None));
                        if (!(h is Spinner)) addFrameToReplay(new LegacyReplayFrame(h.StartTime - h.HitWindowFor(OsuScoreResult.Miss), h.Position.X, h.Position.Y, LegacyButtonState.None));
                    }
                    else if (h.StartTime - h.HitWindowFor(OsuScoreResult.Hit50) > endTime + h.HitWindowFor(OsuScoreResult.Hit50) + 50)
                    {
                        if (!(last is Spinner) && h.StartTime - endTime < 1000) addFrameToReplay(new LegacyReplayFrame(endTime + h.HitWindowFor(OsuScoreResult.Hit50), last.EndPosition.X, last.EndPosition.Y, LegacyButtonState.None));
                        if (!(h is Spinner)) addFrameToReplay(new LegacyReplayFrame(h.StartTime - h.HitWindowFor(OsuScoreResult.Hit50), h.Position.X, h.Position.Y, LegacyButtonState.None));
                    }
                    else if (h.StartTime - h.HitWindowFor(OsuScoreResult.Hit100) > endTime + h.HitWindowFor(OsuScoreResult.Hit100) + 50)
                    {
                        if (!(last is Spinner) && h.StartTime - endTime < 1000) addFrameToReplay(new LegacyReplayFrame(endTime + h.HitWindowFor(OsuScoreResult.Hit100), last.EndPosition.X, last.EndPosition.Y, LegacyButtonState.None));
                        if (!(h is Spinner)) addFrameToReplay(new LegacyReplayFrame(h.StartTime - h.HitWindowFor(OsuScoreResult.Hit100), h.Position.X, h.Position.Y, LegacyButtonState.None));
                    }
                }


                Vector2 targetPosition = h.Position;
                EasingTypes easing = preferredEasing;
                float spinnerDirection = -1;

                if (h is Spinner)
                {
                    targetPosition.X = Frames[Frames.Count - 1].MouseX;
                    targetPosition.Y = Frames[Frames.Count - 1].MouseY;

                    Vector2 difference = spinner_centre - targetPosition;

                    float differenceLength = difference.Length;
                    float newLength = (float)Math.Sqrt(differenceLength * differenceLength - spin_radius * spin_radius);

                    if (differenceLength > spin_radius)
                    {
                        float angle = (float)Math.Asin(spin_radius / differenceLength);

                        if (angle > 0)
                        {
                            spinnerDirection = -1;
                        }
                        else
                        {
                            spinnerDirection = 1;
                        }

                        difference.X = difference.X * (float)Math.Cos(angle) - difference.Y * (float)Math.Sin(angle);
                        difference.Y = difference.X * (float)Math.Sin(angle) + difference.Y * (float)Math.Cos(angle);

                        difference.Normalize();
                        difference *= newLength;

                        targetPosition += difference;

                        easing = EasingTypes.In;
                    }
                    else if (difference.Length > 0)
                    {
                        targetPosition = spinner_centre - difference * (spin_radius / difference.Length);
                    }
                    else
                    {
                        targetPosition = spinner_centre + new Vector2(0, -spin_radius);
                    }
                }


                // Do some nice easing for cursor movements
                if (Frames.Count > 0)
                {
                    LegacyReplayFrame lastFrame = Frames[Frames.Count - 1];

                    // Wait until Auto could "see and react" to the next note.
                    double waitTime = h.StartTime - Math.Max(0.0, DrawableOsuHitObject.TIME_PREEMPT - reactionTime);
                    if (waitTime > lastFrame.Time)
                    {
                        lastFrame = new LegacyReplayFrame(waitTime, lastFrame.MouseX, lastFrame.MouseY, lastFrame.ButtonState);
                        addFrameToReplay(lastFrame);
                    }

                    Vector2 lastPosition = new Vector2(lastFrame.MouseX, lastFrame.MouseY);

                    double timeDifference = applyModsToTime(h.StartTime - lastFrame.Time);

                    // Only "snap" to hitcircles if they are far enough apart. As the time between hitcircles gets shorter the snapping threshold goes up.
                    if (timeDifference > 0 && // Sanity checks
                        ((lastPosition - targetPosition).Length > h.Radius * (1.5 + 100.0 / timeDifference) || // Either the distance is big enough
                        timeDifference >= 266)) // ... or the beats are slow enough to tap anyway.
                    {
                        // Perform eased movement
                        for (double time = lastFrame.Time + frameDelay; time < h.StartTime; time += frameDelay)
                        {
                            Vector2 currentPosition = Interpolation.ValueAt(time, lastPosition, targetPosition, lastFrame.Time, h.StartTime, easing);
                            addFrameToReplay(new LegacyReplayFrame((int)time, currentPosition.X, currentPosition.Y, lastFrame.ButtonState));
                        }

                        buttonIndex = 0;
                    }
                    else
                    {
                        buttonIndex++;
                    }
                }

                LegacyButtonState button = buttonIndex % 2 == 0 ? LegacyButtonState.Left1 : LegacyButtonState.Right1;

                double hEndTime = (h as IHasEndTime)?.EndTime ?? h.StartTime;

                LegacyReplayFrame newFrame = new LegacyReplayFrame(h.StartTime, targetPosition.X, targetPosition.Y, button);
                LegacyReplayFrame endFrame = new LegacyReplayFrame(hEndTime + endDelay, h.EndPosition.X, h.EndPosition.Y, LegacyButtonState.None);

                // Decrement because we want the previous frame, not the next one
                int index = findInsertionIndex(newFrame) - 1;

                // Do we have a previous frame? No need to check for < replay.Count since we decremented!
                if (index >= 0)
                {
                    LegacyReplayFrame previousFrame = Frames[index];
                    var previousButton = previousFrame.ButtonState;

                    // If a button is already held, then we simply alternate
                    if (previousButton != LegacyButtonState.None)
                    {
                        Debug.Assert(previousButton != (LegacyButtonState.Left1 | LegacyButtonState.Right1));

                        // Force alternation if we have the same button. Otherwise we can just keep the naturally to us assigned button.
                        if (previousButton == button)
                        {
                            button = (LegacyButtonState.Left1 | LegacyButtonState.Right1) & ~button;
                            newFrame.SetButtonStates(button);
                        }

                        // We always follow the most recent slider / spinner, so remove any other frames that occur while it exists.
                        int endIndex = findInsertionIndex(endFrame);

                        if (index < Frames.Count - 1)
                            Frames.RemoveRange(index + 1, Math.Max(0, endIndex - (index + 1)));

                        // After alternating we need to keep holding the other button in the future rather than the previous one.
                        for (int j = index + 1; j < Frames.Count; ++j)
                        {
                            // Don't affect frames which stop pressing a button!
                            if (j < Frames.Count - 1 || Frames[j].ButtonState == previousButton)
                                Frames[j].SetButtonStates(button);
                        }
                    }
                }

                addFrameToReplay(newFrame);

                // We add intermediate frames for spinning / following a slider here.
                if (h is Spinner)
                {
                    Spinner s = h as Spinner;

                    Vector2 difference = targetPosition - spinner_centre;

                    float radius = difference.Length;
                    float angle = radius == 0 ? 0 : (float)Math.Atan2(difference.Y, difference.X);

                    double t;

                    for (double j = h.StartTime + frameDelay; j < s.EndTime; j += frameDelay)
                    {
                        t = applyModsToTime(j - h.StartTime) * spinnerDirection;

                        Vector2 pos = spinner_centre + circlePosition(t / 20 + angle, spin_radius);
                        addFrameToReplay(new LegacyReplayFrame((int)j, pos.X, pos.Y, button));
                    }

                    t = applyModsToTime(s.EndTime - h.StartTime) * spinnerDirection;
                    Vector2 endPosition = spinner_centre + circlePosition(t / 20 + angle, spin_radius);

                    addFrameToReplay(new LegacyReplayFrame(s.EndTime, endPosition.X, endPosition.Y, button));

                    endFrame.MouseX = endPosition.X;
                    endFrame.MouseY = endPosition.Y;
                }
                else if (h is Slider)
                {
                    Slider s = h as Slider;

                    for (double j = frameDelay; j < s.Duration; j += frameDelay)
                    {
                        Vector2 pos = s.PositionAt(j / s.Duration);
                        addFrameToReplay(new LegacyReplayFrame(h.StartTime + j, pos.X, pos.Y, button));
                    }

                    addFrameToReplay(new LegacyReplayFrame(s.EndTime, s.EndPosition.X, s.EndPosition.Y, button));
                }

                // We only want to let go of our button if we are at the end of the current replay. Otherwise something is still going on after us so we need to keep the button pressed!
                if (Frames[Frames.Count - 1].Time <= endFrame.Time)
                    addFrameToReplay(endFrame);
            }

            //Player.currentScore.Replay = InputManager.ReplayScore.Replay;
            //Player.currentScore.PlayerName = "osu!";
        }
    }
}

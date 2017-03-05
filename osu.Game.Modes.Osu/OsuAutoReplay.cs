// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Modes.Osu.Objects;
using OpenTK;
using System;
using osu.Framework.Graphics.Transforms;
using osu.Game.Modes.Osu.Objects.Drawables;
using osu.Framework.MathUtils;
using System.Diagnostics;

namespace osu.Game.Modes.Osu
{
    public class OsuAutoReplay : LegacyReplay
    {
        private Beatmap beatmap;

        public OsuAutoReplay(Beatmap beatmap)
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

        private static IComparer<LegacyReplayFrame> replayFrameComparer = new LegacyReplayFrameComparer();

        private static int FindInsertionIndex(List<LegacyReplayFrame> replay, LegacyReplayFrame frame)
        {
            int index = replay.BinarySearch(frame, replayFrameComparer);

            if (index < 0)
            {
                index = ~index;
            }
            else
            {
                // Go to the first index which is actually bigger
                while (index < replay.Count && frame.Time == replay[index].Time)
                {
                    ++index;
                }
            }

            return index;
        }

        private static void AddFrameToReplay(List<LegacyReplayFrame> replay, LegacyReplayFrame frame)
        {
            replay.Insert(FindInsertionIndex(replay, frame), frame);
        }

        private static Vector2 CirclePosition(double t, double radius)
        {
            return new Vector2((float)(Math.Cos(t) * radius), (float)(Math.Sin(t) * radius));
        }

        private void createAutoReplay()
        {
            int buttonIndex = 0;

            bool delayedMovements = false;// ModManager.CheckActive(Mods.Relax2);
            EasingTypes preferredEasing = delayedMovements ? EasingTypes.InOutCubic : EasingTypes.Out;

            AddFrameToReplay(Frames, new LegacyReplayFrame(-100000, 256, 500, LegacyButtonState.None));
            AddFrameToReplay(Frames, new LegacyReplayFrame(beatmap.HitObjects[0].StartTime - 1500, 256, 500, LegacyButtonState.None));
            AddFrameToReplay(Frames, new LegacyReplayFrame(beatmap.HitObjects[0].StartTime - 1000, 256, 192, LegacyButtonState.None));

            // We are using ApplyModsToRate and not ApplyModsToTime to counteract the speed up / slow down from HalfTime / DoubleTime so that we remain at a constant framerate of 60 fps.
            float frameDelay = (float)applyModsToRate(1000.0 / 60.0);
            Vector2 spinnerCentre = new Vector2(256, 192);
            const float spinnerRadius = 50;

            // Already superhuman, but still somewhat realistic
            int reactionTime = (int)applyModsToRate(100);


            for (int i = 0; i < beatmap.HitObjects.Count; i++)
            {
                OsuHitObject h = beatmap.HitObjects[i] as OsuHitObject;

                //if (h.EndTime < InputManager.ReplayStartTime)
                //{
                //    h.IsHit = true;
                //    continue;
                //}

                int endDelay = h is Spinner ? 1 : 0;

                if (delayedMovements && i > 0)
                {
                    OsuHitObject last = beatmap.HitObjects[i - 1] as OsuHitObject;

                    //Make the cursor stay at a hitObject as long as possible (mainly for autopilot).
                    if (h.StartTime - DrawableHitCircle.HITTABLE_RANGE > last.EndTime + DrawableHitCircle.HIT_WINDOW_50 + 50)
                    {
                        if (!(last is Spinner) && h.StartTime - last.EndTime < 1000) AddFrameToReplay(Frames, new LegacyReplayFrame(last.EndTime + DrawableHitCircle.HIT_WINDOW_50, last.EndPosition.X, last.EndPosition.Y, LegacyButtonState.None));
                        if (!(h is Spinner)) AddFrameToReplay(Frames, new LegacyReplayFrame(h.StartTime - DrawableHitCircle.HITTABLE_RANGE, h.Position.X, h.Position.Y, LegacyButtonState.None));
                    }
                    else if (h.StartTime - DrawableHitCircle.HIT_WINDOW_50 > last.EndTime + DrawableHitCircle.HIT_WINDOW_50 + 50)
                    {
                        if (!(last is Spinner) && h.StartTime - last.EndTime < 1000) AddFrameToReplay(Frames, new LegacyReplayFrame(last.EndTime + DrawableHitCircle.HIT_WINDOW_50, last.EndPosition.X, last.EndPosition.Y, LegacyButtonState.None));
                        if (!(h is Spinner)) AddFrameToReplay(Frames, new LegacyReplayFrame(h.StartTime - DrawableHitCircle.HIT_WINDOW_50, h.Position.X, h.Position.Y, LegacyButtonState.None));
                    }
                    else if (h.StartTime - DrawableHitCircle.HIT_WINDOW_100 > last.EndTime + DrawableHitCircle.HIT_WINDOW_100 + 50)
                    {
                        if (!(last is Spinner) && h.StartTime - last.EndTime < 1000) AddFrameToReplay(Frames, new LegacyReplayFrame(last.EndTime + DrawableHitCircle.HIT_WINDOW_100, last.EndPosition.X, last.EndPosition.Y, LegacyButtonState.None));
                        if (!(h is Spinner)) AddFrameToReplay(Frames, new LegacyReplayFrame(h.StartTime - DrawableHitCircle.HIT_WINDOW_100, h.Position.X, h.Position.Y, LegacyButtonState.None));
                    }
                }


                Vector2 targetPosition = h.Position;
                EasingTypes easing = preferredEasing;
                float spinnerDirection = -1;

                if (h is Spinner)
                {
                    targetPosition.X = Frames[Frames.Count - 1].MouseX;
                    targetPosition.Y = Frames[Frames.Count - 1].MouseY;

                    Vector2 difference = spinnerCentre - targetPosition;

                    float differenceLength = difference.Length;
                    float newLength = (float)Math.Sqrt(differenceLength * differenceLength - spinnerRadius * spinnerRadius);

                    if (differenceLength > spinnerRadius)
                    {
                        float angle = (float)Math.Asin(spinnerRadius / differenceLength);

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
                        targetPosition = spinnerCentre - difference * (spinnerRadius / difference.Length);
                    }
                    else
                    {
                        targetPosition = spinnerCentre + new Vector2(0, -spinnerRadius);
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
                        AddFrameToReplay(Frames, lastFrame);
                    }

                    Vector2 lastPosition = new Vector2(lastFrame.MouseX, lastFrame.MouseY);

                    double timeDifference = applyModsToTime(h.StartTime - lastFrame.Time);

                    // Only "snap" to hitcircles if they are far enough apart. As the time between hitcircles gets shorter the snapping threshold goes up.
                    if (timeDifference > 0 && // Sanity checks
                        ((lastPosition - targetPosition).Length > DrawableHitCircle.CIRCLE_RADIUS * (1.5 + 100.0 / timeDifference) || // Either the distance is big enough
                        timeDifference >= 266)) // ... or the beats are slow enough to tap anyway.
                    {
                        // Perform eased movement
                        for (double time = lastFrame.Time + frameDelay; time < h.StartTime; time += frameDelay)
                        {
                            Vector2 currentPosition = Interpolation.ValueAt(time, lastPosition, targetPosition, lastFrame.Time, h.StartTime, easing);
                            AddFrameToReplay(Frames, new LegacyReplayFrame((int)time, currentPosition.X, currentPosition.Y, lastFrame.ButtonState));
                        }

                        buttonIndex = 0;
                    }
                    else
                    {
                        buttonIndex++;
                    }
                }

                LegacyButtonState button = buttonIndex % 2 == 0 ? LegacyButtonState.Left1 : LegacyButtonState.Right1;
                LegacyButtonState previousButton = LegacyButtonState.None;

                LegacyReplayFrame newFrame = new LegacyReplayFrame(h.StartTime, targetPosition.X, targetPosition.Y, button);
                LegacyReplayFrame endFrame = new LegacyReplayFrame(h.EndTime + endDelay, h.EndPosition.X, h.EndPosition.Y, LegacyButtonState.None);

                // Decrement because we want the previous frame, not the next one
                int index = FindInsertionIndex(Frames, newFrame) - 1;

                // Do we have a previous frame? No need to check for < replay.Count since we decremented!
                if (index >= 0)
                {
                    LegacyReplayFrame previousFrame = Frames[index];
                    previousButton = previousFrame.ButtonState;

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
                        int endIndex = FindInsertionIndex(Frames, endFrame);

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

                AddFrameToReplay(Frames, newFrame);

                // We add intermediate frames for spinning / following a slider here.
                if (h is Spinner)
                {
                    Vector2 difference = targetPosition - spinnerCentre;

                    float radius = difference.Length;
                    float angle = radius == 0 ? 0 : (float)Math.Atan2(difference.Y, difference.X);

                    double t;

                    for (double j = h.StartTime + frameDelay; j < h.EndTime; j += frameDelay)
                    {
                        t = applyModsToTime(j - h.StartTime) * spinnerDirection;

                        Vector2 pos = spinnerCentre + CirclePosition(t / 20 + angle, spinnerRadius);
                        AddFrameToReplay(Frames, new LegacyReplayFrame((int)j, pos.X, pos.Y, button));
                    }

                    t = applyModsToTime(h.EndTime - h.StartTime) * spinnerDirection;
                    Vector2 endPosition = spinnerCentre + CirclePosition(t / 20 + angle, spinnerRadius);

                    AddFrameToReplay(Frames, new LegacyReplayFrame(h.EndTime, endPosition.X, endPosition.Y, button));

                    endFrame.MouseX = endPosition.X;
                    endFrame.MouseY = endPosition.Y;
                }
                else if (h is Slider)
                {
                    Slider s = h as Slider;
                    int lastTime = 0;

                    //foreach (
                    //    Transformation t in
                    //        s..Transformations.FindAll(
                    //            tr => tr.Type == TransformationType.Movement))
                    //{
                    //    if (lastTime != 0 && t.Time1 - lastTime < frameDelay) continue;

                    //    AddFrameToReplay(Frames, new LegacyReplayFrame(t.Time1, t.StartVector.X, t.StartVector.Y,
                    //                                button));
                    //    lastTime = t.Time1;
                    //}

                    AddFrameToReplay(Frames, new LegacyReplayFrame(h.EndTime, s.EndPosition.X, s.EndPosition.Y, button));
                }

                // We only want to let go of our button if we are at the end of the current replay. Otherwise something is still going on after us so we need to keep the button pressed!
                if (Frames[Frames.Count - 1].Time <= endFrame.Time)
                {
                    AddFrameToReplay(Frames, endFrame);
                }
            }

            //Player.currentScore.Replay = InputManager.ReplayScore.Replay;
            //Player.currentScore.PlayerName = "osu!";
        }

        private double applyModsToTime(double v) => v;
        private double applyModsToRate(double v) => v;
    }
}

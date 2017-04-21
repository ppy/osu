// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Replays;
using osu.Game.Users;

namespace osu.Game.Rulesets.Osu
{
    public class OsuAutoReplay : Replay
    {
        private static readonly Vector2 spinner_centre = new Vector2(256, 192);

        private const float spin_radius = 50;

        private readonly Beatmap<OsuHitObject> beatmap;

        // ms between each ReplayFrame
        private readonly float frameDelay;

        // ms between 'seeing' a new hitobject and auto moving to 'react' to it
        private readonly int reactionTime;

        // What easing to use when moving between hitobjects
        private EasingTypes preferredEasing;

        // Even means LMB will be used to click, odd means RMB will be used.
        // This keeps track of the button previously used for alt/singletap logic.
        private int buttonIndex;

        public OsuAutoReplay(Beatmap<OsuHitObject> beatmap)
        {
            this.beatmap = beatmap;

            User = new User
            {
                Username = @"Autoplay",
            };

            // We are using ApplyModsToRate and not ApplyModsToTime to counteract the speed up / slow down from HalfTime / DoubleTime so that we remain at a constant framerate of 60 fps.
            frameDelay = (float)applyModsToRate(1000.0 / 60.0);

            // Already superhuman, but still somewhat realistic
            reactionTime = (int)applyModsToRate(100);

            createAutoReplay();
        }

        private class ReplayFrameComparer : IComparer<ReplayFrame>
        {
            public int Compare(ReplayFrame f1, ReplayFrame f2)
            {
                if (f1 == null) throw new NullReferenceException($@"{nameof(f1)} cannot be null");
                if (f2 == null) throw new NullReferenceException($@"{nameof(f2)} cannot be null");

                return f1.Time.CompareTo(f2.Time);
            }
        }

        private static readonly IComparer<ReplayFrame> replay_frame_comparer = new ReplayFrameComparer();

        private int findInsertionIndex(ReplayFrame frame)
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

        private void addFrameToReplay(ReplayFrame frame) => Frames.Insert(findInsertionIndex(frame), frame);

        private static Vector2 circlePosition(double t, double radius) => new Vector2((float)(Math.Cos(t) * radius), (float)(Math.Sin(t) * radius));

        private double applyModsToTime(double v) => v;
        private double applyModsToRate(double v) => v;

        public bool DelayedMovements; // ModManager.CheckActive(Mods.Relax2);

        private void createAutoReplay()
        {
            buttonIndex = 0;

            preferredEasing = DelayedMovements ? EasingTypes.InOutCubic : EasingTypes.Out;

            addFrameToReplay(new ReplayFrame(-100000, 256, 500, ReplayButtonState.None));
            addFrameToReplay(new ReplayFrame(beatmap.HitObjects[0].StartTime - 1500, 256, 500, ReplayButtonState.None));
            addFrameToReplay(new ReplayFrame(beatmap.HitObjects[0].StartTime - 1000, 256, 192, ReplayButtonState.None));


            for (int i = 0; i < beatmap.HitObjects.Count; i++)
            {
                OsuHitObject h = beatmap.HitObjects[i];

                //if (h.EndTime < InputManager.ReplayStartTime)
                //{
                //    h.IsHit = true;
                //    continue;
                //}

                if (DelayedMovements && i > 0)
                {
                    OsuHitObject prev = beatmap.HitObjects[i - 1];
                    addDelayedMovements(h, prev);
                }

                addHitObjectReplay(h);
            }

            //Player.currentScore.Replay = InputManager.ReplayScore.Replay;
            //Player.currentScore.PlayerName = "osu!";
        }

        private void addDelayedMovements(OsuHitObject h, OsuHitObject prev)
        {
            double endTime = (prev as IHasEndTime)?.EndTime ?? prev.StartTime;

            // Make the cursor stay at a hitObject as long as possible (mainly for autopilot).
            if (h.StartTime - h.HitWindowFor(OsuScoreResult.Miss) > endTime + h.HitWindowFor(OsuScoreResult.Hit50) + 50)
            {
                if (!(prev is Spinner) && h.StartTime - endTime < 1000) addFrameToReplay(new ReplayFrame(endTime + h.HitWindowFor(OsuScoreResult.Hit50), prev.EndPosition.X, prev.EndPosition.Y, ReplayButtonState.None));
                if (!(h is Spinner)) addFrameToReplay(new ReplayFrame(h.StartTime - h.HitWindowFor(OsuScoreResult.Miss), h.Position.X, h.Position.Y, ReplayButtonState.None));
            }
            else if (h.StartTime - h.HitWindowFor(OsuScoreResult.Hit50) > endTime + h.HitWindowFor(OsuScoreResult.Hit50) + 50)
            {
                if (!(prev is Spinner) && h.StartTime - endTime < 1000) addFrameToReplay(new ReplayFrame(endTime + h.HitWindowFor(OsuScoreResult.Hit50), prev.EndPosition.X, prev.EndPosition.Y, ReplayButtonState.None));
                if (!(h is Spinner)) addFrameToReplay(new ReplayFrame(h.StartTime - h.HitWindowFor(OsuScoreResult.Hit50), h.Position.X, h.Position.Y, ReplayButtonState.None));
            }
            else if (h.StartTime - h.HitWindowFor(OsuScoreResult.Hit100) > endTime + h.HitWindowFor(OsuScoreResult.Hit100) + 50)
            {
                if (!(prev is Spinner) && h.StartTime - endTime < 1000) addFrameToReplay(new ReplayFrame(endTime + h.HitWindowFor(OsuScoreResult.Hit100), prev.EndPosition.X, prev.EndPosition.Y, ReplayButtonState.None));
                if (!(h is Spinner)) addFrameToReplay(new ReplayFrame(h.StartTime - h.HitWindowFor(OsuScoreResult.Hit100), h.Position.X, h.Position.Y, ReplayButtonState.None));
            }
        }

        private void addHitObjectReplay(OsuHitObject h)
        {
            // Default values for circles/sliders
            Vector2 startPosition = h.Position;
            EasingTypes easing = preferredEasing;
            float spinnerDirection = -1;

            // The startPosition for the slider should not be its .Position, but the point on the circle whose tangent crosses the current cursor position
            // We also modify spinnerDirection so it spins in the direction it enters the spin circle, to make a smooth transition.
            // TODO: Shouldn't the spinner always spin in the same direction?
            if (h is Spinner)
            {
                calcSpinnerStartPosAndDirection(Frames[Frames.Count - 1].Position, out startPosition, out spinnerDirection);

                Vector2 spinCentreOffset = spinner_centre - Frames[Frames.Count - 1].Position;

                if (spinCentreOffset.Length > spin_radius)
                {
                    // If moving in from the outside, don't ease out (default eases out). This means auto will "start" spinning immediately after moving into position.
                    easing = EasingTypes.In;
                }
            }


            // Do some nice easing for cursor movements
            if (Frames.Count > 0)
            {
                moveToHitObject(h.StartTime, startPosition, h.Radius, easing);
            }

            // Add frames to click the hitobject
            addHitObjectClickFrames(h, startPosition, spinnerDirection);
        }


        private static void calcSpinnerStartPosAndDirection(Vector2 prevPos, out Vector2 startPosition, out float spinnerDirection)
        {
            Vector2 spinCentreOffset = spinner_centre - prevPos;
            float distFromCentre = spinCentreOffset.Length;
            float distToTangentPoint = (float)Math.Sqrt(distFromCentre * distFromCentre - spin_radius * spin_radius);

            if (distFromCentre > spin_radius)
            {
                // Previous cursor position was outside spin circle, set startPosition to the tangent point.

                // Angle between centre offset and tangent point offset.
                float angle = (float)Math.Asin(spin_radius / distFromCentre);

                if (angle > 0)
                {
                    spinnerDirection = -1;
                }
                else
                {
                    spinnerDirection = 1;
                }

                // Rotate by angle so it's parallel to tangent line
                spinCentreOffset.X = spinCentreOffset.X * (float)Math.Cos(angle) - spinCentreOffset.Y * (float)Math.Sin(angle);
                spinCentreOffset.Y = spinCentreOffset.X * (float)Math.Sin(angle) + spinCentreOffset.Y * (float)Math.Cos(angle);

                // Set length to distToTangentPoint
                spinCentreOffset.Normalize();
                spinCentreOffset *= distToTangentPoint;

                // Move along the tangent line, now startPosition is at the tangent point.
                startPosition = prevPos + spinCentreOffset;
            }
            else if (spinCentreOffset.Length > 0)
            {
                // Previous cursor position was inside spin circle, set startPosition to the nearest point on spin circle.
                startPosition = spinner_centre - spinCentreOffset * (spin_radius / spinCentreOffset.Length);
                spinnerDirection = 1;
            }
            else
            {
                // Degenerate case where cursor position is exactly at the centre of the spin circle.
                startPosition = spinner_centre + new Vector2(0, -spin_radius);
                spinnerDirection = 1;
            }
        }

        private void moveToHitObject(double targetTime, Vector2 targetPos, double hitObjectRadius, EasingTypes easing)
        {
            ReplayFrame lastFrame = Frames[Frames.Count - 1];

            // Wait until Auto could "see and react" to the next note.
            double waitTime = targetTime - Math.Max(0.0, DrawableOsuHitObject.TIME_PREEMPT - reactionTime);
            if (waitTime > lastFrame.Time)
            {
                lastFrame = new ReplayFrame(waitTime, lastFrame.MouseX, lastFrame.MouseY, lastFrame.ButtonState);
                addFrameToReplay(lastFrame);
            }

            Vector2 lastPosition = lastFrame.Position;

            double timeDifference = applyModsToTime(targetTime - lastFrame.Time);

            // Only "snap" to hitcircles if they are far enough apart. As the time between hitcircles gets shorter the snapping threshold goes up.
            if (timeDifference > 0 && // Sanity checks
                ((lastPosition - targetPos).Length > hitObjectRadius * (1.5 + 100.0 / timeDifference) || // Either the distance is big enough
                timeDifference >= 266)) // ... or the beats are slow enough to tap anyway.
            {
                // Perform eased movement
                for (double time = lastFrame.Time + frameDelay; time < targetTime; time += frameDelay)
                {
                    Vector2 currentPosition = Interpolation.ValueAt(time, lastPosition, targetPos, lastFrame.Time, targetTime, easing);
                    addFrameToReplay(new ReplayFrame((int)time, currentPosition.X, currentPosition.Y, lastFrame.ButtonState));
                }

                buttonIndex = 0;
            }
            else
            {
                buttonIndex++;
            }
        }

        // Add frames to click the hitobject
        private void addHitObjectClickFrames(OsuHitObject h, Vector2 startPosition, float spinnerDirection)
        {
            // Time to insert the first frame which clicks the object
            // Here we mainly need to determine which button to use
            ReplayButtonState button = buttonIndex % 2 == 0 ? ReplayButtonState.Left1 : ReplayButtonState.Right1;

            ReplayFrame startFrame = new ReplayFrame(h.StartTime, startPosition.X, startPosition.Y, button);

            // TODO: Why do we delay 1 ms if the object is a spinner? There already is KEY_UP_DELAY from hEndTime.
            double hEndTime = ((h as IHasEndTime)?.EndTime ?? h.StartTime) + KEY_UP_DELAY;
            int endDelay = h is Spinner ? 1 : 0;
            ReplayFrame endFrame = new ReplayFrame(hEndTime + endDelay, h.EndPosition.X, h.EndPosition.Y, ReplayButtonState.None);

            // Decrement because we want the previous frame, not the next one
            int index = findInsertionIndex(startFrame) - 1;

            // If the previous frame has a button pressed, force alternation.
            // If there are frames ahead, modify those to use the new button press.
            // Do we have a previous frame? No need to check for < replay.Count since we decremented!
            if (index >= 0)
            {
                ReplayFrame previousFrame = Frames[index];
                var previousButton = previousFrame.ButtonState;

                // If a button is already held, then we simply alternate
                if (previousButton != ReplayButtonState.None)
                {
                    Debug.Assert(previousButton != (ReplayButtonState.Left1 | ReplayButtonState.Right1));

                    // Force alternation if we have the same button. Otherwise we can just keep the naturally to us assigned button.
                    if (previousButton == button)
                    {
                        button = (ReplayButtonState.Left1 | ReplayButtonState.Right1) & ~button;
                        startFrame.ButtonState = button;
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
                            Frames[j].ButtonState = button;
                    }
                }
            }

            addFrameToReplay(startFrame);

            // We add intermediate frames for spinning / following a slider here.
            if (h is Spinner)
            {
                Spinner s = h as Spinner;

                Vector2 difference = startPosition - spinner_centre;

                float radius = difference.Length;
                float angle = radius == 0 ? 0 : (float)Math.Atan2(difference.Y, difference.X);

                double t;

                for (double j = h.StartTime + frameDelay; j < s.EndTime; j += frameDelay)
                {
                    t = applyModsToTime(j - h.StartTime) * spinnerDirection;

                    Vector2 pos = spinner_centre + circlePosition(t / 20 + angle, spin_radius);
                    addFrameToReplay(new ReplayFrame((int)j, pos.X, pos.Y, button));
                }

                t = applyModsToTime(s.EndTime - h.StartTime) * spinnerDirection;
                Vector2 endPosition = spinner_centre + circlePosition(t / 20 + angle, spin_radius);

                addFrameToReplay(new ReplayFrame(s.EndTime, endPosition.X, endPosition.Y, button));

                endFrame.MouseX = endPosition.X;
                endFrame.MouseY = endPosition.Y;
            }
            else if (h is Slider)
            {
                Slider s = h as Slider;

                for (double j = frameDelay; j < s.Duration; j += frameDelay)
                {
                    Vector2 pos = s.PositionAt(j / s.Duration);
                    addFrameToReplay(new ReplayFrame(h.StartTime + j, pos.X, pos.Y, button));
                }

                addFrameToReplay(new ReplayFrame(s.EndTime, s.EndPosition.X, s.EndPosition.Y, button));
            }

            // We only want to let go of our button if we are at the end of the current replay. Otherwise something is still going on after us so we need to keep the button pressed!
            if (Frames[Frames.Count - 1].Time <= endFrame.Time)
                addFrameToReplay(endFrame);
        }
    }
}

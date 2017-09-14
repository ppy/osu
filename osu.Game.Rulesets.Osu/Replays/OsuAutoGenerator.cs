// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using System;
using System.Diagnostics;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Osu.Replays
{
    public class OsuAutoGenerator : OsuAutoGeneratorBase
    {
        #region Parameters

        /// <summary>
        /// If delayed movements should be used, causing the cursor to stay on each hitobject for as long as possible.
        /// Mainly for Autopilot.
        /// </summary>
        public bool DelayedMovements; // ModManager.CheckActive(Mods.Relax2);

        #endregion

        #region Constants

        /// <summary>
        /// The "reaction time" in ms between "seeing" a new hit object and moving to "react" to it.
        /// </summary>
        private readonly double reactionTime;

        /// <summary>
        /// What easing to use when moving between hitobjects
        /// </summary>
        private Easing preferredEasing => DelayedMovements ? Easing.InOutCubic : Easing.Out;

        #endregion

        #region Construction / Initialisation

        public OsuAutoGenerator(Beatmap<OsuHitObject> beatmap)
            : base(beatmap)
        {
            // Already superhuman, but still somewhat realistic
            reactionTime = ApplyModsToRate(100);
        }

        #endregion

        #region Generator

        /// <summary>
        /// Which button (left or right) to use for the current hitobject.
        /// Even means LMB will be used to click, odd means RMB will be used.
        /// This keeps track of the button previously used for alt/singletap logic.
        /// </summary>
        private int buttonIndex;

        public override Replay Generate()
        {
            buttonIndex = 0;

            AddFrameToReplay(new ReplayFrame(-100000, 256, 500, ReplayButtonState.None));
            AddFrameToReplay(new ReplayFrame(Beatmap.HitObjects[0].StartTime - 1500, 256, 500, ReplayButtonState.None));
            AddFrameToReplay(new ReplayFrame(Beatmap.HitObjects[0].StartTime - 1000, 256, 192, ReplayButtonState.None));

            for (int i = 0; i < Beatmap.HitObjects.Count; i++)
            {
                OsuHitObject h = Beatmap.HitObjects[i];

                if (DelayedMovements && i > 0)
                {
                    OsuHitObject prev = Beatmap.HitObjects[i - 1];
                    addDelayedMovements(h, prev);
                }

                addHitObjectReplay(h);
            }

            return Replay;
        }

        private void addDelayedMovements(OsuHitObject h, OsuHitObject prev)
        {
            double endTime = (prev as IHasEndTime)?.EndTime ?? prev.StartTime;

            // Make the cursor stay at a hitObject as long as possible (mainly for autopilot).
            if (h.StartTime - h.HitWindowFor(HitResult.Miss) > endTime + h.HitWindowFor(HitResult.Meh) + 50)
            {
                if (!(prev is Spinner) && h.StartTime - endTime < 1000) AddFrameToReplay(new ReplayFrame(endTime + h.HitWindowFor(HitResult.Meh), prev.StackedEndPosition.X, prev.StackedEndPosition.Y, ReplayButtonState.None));
                if (!(h is Spinner)) AddFrameToReplay(new ReplayFrame(h.StartTime - h.HitWindowFor(HitResult.Miss), h.StackedPosition.X, h.StackedPosition.Y, ReplayButtonState.None));
            }
            else if (h.StartTime - h.HitWindowFor(HitResult.Meh) > endTime + h.HitWindowFor(HitResult.Meh) + 50)
            {
                if (!(prev is Spinner) && h.StartTime - endTime < 1000) AddFrameToReplay(new ReplayFrame(endTime + h.HitWindowFor(HitResult.Meh), prev.StackedEndPosition.X, prev.StackedEndPosition.Y, ReplayButtonState.None));
                if (!(h is Spinner)) AddFrameToReplay(new ReplayFrame(h.StartTime - h.HitWindowFor(HitResult.Meh), h.StackedPosition.X, h.StackedPosition.Y, ReplayButtonState.None));
            }
            else if (h.StartTime - h.HitWindowFor(HitResult.Good) > endTime + h.HitWindowFor(HitResult.Good) + 50)
            {
                if (!(prev is Spinner) && h.StartTime - endTime < 1000) AddFrameToReplay(new ReplayFrame(endTime + h.HitWindowFor(HitResult.Good), prev.StackedEndPosition.X, prev.StackedEndPosition.Y, ReplayButtonState.None));
                if (!(h is Spinner)) AddFrameToReplay(new ReplayFrame(h.StartTime - h.HitWindowFor(HitResult.Good), h.StackedPosition.X, h.StackedPosition.Y, ReplayButtonState.None));
            }
        }

        private void addHitObjectReplay(OsuHitObject h)
        {
            // Default values for circles/sliders
            Vector2 startPosition = h.StackedPosition;
            Easing easing = preferredEasing;
            float spinnerDirection = -1;

            // The startPosition for the slider should not be its .Position, but the point on the circle whose tangent crosses the current cursor position
            // We also modify spinnerDirection so it spins in the direction it enters the spin circle, to make a smooth transition.
            // TODO: Shouldn't the spinner always spin in the same direction?
            if (h is Spinner)
            {
                calcSpinnerStartPosAndDirection(Frames[Frames.Count - 1].Position, out startPosition, out spinnerDirection);

                Vector2 spinCentreOffset = SPINNER_CENTRE - Frames[Frames.Count - 1].Position;

                if (spinCentreOffset.Length > SPIN_RADIUS)
                {
                    // If moving in from the outside, don't ease out (default eases out). This means auto will "start" spinning immediately after moving into position.
                    easing = Easing.In;
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

        #endregion

        #region Helper subroutines

        private static void calcSpinnerStartPosAndDirection(Vector2 prevPos, out Vector2 startPosition, out float spinnerDirection)
        {
            Vector2 spinCentreOffset = SPINNER_CENTRE - prevPos;
            float distFromCentre = spinCentreOffset.Length;
            float distToTangentPoint = (float)Math.Sqrt(distFromCentre * distFromCentre - SPIN_RADIUS * SPIN_RADIUS);

            if (distFromCentre > SPIN_RADIUS)
            {
                // Previous cursor position was outside spin circle, set startPosition to the tangent point.

                // Angle between centre offset and tangent point offset.
                float angle = (float)Math.Asin(SPIN_RADIUS / distFromCentre);

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
                startPosition = SPINNER_CENTRE - spinCentreOffset * (SPIN_RADIUS / spinCentreOffset.Length);
                spinnerDirection = 1;
            }
            else
            {
                // Degenerate case where cursor position is exactly at the centre of the spin circle.
                startPosition = SPINNER_CENTRE + new Vector2(0, -SPIN_RADIUS);
                spinnerDirection = 1;
            }
        }

        private void moveToHitObject(double targetTime, Vector2 targetPos, double hitObjectRadius, Easing easing)
        {
            ReplayFrame lastFrame = Frames[Frames.Count - 1];

            // Wait until Auto could "see and react" to the next note.
            double waitTime = targetTime - Math.Max(0.0, DrawableOsuHitObject.TIME_PREEMPT - reactionTime);
            if (waitTime > lastFrame.Time)
            {
                lastFrame = new ReplayFrame(waitTime, lastFrame.MouseX, lastFrame.MouseY, lastFrame.ButtonState);
                AddFrameToReplay(lastFrame);
            }

            Vector2 lastPosition = lastFrame.Position;

            double timeDifference = ApplyModsToTime(targetTime - lastFrame.Time);

            // Only "snap" to hitcircles if they are far enough apart. As the time between hitcircles gets shorter the snapping threshold goes up.
            if (timeDifference > 0 && // Sanity checks
                ((lastPosition - targetPos).Length > hitObjectRadius * (1.5 + 100.0 / timeDifference) || // Either the distance is big enough
                timeDifference >= 266)) // ... or the beats are slow enough to tap anyway.
            {
                // Perform eased movement
                for (double time = lastFrame.Time + FrameDelay; time < targetTime; time += FrameDelay)
                {
                    Vector2 currentPosition = Interpolation.ValueAt(time, lastPosition, targetPos, lastFrame.Time, targetTime, easing);
                    AddFrameToReplay(new ReplayFrame((int)time, currentPosition.X, currentPosition.Y, lastFrame.ButtonState));
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
            ReplayFrame endFrame = new ReplayFrame(hEndTime + endDelay, h.StackedEndPosition.X, h.StackedEndPosition.Y, ReplayButtonState.None);

            // Decrement because we want the previous frame, not the next one
            int index = FindInsertionIndex(startFrame) - 1;

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
                    Debug.Assert(previousButton != (ReplayButtonState.Left1 | ReplayButtonState.Right1), "Previous button state was not Left1 nor Right1 despite only using those two states.");

                    // Force alternation if we have the same button. Otherwise we can just keep the naturally to us assigned button.
                    if (previousButton == button)
                    {
                        button = (ReplayButtonState.Left1 | ReplayButtonState.Right1) & ~button;
                        startFrame.ButtonState = button;
                    }

                    // We always follow the most recent slider / spinner, so remove any other frames that occur while it exists.
                    int endIndex = FindInsertionIndex(endFrame);

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

            AddFrameToReplay(startFrame);

            // We add intermediate frames for spinning / following a slider here.
            if (h is Spinner)
            {
                Spinner s = h as Spinner;

                Vector2 difference = startPosition - SPINNER_CENTRE;

                float radius = difference.Length;
                float angle = radius == 0 ? 0 : (float)Math.Atan2(difference.Y, difference.X);

                double t;

                for (double j = h.StartTime + FrameDelay; j < s.EndTime; j += FrameDelay)
                {
                    t = ApplyModsToTime(j - h.StartTime) * spinnerDirection;

                    Vector2 pos = SPINNER_CENTRE + CirclePosition(t / 20 + angle, SPIN_RADIUS);
                    AddFrameToReplay(new ReplayFrame((int)j, pos.X, pos.Y, button));
                }

                t = ApplyModsToTime(s.EndTime - h.StartTime) * spinnerDirection;
                Vector2 endPosition = SPINNER_CENTRE + CirclePosition(t / 20 + angle, SPIN_RADIUS);

                AddFrameToReplay(new ReplayFrame(s.EndTime, endPosition.X, endPosition.Y, button));

                endFrame.MouseX = endPosition.X;
                endFrame.MouseY = endPosition.Y;
            }
            else if (h is Slider)
            {
                Slider s = h as Slider;

                for (double j = FrameDelay; j < s.Duration; j += FrameDelay)
                {
                    Vector2 pos = s.PositionAt(j / s.Duration);
                    AddFrameToReplay(new ReplayFrame(h.StartTime + j, pos.X, pos.Y, button));
                }

                AddFrameToReplay(new ReplayFrame(s.EndTime, s.EndPosition.X, s.EndPosition.Y, button));
            }

            // We only want to let go of our button if we are at the end of the current replay. Otherwise something is still going on after us so we need to keep the button pressed!
            if (Frames[Frames.Count - 1].Time <= endFrame.Time)
                AddFrameToReplay(endFrame);
        }

        #endregion
    }
}

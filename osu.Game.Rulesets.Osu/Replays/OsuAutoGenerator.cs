// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Replays
{
    public class OsuAutoGenerator : OsuAutoGeneratorBase
    {
        public new OsuBeatmap Beatmap => (OsuBeatmap)base.Beatmap;

        #region Parameters

        /// <summary>
        /// If delayed movements should be used, causing the cursor to stay on each hitobject for as long as possible.
        /// Mainly for Autopilot.
        /// </summary>
        public bool DelayedMovements; // ModManager.CheckActive(Mods.Relax2);

        #endregion

        #region Constants

        private readonly HitWindows defaultHitWindows;

        /// <summary>
        /// What easing to use when moving between hitobjects
        /// </summary>
        private Easing preferredEasing => DelayedMovements ? Easing.InOutCubic : Easing.Out;

        #endregion

        #region Construction / Initialisation

        public OsuAutoGenerator(IBeatmap beatmap, IReadOnlyList<Mod> mods)
            : base(beatmap, mods)
        {
            defaultHitWindows = new OsuHitWindows();
            defaultHitWindows.SetDifficulty(Beatmap.Difficulty.OverallDifficulty);
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
            if (Beatmap.HitObjects.Count == 0)
                return Replay;

            buttonIndex = 0;

            AddFrameToReplay(new OsuReplayFrame(Beatmap.HitObjects[0].StartTime - 1500, new Vector2(256, 500)));

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
            double endTime = prev.GetEndTime();

            HitWindows? hitWindows = null;

            switch (h)
            {
                case HitCircle hitCircle:
                    hitWindows = hitCircle.HitWindows;
                    break;

                case Slider slider:
                    hitWindows = slider.TailCircle.HitWindows;
                    break;

                case Spinner:
                    hitWindows = defaultHitWindows;
                    break;
            }

            Debug.Assert(hitWindows != null);

            // Make the cursor stay at a hitObject as long as possible (mainly for autopilot).
            if (h.StartTime - hitWindows.WindowFor(HitResult.Miss) > endTime + hitWindows.WindowFor(HitResult.Meh) + 50)
            {
                if (!(prev is Spinner) && h.StartTime - endTime < 1000)
                    AddFrameToReplay(new OsuReplayFrame(endTime + hitWindows.WindowFor(HitResult.Meh), new Vector2(prev.StackedEndPosition.X, prev.StackedEndPosition.Y)));

                if (!(h is Spinner))
                    AddFrameToReplay(new OsuReplayFrame(h.StartTime - hitWindows.WindowFor(HitResult.Miss), new Vector2(h.StackedPosition.X, h.StackedPosition.Y)));
            }
            else if (h.StartTime - hitWindows.WindowFor(HitResult.Meh) > endTime + hitWindows.WindowFor(HitResult.Meh) + 50)
            {
                if (!(prev is Spinner) && h.StartTime - endTime < 1000)
                    AddFrameToReplay(new OsuReplayFrame(endTime + hitWindows.WindowFor(HitResult.Meh), new Vector2(prev.StackedEndPosition.X, prev.StackedEndPosition.Y)));

                if (!(h is Spinner))
                    AddFrameToReplay(new OsuReplayFrame(h.StartTime - hitWindows.WindowFor(HitResult.Meh), new Vector2(h.StackedPosition.X, h.StackedPosition.Y)));
            }
            else if (h.StartTime - hitWindows.WindowFor(HitResult.Ok) > endTime + hitWindows.WindowFor(HitResult.Ok) + 50)
            {
                if (!(prev is Spinner) && h.StartTime - endTime < 1000)
                    AddFrameToReplay(new OsuReplayFrame(endTime + hitWindows.WindowFor(HitResult.Ok), new Vector2(prev.StackedEndPosition.X, prev.StackedEndPosition.Y)));

                if (!(h is Spinner))
                    AddFrameToReplay(new OsuReplayFrame(h.StartTime - hitWindows.WindowFor(HitResult.Ok), new Vector2(h.StackedPosition.X, h.StackedPosition.Y)));
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
            if (h is Spinner spinner)
            {
                // spinners with 0 spins required will auto-complete - don't bother
                if (spinner.SpinsRequired == 0)
                    return;

                calcSpinnerStartPosAndDirection(((OsuReplayFrame)Frames[^1]).Position, out startPosition, out spinnerDirection);

                Vector2 spinCentreOffset = SPINNER_CENTRE - ((OsuReplayFrame)Frames[^1]).Position;

                if (spinCentreOffset.Length > SPIN_RADIUS)
                {
                    // If moving in from the outside, don't ease out (default eases out). This means auto will "start" spinning immediately after moving into position.
                    easing = Easing.In;
                }
            }

            // Do some nice easing for cursor movements
            if (Frames.Count > 0)
            {
                moveToHitObject(h, startPosition, easing);
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
            float distToTangentPoint = MathF.Sqrt(distFromCentre * distFromCentre - SPIN_RADIUS * SPIN_RADIUS);

            if (distFromCentre > SPIN_RADIUS)
            {
                // Previous cursor position was outside spin circle, set startPosition to the tangent point.

                // Angle between centre offset and tangent point offset.
                float angle = MathF.Asin(SPIN_RADIUS / distFromCentre);

                if (angle > 0)
                {
                    spinnerDirection = -1;
                }
                else
                {
                    spinnerDirection = 1;
                }

                // Rotate by angle so it's parallel to tangent line
                spinCentreOffset.X = spinCentreOffset.X * MathF.Cos(angle) - spinCentreOffset.Y * MathF.Sin(angle);
                spinCentreOffset.Y = spinCentreOffset.X * MathF.Sin(angle) + spinCentreOffset.Y * MathF.Cos(angle);

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

        private void moveToHitObject(OsuHitObject h, Vector2 targetPos, Easing easing)
        {
            OsuReplayFrame lastFrame = (OsuReplayFrame)Frames[^1];

            // Wait until Auto could "see and react" to the next note.
            double waitTime = h.StartTime - Math.Max(0.0, h.TimePreempt - getReactionTime(h.StartTime - h.TimePreempt));
            bool hasWaited = false;

            if (waitTime > lastFrame.Time)
            {
                lastFrame = new OsuReplayFrame(waitTime, lastFrame.Position) { Actions = lastFrame.Actions };
                hasWaited = true;
                AddFrameToReplay(lastFrame);
            }

            double timeDifference = ApplyModsToTimeDelta(lastFrame.Time, h.StartTime);
            OsuReplayFrame? lastLastFrame = Frames.Count >= 2 ? (OsuReplayFrame)Frames[^2] : null;

            if (timeDifference > 0)
            {
                // If the last frame is a key-up frame and there has been no wait period, adjust the last frame's position such that it begins eased movement instantaneously.
                if (lastLastFrame != null && lastFrame is OsuKeyUpReplayFrame && !hasWaited)
                {
                    // [lastLastFrame] ... [lastFrame] ... [current frame]
                    // We want to find the cursor position at lastFrame, so interpolate between lastLastFrame and the new target position.
                    lastFrame.Position = Interpolation.ValueAt(lastFrame.Time, lastFrame.Position, targetPos, lastLastFrame.Time, h.StartTime, easing);
                }

                Vector2 lastPosition = lastFrame.Position;

                // Perform the rest of the eased movement until the target position is reached.
                for (double time = lastFrame.Time + GetFrameDelay(lastFrame.Time); time < h.StartTime; time += GetFrameDelay(time))
                {
                    Vector2 currentPosition = Interpolation.ValueAt(time, lastPosition, targetPos, lastFrame.Time, h.StartTime, easing);
                    AddFrameToReplay(new OsuReplayFrame((int)time, new Vector2(currentPosition.X, currentPosition.Y)) { Actions = lastFrame.Actions });
                }
            }

            // Start alternating once the time separation is too small (faster than ~225BPM).
            if (timeDifference > 0 && timeDifference < 266)
                buttonIndex++;
            else
                buttonIndex = 0;
        }

        /// <summary>
        /// Calculates the "reaction time" in ms between "seeing" a new hit object and moving to "react" to it.
        /// </summary>
        /// <remarks>
        /// Already superhuman, but still somewhat realistic.
        /// </remarks>
        private double getReactionTime(double timeInstant) => ApplyModsToRate(timeInstant, 100);

        // Add frames to click the hitobject
        private void addHitObjectClickFrames(OsuHitObject h, Vector2 startPosition, float spinnerDirection)
        {
            // Time to insert the first frame which clicks the object
            // Here we mainly need to determine which button to use
            var action = buttonIndex % 2 == 0 ? OsuAction.LeftButton : OsuAction.RightButton;

            var startFrame = new OsuReplayFrame(h.StartTime, new Vector2(startPosition.X, startPosition.Y), action);

            // TODO: Why do we delay 1 ms if the object is a spinner? There already is KEY_UP_DELAY from hEndTime.
            double hEndTime = h.GetEndTime() + KEY_UP_DELAY;
            int endDelay = h is Spinner ? 1 : 0;
            var endFrame = new OsuKeyUpReplayFrame(hEndTime + endDelay, new Vector2(h.StackedEndPosition.X, h.StackedEndPosition.Y));

            // Decrement because we want the previous frame, not the next one
            int index = FindInsertionIndex(startFrame) - 1;

            // If the previous frame has a button pressed, force alternation.
            // If there are frames ahead, modify those to use the new button press.
            // Do we have a previous frame? No need to check for < replay.Count since we decremented!
            if (index >= 0)
            {
                var previousFrame = (OsuReplayFrame)Frames[index];
                var previousActions = previousFrame.Actions;

                // If a button is already held, then we simply alternate
                if (previousActions.Any())
                {
                    // Force alternation if we have the same button. Otherwise we can just keep the naturally to us assigned button.
                    if (previousActions.Contains(action))
                    {
                        action = action == OsuAction.LeftButton ? OsuAction.RightButton : OsuAction.LeftButton;
                        startFrame.Actions.Clear();
                        startFrame.Actions.Add(action);
                    }

                    // We always follow the most recent slider / spinner, so remove any other frames that occur while it exists.
                    int endIndex = FindInsertionIndex(endFrame);

                    if (index < Frames.Count - 1)
                        Frames.RemoveRange(index + 1, Math.Max(0, endIndex - (index + 1)));

                    // After alternating we need to keep holding the other button in the future rather than the previous one.
                    for (int j = index + 1; j < Frames.Count; ++j)
                    {
                        var frame = (OsuReplayFrame)Frames[j];

                        // Don't affect frames which stop pressing a button!
                        if (j < Frames.Count - 1 || frame.Actions.SequenceEqual(previousActions))
                        {
                            frame.Actions.Clear();
                            frame.Actions.Add(action);
                        }
                    }
                }
            }

            AddFrameToReplay(startFrame);

            // 0.05 rad/ms, or ~477 RPM, as per stable.
            // the redundant conversion from RPM to rad/ms is here for ease of testing custom SPM specs.
            const float spin_rpm = 0.05f / (2 * MathF.PI) * 60000;
            float radsPerMillisecond = MathUtils.DegreesToRadians(spin_rpm * 360) / 60000;

            switch (h)
            {
                // We add intermediate frames for spinning / following a slider here.
                case Spinner spinner:
                    Vector2 difference = startPosition - SPINNER_CENTRE;

                    float radius = difference.Length;
                    float angle = radius == 0 ? 0 : MathF.Atan2(difference.Y, difference.X);

                    double t;
                    double previousFrame = h.StartTime;

                    for (double nextFrame = h.StartTime + GetFrameDelay(h.StartTime); nextFrame < spinner.EndTime; nextFrame += GetFrameDelay(nextFrame))
                    {
                        t = ApplyModsToTimeDelta(previousFrame, nextFrame) * spinnerDirection;
                        angle += (float)t * radsPerMillisecond;

                        Vector2 pos = SPINNER_CENTRE + CirclePosition(angle, SPIN_RADIUS);
                        AddFrameToReplay(new OsuReplayFrame((int)nextFrame, new Vector2(pos.X, pos.Y), action));

                        previousFrame = nextFrame;
                    }

                    t = ApplyModsToTimeDelta(previousFrame, spinner.EndTime) * spinnerDirection;
                    angle += (float)t * radsPerMillisecond;

                    Vector2 endPosition = SPINNER_CENTRE + CirclePosition(angle, SPIN_RADIUS);

                    AddFrameToReplay(new OsuReplayFrame(spinner.EndTime, new Vector2(endPosition.X, endPosition.Y), action));

                    endFrame.Position = endPosition;
                    break;

                case Slider slider:
                    for (double j = GetFrameDelay(slider.StartTime); j < slider.Duration; j += GetFrameDelay(slider.StartTime + j))
                    {
                        Vector2 pos = slider.StackedPositionAt(j / slider.Duration);
                        AddFrameToReplay(new OsuReplayFrame(h.StartTime + j, new Vector2(pos.X, pos.Y), action));
                    }

                    AddFrameToReplay(new OsuReplayFrame(slider.EndTime, new Vector2(slider.StackedEndPosition.X, slider.StackedEndPosition.Y), action));
                    break;
            }

            // We only want to let go of our button if we are at the end of the current replay. Otherwise something is still going on after us so we need to keep the button pressed!
            if (Frames[^1].Time <= endFrame.Time)
                AddFrameToReplay(endFrame);
        }

        #endregion

        private class OsuKeyUpReplayFrame : OsuReplayFrame
        {
            public OsuKeyUpReplayFrame(double time, Vector2 position)
                : base(time, position)
            {
            }
        }
    }
}

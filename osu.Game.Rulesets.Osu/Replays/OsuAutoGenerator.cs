// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using System;
using System.Linq;
using System.Diagnostics;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Replays;
using System.Collections.Generic;

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

        /// <summary>
        /// The amount of padding added between hitobjects overlapping spinners
        /// before Auto will try to spin the spinner
        /// </summary>
        public const double SPIN_BUFFER_TIME = 300; // Won't spin between 100bpm 1/1 beat patterns or faster

        /// <summary>
        /// Auto will try to click reactionTime ms after hit object appears,
        /// unless it's less than MIN_MOVE_TIME ms before hitpoint
        /// </summary>
        public const double MIN_MOVE_TIME = 50;

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

        public override Replay Generate()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            Frames.Clear();

            // Summary of replay generation:
            // It is split into 6 steps
            //  1) The 1st step collects information about all the hitobjects
            //     and lays out key frames (clicking, moving, releasing)
            //     in strictly chronological order in the importantFrames list.
            //  2) The 2nd step filters down the hitpoints to only the ones that will be clicked (activeHitpoints)
            //  3) The 3rd step creates a plan for which buttons should be used for which hitpoints
            //  4) The 4th step generates the actual button states from the plan
            //  5) The 5th step generates all cursor positions
            //  6) The 6th step combines cursor locations with button states to produce the actual replayframes

            collectKeyInfo(out IntervalSet spinZones, out IntervalSet spinnerVisibleZones, out SortedDictionary<double, KeyFrame> keyFrames);
            filterHitpoints(out SortedDictionary<double, Hitpoint> activeHitpoints,
                            keyFrames);
            planButtons(out SortedDictionary<double, ButtonPlan> buttonsPlan,
                        keyFrames);
            generateButtons(out SortedDictionary<double, IEnumerable<OsuAction>> buttons,
                            buttonsPlan);
            generatePositions(out SortedDictionary<double, Vector2> positions, activeHitpoints, spinZones, spinnerVisibleZones);

            // Combine to form actual replay
            AddFrameToReplay(new OsuReplayFrame(-100000, new Vector2(256, 500), Array.Empty<OsuAction>()));
            AddFrameToReplay(new OsuReplayFrame(Beatmap.HitObjects[0].StartTime - 1500, new Vector2(256, 500), Array.Empty<OsuAction>()));
            AddFrameToReplay(new OsuReplayFrame(Beatmap.HitObjects[0].StartTime - 1000, new Vector2(256, 192), Array.Empty<OsuAction>()));

            generateReplayFrames(buttons, positions);

            sw.Stop();
            Logger.Log("Replay took " + sw.ElapsedMilliseconds + "ms to generate.", LoggingTarget.Performance);

            return Replay;
        }

        #region Generation steps

        /// <summary>
        /// Extracts the most important info from the Beatmap first, which will be further processed to generate the replay.
        /// </summary>
        /// <param name="spinZones">The intervals where spinning will be required.</param>
        /// <param name="spinnerVisibleZones">The intervals where at least 1 spinner is visible.</param>
        /// <param name="keyFrames">A list of <see cref="KeyFrame"/> in the Beatmap, storing important timestamps where clicks/holds/etc. occur.</param>
        private void collectKeyInfo(out IntervalSet spinZones, out IntervalSet spinnerVisibleZones, out SortedDictionary<double, KeyFrame> keyFrames)
        {
            IntervalSet holdZones = new IntervalSet();
            spinZones             = new IntervalSet();
            spinnerVisibleZones   = new IntervalSet();
            keyFrames             = new SortedDictionary<double, KeyFrame>();

            foreach (OsuHitObject obj in Beatmap.HitObjects)
            {
                // Circles are also "holds" for KEY_UP_DELAY amount of time
                // so we just add holdZones regardless of object type
                {
                    Interval interval = holdZones.AddInterval(
                        obj.StartTime,
                        ((obj as IHasEndTime)?.EndTime ?? obj.StartTime) + KEY_UP_DELAY
                    );

                    // Create frames for the new hold
                    addKeyFrame(keyFrames, interval.Start);
                    addKeyFrame(keyFrames, interval.End);
                }

                // Now we add hitpoints of interest (clicks and follows or spins)
                if (obj is HitCircle)
                {
                    addHitpoint(keyFrames, obj, obj.StartTime, true, false);
                }
                else if (obj is Slider)
                {
                    Slider slider = obj as Slider;

                    // Slider head
                    addHitpoint(keyFrames, slider, slider.StartTime, true, false);

                    // Slider ticks and repeats
                    foreach (var n in slider.NestedHitObjects)
                    {
                        if (n is SliderTick || n is RepeatPoint)
                        {
                            addHitpoint(keyFrames, slider, n.StartTime, false, true);
                        }
                    }

                    // Slider tail
                    addHitpoint(keyFrames, slider, slider.EndTime, false, true);
                }
                else if (obj is Spinner)
                {
                    Spinner spinner = (Spinner)obj;

                    Interval interval = spinZones.AddInterval(spinner.StartTime, spinner.EndTime);
                    spinnerVisibleZones.AddInterval(spinner.StartTime - spinner.TimePreempt, spinner.EndTime);

                    // Create frames for the new spin
                    addKeyFrame(keyFrames, interval.Start);
                    addKeyFrame(keyFrames, interval.End);
                }
            }

            // Set Hold and Spin IntervalStates
            var keyFrameIter = keyFrames.GetEnumerator();
            keyFrameIter.MoveNext();
            foreach (var hold in holdZones)
            {
                while (keyFrameIter.Current.Key < hold.Start)
                {
                    keyFrameIter.MoveNext();
                }
                keyFrameIter.Current.Value.Hold = IntervalState.Start;
                keyFrameIter.MoveNext();
                while (keyFrameIter.Current.Key < hold.End)
                {
                    keyFrameIter.Current.Value.Hold = IntervalState.Mid;
                    keyFrameIter.MoveNext();
                }
                keyFrameIter.Current.Value.Hold = IntervalState.End;
                keyFrameIter.MoveNext();
            }
            keyFrameIter.Dispose();
            keyFrameIter = keyFrames.GetEnumerator();
            keyFrameIter.MoveNext();
            foreach (var spin in spinZones)
            {
                while (keyFrameIter.Current.Key < spin.Start)
                {
                    keyFrameIter.MoveNext();
                }
                keyFrameIter.Current.Value.Spin = IntervalState.Start;
                keyFrameIter.MoveNext();
                while (keyFrameIter.Current.Key < spin.End)
                {
                    keyFrameIter.Current.Value.Spin = IntervalState.Mid;
                    keyFrameIter.MoveNext();
                }
                keyFrameIter.Current.Value.Spin = IntervalState.End;
                keyFrameIter.MoveNext();
            }
            keyFrameIter.Dispose();
        }

        /// <summary>
        /// Determines when and which buttons should be pressed using the key frames given.
        /// </summary>
        /// <param name="buttonsPlan">A list of <see cref="ButtonPlan"/> which is basically a slightly more informative <see cref="OsuAction"/>.</param>
        /// <param name="keyFrames">A list of <see cref="KeyFrame"/> in the Beatmap, storing hitpoints (important timestamps where clicks/holds/etc. occur).</param>
        private void planButtons(out SortedDictionary<double, ButtonPlan> buttonsPlan, SortedDictionary<double, KeyFrame> keyFrames)
        {
            buttonsPlan = new SortedDictionary<double, ButtonPlan> {
                [Beatmap.HitObjects[0].StartTime - 1000] = new ButtonPlan()
            };

            ButtonPlanner buttonManager = new ButtonPlanner();
            foreach (KeyFrame curr in keyFrames.Values)
            {
                if (curr.Hold == IntervalState.Mid && curr.HasClick)
                {
                    buttonsPlan[curr.Time] = buttonManager.Press(curr.Time);
                    buttonManager.Release(curr.Time);
                }
                else if (curr.Hold == IntervalState.End)
                {
                    buttonsPlan[curr.Time] = buttonManager.Release(curr.Time);
                }
                else if (curr.Hold == IntervalState.Start)
                {
                    buttonsPlan[curr.Time] = buttonManager.Press(curr.Time);
                }
            }
        }

        /// <summary>
        /// Generates the final button presses that will be used in the replay using the buttonsPlan.
        /// </summary>
        /// <param name="buttons">A list of <see cref="OsuAction"/> which will be the actual key presses used in the auto replay.</param>
        /// <param name="buttonsPlan">A list of <see cref="ButtonPlan"/> which is basically a slightly more informative <see cref="OsuAction"/>.</param>
        private void generateButtons(out SortedDictionary<double, IEnumerable<OsuAction>> buttons, SortedDictionary<double, ButtonPlan> buttonsPlan)
        {
            buttons = new SortedDictionary<double, IEnumerable<OsuAction>> {
                [Beatmap.HitObjects[0].StartTime - 1000] = Enumerable.Empty<OsuAction>()
            };

            var prev = new ButtonPlan();
            int i = 0;
            foreach (var ibutton in buttonsPlan)
            {
                if (prev.Primary == Button.None && ibutton.Value.Primary != Button.None)
                {
                    // Fresh click
                    buttons[ibutton.Key] = ibutton.Value.Rbs;
                }
                else if (prev.Primary != Button.None && ibutton.Value.Primary == Button.None)
                {
                    // Complete release
                    buttons[ibutton.Key] = Enumerable.Empty<OsuAction>();
                }
                else if (ibutton.Value.Primary != Button.None)
                {
                    // We want to create a new click but we're already holding (prev != Button.None)
                    // We want to extend the existing button KEY_UP_DELAY before releasing
                    // But if there's another key down event for that button, we must ensure we let go earlier
                    double keyUpTime = ibutton.Key + KEY_UP_DELAY;
                    foreach (var jbutton in buttonsPlan.Skip(i + 1))
                    {
                        if (jbutton.Key > keyUpTime)
                            break; // No clash
                        if (jbutton.Value.Primary == ibutton.Value.Secondary || jbutton.Value.Secondary == ibutton.Value.Secondary)
                        {
                            // Let go earlier, halfway between here and the clash
                            keyUpTime = (ibutton.Key + jbutton.Key) / 2;
                            break;
                        }
                    }
                    buttons[ibutton.Key] = new[] { OsuAction.LeftButton, OsuAction.RightButton };
                    buttons[keyUpTime] = ibutton.Value.PrimaryRbs;
                }
                i++;
                prev = ibutton.Value;
            }
        }

        /// <summary>
        /// Determines the actual hitpoints that should be used, as a single KeyFrame may contain multiple hitpoints on the same timestamp.
        /// The hitpoints will then be used to generate cursor movement.
        /// </summary>
        /// <param name="activeHitpoints">The finalised list of <see cref="Hitpoint"/> that will be used to generate positions.</param>
        /// <param name="keyFrames">A list of <see cref="KeyFrame"/>.</param>
        private void filterHitpoints(out SortedDictionary<double, Hitpoint> activeHitpoints,
            SortedDictionary<double, KeyFrame> keyFrames)
        {
            activeHitpoints = new SortedDictionary<double, Hitpoint>();
            foreach (var curr in keyFrames.Values)
            {
                // For now just make it click/move to the first object, prioritising clicks
                if (curr.HasClick && !curr.HasMove)
                    activeHitpoints[curr.Time] = curr.Clicks[0];
                else if (!curr.HasClick && curr.HasMove)
                    activeHitpoints[curr.Time] = curr.Moves[0];
                else if (curr.HasClick && curr.HasMove)
                    activeHitpoints[curr.Time] = curr.Clicks[0];
                else
                {
                    // Nothing to do if there is already nothing to click or move to
                }
            }
        }

        /// <summary>
        /// Generates cursor positions that aim at all the hitpoints given.
        /// Also spins the cursor if we should be spinning and there is sufficient time between any two hitpoints.
        /// </summary>
        /// <param name="positions">The cursor positions.</param>
        /// <param name="activeHitpoints">The list of <see cref="Hitpoint"/>.</param>
        /// <param name="spinZones">The intervals where we should spin the cursor.</param>
        /// <param name="spinnerVisibleZones">The intervals where at least one spinner is visible.</param>
        private void generatePositions(out SortedDictionary<double, Vector2> positions, SortedDictionary<double, Hitpoint> activeHitpoints, IntervalSet spinZones, IntervalSet spinnerVisibleZones)
        {
            positions = new SortedDictionary<double, Vector2> {
                [Beatmap.HitObjects[0].StartTime - 1000] = new Vector2(256, 192)
            };

            // First we "dot in" all the positions *at* hitpoints, before generating positions between hitpoints.
            foreach (Hitpoint curr in activeHitpoints.Values)
                positions[curr.Time] = curr.Position;

            Hitpoint left = activeHitpoints.First().Value;
            foreach (Hitpoint right in activeHitpoints.Values.Skip(1))
            {
                IntervalSet spins = spinZones.Intersect(left.Time + SPIN_BUFFER_TIME, right.Time - SPIN_BUFFER_TIME);

                if (spins.Count == 0)
                {
                    // No spins, move directly between hitpoints
                    if (right.HitObject is Slider && left.HitObject == right.HitObject)
                    {
                        // Follow the slider
                        Slider s = (Slider)right.HitObject;
                        addFollowSliderPositions(positions, left.Time, right.Time, s);
                    }
                    else
                    {
                        // In every other case, just interpolate with preferredEasing

                        // Time when we should react to the right hitpoint
                        double reactionStartTime = right.HitObject.StartTime - Math.Max(0, right.HitObject.TimePreempt - reactionTime);
                        double startTime;
                        if (left.Time >= right.Time - MIN_MOVE_TIME) {
                            startTime = left.Time;
                        } else if (left.Time + KEY_UP_DELAY >= right.Time - MIN_MOVE_TIME) {
                            startTime = right.Time - MIN_MOVE_TIME;
                        } else if (left.Time + KEY_UP_DELAY > reactionStartTime) {
                            // If possible, hover on the previous object for KEY_UP_DELAY ms
                            startTime = left.Time + KEY_UP_DELAY;
                        } else {
                            startTime = reactionStartTime;
                        }

                        addMovePositions(positions, startTime, right.Time, left.Position, right.Position);
                    }
                }
                else
                {
                    // There are spins to handle

                    // First calculate the spins
                    // Keep track of initial spin pos and end spin pos
                    Vector2 curpos = left.Position;
                    Vector2 firstSpinPos = CalcSpinnerStartPos(left.Position);
                    double startSpinTime = spins[0].Start;
                    double endSpinTime = spins[spins.Count - 1].End;

                    foreach (var spin in spins)
                        curpos = addSpinPositions(positions, curpos, spin);

                    // Travel from left to spin
                    double spinnerVisible = spinnerVisibleZones.GetIntervalContaining(startSpinTime).Start;
                    double leftStartTime = Math.Max(left.Time, Math.Min(startSpinTime - MIN_MOVE_TIME,
                            spinnerVisible + reactionTime));
                    addMovePositions(positions, leftStartTime, startSpinTime, left.Position, firstSpinPos);

                    // Travel from spin to right
                    double rightStartTime = Math.Max(endSpinTime, Math.Min(right.Time - MIN_MOVE_TIME,
                        right.HitObject.StartTime - Math.Max(0, right.HitObject.TimePreempt - reactionTime)));
                    addMovePositions(positions, rightStartTime, right.Time, curpos, right.Position);
                }

                left = right;
            }

            // Handle spins at the beginning or end of the replay
            // This is needed because these spins don't occur between any activeHitpoints
            Hitpoint firstHitpoint = activeHitpoints.First().Value;
            Hitpoint lastHitpoint = activeHitpoints.Last().Value;
            IntervalSet startSpins = spinZones.Intersect(double.NegativeInfinity, firstHitpoint.Time - SPIN_BUFFER_TIME);
            IntervalSet endSpins = spinZones.Intersect(lastHitpoint.Time + SPIN_BUFFER_TIME, double.PositiveInfinity);
            if (startSpins.Count > 0)
            {
                Vector2 firstSpinPos = new Vector2(); // will be overwritten, but C# complains about possible null
                foreach (Interval spin in startSpins)
                {
                    firstSpinPos = addSpinPositions(positions, SPINNER_CENTRE + new Vector2(0, -SPIN_RADIUS), spin);
                }

                // Travel from spin to first hitpoint
                double startTime = Math.Max(startSpins.Last().End, Math.Min(firstHitpoint.Time - MIN_MOVE_TIME,
                        firstHitpoint.HitObject.StartTime - Math.Max(0, firstHitpoint.HitObject.TimePreempt - reactionTime)));
                addMovePositions(positions, startTime, firstHitpoint.Time, firstSpinPos, firstHitpoint.Position);
            }
            if (endSpins.Count > 0)
            {
                Vector2 curpos = lastHitpoint.Position;
                Vector2 endSpinPos = CalcSpinnerStartPos(curpos);
                foreach (Interval spin in endSpins)
                {
                    addSpinPositions(positions, curpos, spin);
                }

                // Travel from last hitpoint to spin
                double spinnerVisible = spinnerVisibleZones.GetIntervalContaining(endSpins[0].Start).Start;
                double startTime = Math.Max(lastHitpoint.Time, Math.Min(endSpins[0].Start - MIN_MOVE_TIME,
                        spinnerVisible + reactionTime));
                addMovePositions(positions, startTime, endSpins[0].Start, lastHitpoint.Position, endSpinPos);
            }
        }

        /// <summary>
        /// Combines the button presses with cursor positions to generate the final <see cref="ReplayFrame"/>s.
        /// </summary>
        /// <param name="buttons">The button presses.</param>
        /// <param name="positions">The cursor positions.</param>
        private void generateReplayFrames(SortedDictionary<double, IEnumerable<OsuAction>> buttons, SortedDictionary<double, Vector2> positions)
        {
            // Loop through each position, and advance buttons accordingly
            int buttonIndex = 0;
            var button = buttons.First();
            var buttonIter = buttons.GetEnumerator();
            buttonIter.MoveNext(); buttonIter.MoveNext();
            var nextbutton = buttonIter.Current;
            foreach (var pos in positions)
            {
                if (buttonIndex == 0 && button.Key > pos.Key) // Special case where pos occurs before any button keys
                {
                    AddFrameToReplay(new OsuReplayFrame(pos.Key, new Vector2(pos.Value.X, pos.Value.Y), Array.Empty<OsuAction>()));
                    continue;
                }

                while (buttonIndex != buttons.Count - 1 && Precision.DefinitelyBigger(pos.Key, nextbutton.Key))
                {
                    // Insert a frame at nextbutton.Key as that might've not have a positions entry (i.e. button releases)
                    AddFrameToReplay(new OsuReplayFrame(nextbutton.Key, new Vector2(pos.Value.X, pos.Value.Y), nextbutton.Value.ToArray()));
                    buttonIndex++;
                    button = nextbutton;
                    buttonIter.MoveNext();
                    nextbutton = buttonIter.Current;
                }

                while (buttonIndex != buttons.Count - 1 && Precision.AlmostBigger(pos.Key, nextbutton.Key))
                {
                    // Advance past pos so button.Key is at or before pos.Key
                    buttonIndex++;
                    button = nextbutton;
                    buttonIter.MoveNext();
                    nextbutton = buttonIter.Current;
                }

                AddFrameToReplay(new OsuReplayFrame(pos.Key, new Vector2(pos.Value.X, pos.Value.Y), button.Value.ToArray()));
            }
            buttonIter.Dispose();
        }

        #endregion

        #region positions Helpers

        /// <summary>
        /// Create cursor positions that spin in the time interval <paramref name="spin"/>, given that the cursor is currently at <paramref name="curpos"/>.
        /// </summary>
        /// <param name="positions">The list of cursor positions to generate into.</param>
        /// <param name="curpos">The current cursor position just before spinning.</param>
        /// <param name="spin">The time interval where we should spin the cursor.</param>
        /// <returns>The final cursor position after spinning.</returns>
        private Vector2 addSpinPositions(SortedDictionary<double, Vector2> positions, Vector2 curpos, Interval spin)
        {
            Vector2 startPosition = CalcSpinnerStartPos(curpos);

            Vector2 difference = startPosition - SPINNER_CENTRE;

            float radius = difference.Length;
            float angle = radius == 0 ? 0 : (float)Math.Atan2(difference.Y, difference.X);

            double t;

            for (double j = spin.Start; j < spin.End; j += FrameDelay)
            {
                t = ApplyModsToTime(j - spin.Start);

                Vector2 pos = SPINNER_CENTRE + CirclePosition(t / 20 + angle, SPIN_RADIUS);
                positions[j] = pos;
            }

            t = ApplyModsToTime(spin.End - spin.Start);
            Vector2 endPosition = SPINNER_CENTRE + CirclePosition(t / 20 + angle, SPIN_RADIUS);
            positions[spin.End] = endPosition;

            return endPosition;
        }

        /// <summary>
        /// Create cursor positions that follow a certain slider, between the timestamps <paramref name="startTime"/> and <paramref name="endTime"/>.
        /// We don't just follow the whole slider because in some 2B maps, we want to only follow part of a slider (between two slider ticks for example).
        /// And so <paramref name="startTime"/> and <paramref name="endTime"/> are used to specify this interval.
        /// </summary>
        /// <param name="positions">The list of cursor positions to generate into.</param>
        /// <param name="startTime">The timestamp where we start following the slider.</param>
        /// <param name="endTime">The timestamp where we stop following the slider.</param>
        /// <param name="s">The <see cref="Slider"/> which we are following.</param>
        private void addFollowSliderPositions(SortedDictionary<double, Vector2> positions, double startTime, double endTime, Slider s)
        {
            for (double t = startTime + FrameDelay; t < endTime; t += FrameDelay)
            {
                positions[t] = s.StackedPositionAt((t - s.StartTime) / s.Duration);
            }
        }

        /// <summary>
        /// Create cursor positions that move between two positions, interpolating between them using <see cref="preferredEasing"/>.
        /// </summary>
        /// <param name="positions">The list of cursor positions to generate into.</param>
        /// <param name="startTime">The timestamp where we start moving.</param>
        /// <param name="endTime">The timestamp where we stop moving.</param>
        /// <param name="startPosition">The initial position our cursor is at.</param>
        /// <param name="endPosition">The position our cursor should move to.</param>
        private void addMovePositions(SortedDictionary<double, Vector2> positions, double startTime, double endTime, Vector2 startPosition, Vector2 endPosition)
        {
            if (!positions.ContainsKey(startTime))
                positions[startTime] = startPosition;
            for (double t = startTime + FrameDelay; t < endTime; t += FrameDelay)
            {
                positions[t] = Interpolation.ValueAt(
                    t, startPosition, endPosition,
                    startTime, endTime, preferredEasing
                );
            }
        }

        #endregion

        #region keyframe/hitpoint/zones Helpers

        /// <summary>
        /// Create a new <see cref="KeyFrame"/> in the <paramref name="keyFrames"/> list at the specified timestamp <paramref name="time"/>.
        /// </summary>
        /// <param name="keyFrames">The key frame list.</param>
        /// <param name="time">The timestamp to generate the key frame at.</param>
        private void addKeyFrame(SortedDictionary<double, KeyFrame> keyFrames, double time)
        {
            if (!keyFrames.ContainsKey(time))
            {
                keyFrames[time] = new KeyFrame(time);
            }
        }

        /// <summary>
        /// Adds a hitpoint to the key frame list, creating a new key frame if necessary.
        /// </summary>
        /// <param name="keyFrames">The key frame list.</param>
        /// <param name="obj">The <see cref="OsuHitObject"/> of this hitpoint.</param>
        /// <param name="time">The timestamp of this hitpoint.</param>
        /// <param name="click">Whether this hitpoint should be clicked or not (a circle or slider head).</param>
        /// <param name="move">Whether the cursor should move to this hitpoint or not (a circle or slider, not a spinner).</param>
        private void addHitpoint(SortedDictionary<double, KeyFrame> keyFrames, OsuHitObject obj, double time, bool click, bool move)
        {
            Hitpoint newhitpoint = new Hitpoint
            {
                Time = time,
                HitObject = obj
            };

            // Add click to keyFrames
            if (click)
            {
                addKeyFrame(keyFrames, time);

                keyFrames[time].Clicks.Add(newhitpoint);
            }

            // Add move to keyFrames
            if (move)
            {
                addKeyFrame(keyFrames, time);

                keyFrames[time].Moves.Add(newhitpoint);
            }
        }

        #endregion

        #endregion

        #region Helper classes and subroutines

        /// <summary>
        /// Basically a timestamp and a position, used to generate positions.
        /// Keeps a reference to the corresponding <see cref="OsuHitObject"/> in so we can see whether we should follow sliders or not afterwards.
        /// </summary>
        private class Hitpoint
        {
            public double Time;

            // The circle/slider/spinner associated with this hitpoint
            public OsuHitObject HitObject;

            public Vector2 Position
            {
                get
                {
                    Slider s = HitObject as Slider;
                    if (s != null)
                    {
                        double progress = (Time - s.StartTime) / s.Duration;
                        return s.StackedPositionAt(progress) + s.StackOffset;
                    }
                    else
                    {
                        return HitObject.StackedPosition;
                    }
                }
            }
        }

        private enum IntervalState
        {
            None,
            Start,
            Mid,
            End
        }

        /// <summary>
        /// Aggregates all the hitpoints/zones at a certain time into one data object.
        /// </summary>
        private class KeyFrame
        {
            // The timestamp where all this is happening
            public readonly double Time;

            // Whether we're at the start of a holdZone, middle of one, or at the end of one.
            public IntervalState Hold = IntervalState.None;
            public bool WasHolding => Hold == IntervalState.Mid || Hold == IntervalState.End;
            public bool Holding => Hold == IntervalState.Start || Hold == IntervalState.Mid;

            // Ditto for spins
            public IntervalState Spin = IntervalState.None;
            public bool WasSpinning => Spin == IntervalState.Mid || Spin == IntervalState.End;
            public bool Spinning => Spin == IntervalState.Start || Hold == IntervalState.Mid;

            // List of hitpoints we want our cursor to be near to
            public readonly List<Hitpoint> Moves = new List<Hitpoint>();
            public bool HasMove => Moves.Count > 0;

            // List of hitpoints we need to click
            public readonly List<Hitpoint> Clicks = new List<Hitpoint>();
            public bool HasClick => Clicks.Count > 0;

            public KeyFrame(double time)
            {
                Time = time;
            }
        }

        [Flags]
        private enum Button
        {
            None = 0,
            Left = 1,
            Right = 2,
        }

        /// <summary>
        /// A slightly more informative version of <see cref="OsuAction"/>.
        /// Keeps track of which buttons are the primary or secondary buttons, to more easily determine how we should alternate buttons.
        /// </summary>
        private class ButtonPlan
        {
            public Button Primary;
            public Button Secondary;

            private static IEnumerable<OsuAction> toRbs(Button button)
            {
                switch (button)
                {
                    default:
                    case Button.None:
                        break;
                    case Button.Left:
                        yield return OsuAction.LeftButton;
                        break;
                    case Button.Right:
                        yield return OsuAction.RightButton;
                        break;
                }
            }

            public IEnumerable<OsuAction> Rbs => toRbs(Primary).Concat(toRbs(Secondary));
            public IEnumerable<OsuAction> PrimaryRbs => toRbs(Primary);
        }

        // Handles alternating buttons and 2B style playing
        private class ButtonPlanner
        {
            private ButtonPlan curr = new ButtonPlan();

            // Parameters
            //private const bool cycle_when_both_held = false;
            private const double alternate_threshold = 150; // 150ms is threshold for 120bpm streams

            // Extra metadata to manage state changes (when to alternate after Press, etc)
            private double lastUsedLeft  = double.NegativeInfinity;
            private double lastUsedRight = double.NegativeInfinity;

            private int numHeld; // Buttons currently held

            private void setLastUsed(Button b, double time)
            {
                if (b.HasFlag(Button.Left))
                    lastUsedLeft = Math.Max(lastUsedLeft, time);
                else
                    lastUsedRight = Math.Max(lastUsedRight, time);
            }

            public ButtonPlan Press(double time)
            {
                if (numHeld == 0)
                {
                    // Decide whether to alternate or not
                    if (time - lastUsedLeft + KEY_UP_DELAY > alternate_threshold)
                    {
                        // The time since last used is big enough so we singletap
                        curr = new ButtonPlan{Primary = Button.Left};
                    }
                    else if (lastUsedLeft < lastUsedRight)
                    {
                        // We're alternating, use the less recently used button
                        curr = new ButtonPlan{Primary = Button.Left};
                    }
                    else
                    {
                        curr = new ButtonPlan{Primary = Button.Right};
                    }
                    setLastUsed(curr.Primary, time);
                }
                else if (numHeld == 1)
                {
                    // Uncomment these if you want to use this option,
                    // inspectcode doesn't like either public fields that are never accessed or unreachable code.
                    // if (cycle_when_both_held) {
                    //     curr = new ButtonPlan{
                    //         Primary   = curr.Primary ^ (Button.Left | Button.Right),
                    //         Secondary = curr.Primary
                    //     };
                    //     setLastUsed(curr.Primary, time);
                    // }
                    // else
                    // {
                    curr = new ButtonPlan{
                        Primary   = curr.Primary,
                        Secondary = curr.Primary ^ (Button.Left | Button.Right)
                    };
                    setLastUsed(curr.Secondary, time);
                    // }
                }
                else
                {
                    // what
                    numHeld--;
                    throw new InvalidOperationException("Trying to click when both buttons are already pressed is likely a mistake. (at " + time + ")");
                }
                numHeld++;
                return curr;
            }

            public ButtonPlan Release(double time)
            {
                if (numHeld == 1)
                {
                    setLastUsed(curr.Primary, time);
                    curr = new ButtonPlan();
                }
                else if (numHeld == 2)
                {
                    setLastUsed(curr.Secondary, time);
                    curr = new ButtonPlan{
                        Primary = curr.Primary
                    };
                }
                else
                {
                    // do nothing
                    numHeld++;
                }
                numHeld--;
                return curr;
            }
        }

        private class Interval : IComparable<Interval>
        {
            public double Start;
            public double End;

            public Interval() {}

            public Interval(double start, double end)
            {
                Start = start;
                End = end;
            }

            public bool Contains(double value)
            {
                return value >= Start && value <= End;
            }

            public int CompareTo(Interval i)
            {
                if (End < i.Start)
                    return -1;
                if (Start > i.End)
                    return 1;
                return 0; // Overlap
            }

            public double Clamp(double value)
            {
                return Math.Max(Start, Math.Min(End, value));
            }
        }

        /// <summary>
        /// Stores an interal, usually an interval between two timestamps.
        /// </summary>
        private class IntervalSet : List<Interval>
        {

            /// <summary>
            /// Add a new interval to the interval set, merging intervals if they overlap.
            /// Returns the interval that was ultimately added (after merging)
            /// </summary>
            public Interval AddInterval(double start, double end) {
                if (end < start)
                    return null;

                // Smallest and largest overlapping intervals
                int lowest = FindIndex(s => s.End >= start);
                int highest = FindLastIndex(s => s.Start <= end);

                // This means that the interval being inserted is larger than all existing intervals.
                if (lowest == -1) {
                    lowest = Count;
                }

                // The case where the interval is smaller than everything is "automatic"
                //if (highest == -1) {
                //    highest = -1;
                //}

                if (lowest == highest + 1)
                {
                    Interval interval = new Interval
                    {
                        Start = start,
                        End = end
                    };
                    // There are no intervals to merge
                    Insert(lowest, interval);
                    return interval;
                }
                else
                {
                    // Create a new interval that merges the overlapping intervals
                    Interval interval = new Interval
                    {
                        Start = Math.Min(start, this[lowest].Start),
                        End   = Math.Max(end, this[highest].End)
                    };

                    RemoveRange(lowest, highest-lowest+1);
                    Insert(lowest, interval);
                    return interval;
                }
            }

            public Interval AddInterval(Interval interval)
            {
                return AddInterval(interval.Start, interval.End);
            }

            public void RemoveInterval(double start, double end)
            {
                // Smallest and largest overlapping intervals
                int lowest = BinarySearch(new Interval(start, start));
                int highest = BinarySearch(new Interval(end, end));

                // Special case where both lowest and highest are on the same interval
                if (lowest >= 0 && lowest == highest)
                {
                    double origend = this[lowest].End;
                    this[lowest].End = start;
                    AddInterval(end, origend);
                    return;
                }

                // Trim the edge overlapping intervals
                // also set lowest and highest to the boundaries of all the intervals fully contained in (start, end)
                if (lowest >= 0)
                    this[lowest++].End = start;
                else
                    lowest = ~lowest;
                if (highest >= 0)
                    this[highest].Start = end;
                else
                    highest = ~highest;

                // Remove all the intervals that were fully contained
                RemoveRange(lowest, highest - lowest);
            }

            public void RemoveInterval(Interval interval)
            {
                RemoveInterval(interval.Start, interval.End);
            }

            public bool Contains(double value)
            {
                return BinarySearch(new Interval(value, value)) >= 0;
            }

            public Interval GetIntervalContaining(double value)
            {
                int index = BinarySearch(new Interval(value, value));
                if (index >= 0)
                    return this[index];
                else
                    return null;
            }

            public IntervalSet Intersect(double start, double end)
            {
                if (end < start)
                    return new IntervalSet();

                int startindex = BinarySearch(new Interval(start, start));
                if (startindex < 0)
                    startindex = ~startindex;

                IntervalSet result = new IntervalSet();
                for (int index = startindex; index < Count; index++)
                {
                    if (this[index].Start > end)
                    {
                        break;
                    }
                    result.AddInterval(Math.Max(start, this[index].Start), Math.Min(end, this[index].End));
                }
                return result;
            }

            public IntervalSet Intersect(Interval interval)
            {
                return Intersect(interval.Start, interval.End);
            }
        }

        #endregion
    }
}

// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Replays;
using osu.Game.Users;

namespace osu.Game.Rulesets.Osu.Replays
{
    public abstract class OsuAutoGeneratorBase : AutoGenerator<OsuHitObject>
    {
        #region Constants

        /// <summary>
        /// Constants (for spinners).
        /// </summary>
        protected static readonly Vector2 SPINNER_CENTRE = new Vector2(256, 192);
        protected const float SPIN_RADIUS = 50;

        /// <summary>
        /// The time in ms between each ReplayFrame.
        /// </summary>
        protected readonly double FrameDelay;

        #endregion

        #region Construction / Initialisation

        protected Replay Replay;
        protected List<ReplayFrame> Frames => Replay.Frames;

        protected OsuAutoGeneratorBase(Beatmap<OsuHitObject> beatmap)
            : base(beatmap)
        {
            Replay = new Replay
            {
                User = new User
                {
                    Username = @"Autoplay",
                }
            };

            // We are using ApplyModsToRate and not ApplyModsToTime to counteract the speed up / slow down from HalfTime / DoubleTime so that we remain at a constant framerate of 60 fps.
            FrameDelay = ApplyModsToRate(1000.0 / 60.0);
        }

        #endregion

        #region Utilities
        protected double ApplyModsToTime(double v) => v;
        protected double ApplyModsToRate(double v) => v;

        private class ReplayFrameComparer : IComparer<ReplayFrame>
        {
            public int Compare(ReplayFrame f1, ReplayFrame f2)
            {
                if (f1 == null) throw new ArgumentNullException(nameof(f1));
                if (f2 == null) throw new ArgumentNullException(nameof(f2));

                return f1.Time.CompareTo(f2.Time);
            }
        }

        private static readonly IComparer<ReplayFrame> replay_frame_comparer = new ReplayFrameComparer();

        protected int FindInsertionIndex(ReplayFrame frame)
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

        protected void AddFrameToReplay(ReplayFrame frame) => Frames.Insert(FindInsertionIndex(frame), frame);

        protected static Vector2 CirclePosition(double t, double radius) => new Vector2((float)(Math.Cos(t) * radius), (float)(Math.Sin(t) * radius));

        protected static Vector2 CalcSpinnerStartPos(Vector2 prevPos)
        {
            Vector2 spinCentreOffset = SPINNER_CENTRE - prevPos;
            float distFromCentre = spinCentreOffset.Length;

            if (distFromCentre > SPIN_RADIUS)
            {
                // Previous cursor position was outside spin circle, set startPosition to the tangent point.

                // Angle between centre offset and tangent point offset.
                double a = Math.Asin(SPIN_RADIUS / distFromCentre);

                // Angle between centre to tangent and centre offset
                double b = Math.PI / 2 - a;

                // Rotate clockwise by b to get tangent point direction
                Vector2 tangentPointDirection;
                tangentPointDirection.X = spinCentreOffset.X * (float)Math.Cos(b) - spinCentreOffset.Y * (float)Math.Sin(b);
                tangentPointDirection.Y = spinCentreOffset.X * (float)Math.Sin(b) + spinCentreOffset.Y * (float)Math.Cos(b);

                // Normalise tangent point direction to get tangent point
                tangentPointDirection *= SPIN_RADIUS / tangentPointDirection.Length;

                return SPINNER_CENTRE - tangentPointDirection;
            }
            else if (spinCentreOffset.Length > 0)
            {
                // Previous cursor position was inside spin circle, set startPosition to the nearest point on spin circle.
                return SPINNER_CENTRE - spinCentreOffset * (SPIN_RADIUS / spinCentreOffset.Length);
            }
            else
            {
                // Degenerate case where cursor position is exactly at the centre of the spin circle.
                return SPINNER_CENTRE + new Vector2(0, -SPIN_RADIUS);
            }
        }

        #endregion
    }
}

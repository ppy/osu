﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
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

        #endregion
    }
}

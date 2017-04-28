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

namespace osu.Game.Rulesets.Osu.Replays
{
    public abstract class OsuAutoReplayBase : Replay
    {
        #region Constants

        /// <summary>
        /// Constants (for spinners).
        /// </summary>
        protected static readonly Vector2 spinner_centre = new Vector2(256, 192);
        protected const float spin_radius = 50;

        /// <summary>
        /// The beatmap we're making a replay out of.
        /// </summary>
        protected readonly Beatmap<OsuHitObject> beatmap;

        /// <summary>
        /// The time in ms between each ReplayFrame.
        /// </summary>
        protected double frameDelay;

        #endregion

        #region Construction

        public OsuAutoReplayBase(Beatmap<OsuHitObject> beatmap)
        {
            this.beatmap = beatmap;
            Initialise();
            CreateAutoReplay();
        }

        /// <summary>
        /// Initialise this instance. Called before CreateAutoReplay.
        /// </summary>
        protected virtual void Initialise()
        {
			User = new User
			{
				Username = @"Autoplay",
			};

            // We are using ApplyModsToRate and not ApplyModsToTime to counteract the speed up / slow down from HalfTime / DoubleTime so that we remain at a constant framerate of 60 fps.
            frameDelay = applyModsToRate(1000.0 / 60.0);
        }

        /// <summary>
        /// Creates the auto replay. Every OsuAutoReplayBase subclass should implement this!
        /// </summary>
        protected abstract void CreateAutoReplay();

        #endregion

        #region Utilities
        protected double applyModsToTime(double v) => v;
        protected double applyModsToRate(double v) => v;

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

        protected int findInsertionIndex(ReplayFrame frame)
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

        protected void addFrameToReplay(ReplayFrame frame) => Frames.Insert(findInsertionIndex(frame), frame);

        protected static Vector2 circlePosition(double t, double radius) => new Vector2((float)(Math.Cos(t) * radius), (float)(Math.Sin(t) * radius));

        #endregion
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Game.Beatmaps;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Osu.Replays
{
    public abstract class OsuAutoGeneratorBase : AutoGenerator
    {
        #region Constants

        /// <summary>
        /// Constants (for spinners).
        /// </summary>
        protected static readonly Vector2 SPINNER_CENTRE = OsuPlayfield.BASE_SIZE / 2;

        public const float SPIN_RADIUS = 50;

        #endregion

        #region Construction / Initialisation

        protected Replay Replay;
        protected List<ReplayFrame> Frames => Replay.Frames;
        private readonly IReadOnlyList<IApplicableToRate> timeAffectingMods;

        protected OsuAutoGeneratorBase(IBeatmap beatmap, IReadOnlyList<Mod> mods)
            : base(beatmap)
        {
            Replay = new Replay();

            timeAffectingMods = mods.OfType<IApplicableToRate>().ToList();
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Returns the real duration of time between <paramref name="startTime"/> and <paramref name="endTime"/>
        /// after applying rate-affecting mods.
        /// </summary>
        /// <remarks>
        /// This method should only be used when <paramref name="startTime"/> and <paramref name="endTime"/> are very close.
        /// That is because the track rate might be changing with time,
        /// and the method used here is a rough instantaneous approximation.
        /// </remarks>
        /// <param name="startTime">The start time of the time delta, in original track time.</param>
        /// <param name="endTime">The end time of the time delta, in original track time.</param>
        protected double ApplyModsToTimeDelta(double startTime, double endTime)
        {
            double delta = endTime - startTime;

            foreach (var mod in timeAffectingMods)
                delta /= mod.ApplyToRate(startTime);

            return delta;
        }

        protected double ApplyModsToRate(double time, double rate)
        {
            foreach (var mod in timeAffectingMods)
                rate = mod.ApplyToRate(time, rate);
            return rate;
        }

        /// <summary>
        /// Calculates the interval after which the next <see cref="ReplayFrame"/> should be generated,
        /// in milliseconds.
        /// </summary>
        /// <param name="time">The time of the previous frame.</param>
        protected double GetFrameDelay(double time)
            => ApplyModsToRate(time, 1000.0 / 60);

        private class ReplayFrameComparer : IComparer<ReplayFrame>
        {
            public int Compare(ReplayFrame? f1, ReplayFrame? f2)
            {
                ArgumentNullException.ThrowIfNull(f1);
                ArgumentNullException.ThrowIfNull(f2);

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

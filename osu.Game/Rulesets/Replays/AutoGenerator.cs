// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Replays
{
    public abstract class AutoGenerator
    {
        /// <summary>
        /// The default duration of a key press in milliseconds.
        /// </summary>
        public const double KEY_UP_DELAY = 50;

        /// <summary>
        /// The beatmap the autoplay is generated for.
        /// </summary>
        protected IBeatmap Beatmap { get; }

        protected AutoGenerator(IBeatmap beatmap)
        {
            Beatmap = beatmap;
        }

        /// <summary>
        /// Generate the replay of the autoplay.
        /// </summary>
        public abstract Replay Generate();

        protected virtual HitObject? GetNextObject(int currentIndex)
        {
            if (currentIndex >= Beatmap.HitObjects.Count - 1)
                return null;

            return Beatmap.HitObjects[currentIndex + 1];
        }
    }

    public abstract class AutoGenerator<TFrame> : AutoGenerator
        where TFrame : ReplayFrame
    {
        /// <summary>
        /// The replay frames of the autoplay.
        /// </summary>
        protected readonly List<TFrame> Frames = new List<TFrame>();

        protected TFrame? LastFrame => Frames.Count == 0 ? null : Frames[^1];

        protected AutoGenerator(IBeatmap beatmap)
            : base(beatmap)
        {
        }

        public sealed override Replay Generate()
        {
            Frames.Clear();
            GenerateFrames();

            return new Replay
            {
                Frames = Frames.OrderBy(frame => frame.Time).Cast<ReplayFrame>().ToList()
            };
        }

        /// <summary>
        /// Generate the replay frames of the autoplay and populate <see cref="Frames"/>.
        /// </summary>
        protected abstract void GenerateFrames();
    }
}

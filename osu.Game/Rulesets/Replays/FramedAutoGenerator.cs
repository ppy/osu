// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Replays;

namespace osu.Game.Rulesets.Replays
{
    public abstract class FramedAutoGenerator<TFrame> : AutoGenerator
        where TFrame : ReplayFrame
    {
        /// <summary>
        /// The replay frames of the autoplay.
        /// </summary>
        protected readonly List<TFrame> Frames = new List<TFrame>();

        protected TFrame? LastFrame => Frames.Count == 0 ? null : Frames[^1];

        protected FramedAutoGenerator(IBeatmap beatmap)
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

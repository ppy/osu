// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Replays
{
    /// <summary>
    /// The list of frames of a replay.
    /// </summary>
    public class Replay
    {
        /// <summary>
        /// Whether no more frames would be added to this replay.
        /// If false, gameplay would be paused to wait for further data, for instance.
        /// </summary>
        public virtual bool IsComplete => true;

        /// <summary>
        /// The list of frames of this replay, sorted by <see cref="ReplayFrame.Time"/>.
        /// </summary>
        /// <remarks>
        /// Consumers of this replay may assume this list doesn't change randomly.
        /// That is, this list shouldn't change when <see cref="IsComplete"/> is <c>true</c>.
        /// Even if this replay is not complete, new frames should be only added at the end of the list.
        /// </remarks>
        public virtual IReadOnlyList<ReplayFrame> Frames => frames;

        private readonly ReplayFrame[] frames;

        /// <summary>
        /// Construct an empty replay.
        /// </summary>
        public Replay()
        {
            frames = Array.Empty<ReplayFrame>();
        }

        /// <summary>
        /// Construct a replay from its frames.
        /// Frames are automatically sorted by its time.
        /// </summary>
        /// <param name="frames">The frames of the replay.</param>
        public Replay(IEnumerable<ReplayFrame> frames)
        {
            this.frames = frames.OrderBy(f => f.Time).ToArray();
        }
    }
}

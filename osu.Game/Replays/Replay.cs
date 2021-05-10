// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Replays
{
    /// <summary>
    /// The list of frames of a replay.
    /// Frames may be added at the end when <see cref="IsComplete"/> is <c>false</c>.
    /// </summary>
    public class Replay
    {
        /// <summary>
        /// Whether no more frames would be added to this replay.
        /// If false, gameplay would be paused to wait for further data, for instance.
        /// </summary>
        public virtual bool IsComplete => true;

        /// <summary>
        /// The list of frames of this replay.
        /// This list should be sorted based on <see cref="ReplayFrame.Time"/>.
        /// </summary>
        public List<ReplayFrame> Frames { get; }

        /// <summary>
        /// Construct an empty replay.
        /// </summary>
        public Replay()
        {
            Frames = new List<ReplayFrame>();
        }

        /// <summary>
        /// Construct a replay from its frames.
        /// Frames are automatically sorted by its time.
        /// </summary>
        /// <param name="frames">The frames of the replay.</param>
        public Replay(IEnumerable<ReplayFrame> frames)
        {
            Frames = frames.OrderBy(f => f.Time).ToList();
        }
    }
}

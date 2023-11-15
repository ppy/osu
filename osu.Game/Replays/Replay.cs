// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Replays;
using osu.Game.Utils;

namespace osu.Game.Replays
{
    public class Replay : IDeepCloneable<Replay>
    {
        /// <summary>
        /// Whether all frames for this replay have been received.
        /// If false, gameplay would be paused to wait for further data, for instance.
        /// </summary>
        public bool HasReceivedAllFrames = true;

        public List<ReplayFrame> Frames = new List<ReplayFrame>();

        Replay IDeepCloneable<Replay>.DeepClone(IDictionary<object, object> referenceLookup)
        {
            if (referenceLookup.TryGetValue(this, out object? existing))
                return (Replay)existing;

            var clone = new Replay
            {
                HasReceivedAllFrames = HasReceivedAllFrames,
                Frames = Frames.Select(f => f.DeepClone(referenceLookup)).ToList()
            };

            referenceLookup[this] = clone;

            return clone;
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using MessagePack;
using osu.Game.Online.Spectator;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Replays
{
    [MessagePackObject]
    public class ReplayFrame : IDeepCloneable<ReplayFrame>
    {
        /// <summary>
        /// The time at which this <see cref="ReplayFrame"/> takes place.
        /// </summary>
        [Key(0)]
        public double Time;

        /// <summary>
        /// A <see cref="FrameHeader"/> containing the state of a play after this <see cref="ReplayFrame"/> takes place.
        /// May be omitted where exact per-frame accuracy is not required.
        /// </summary>
        [IgnoreMember]
        public FrameHeader? Header;

        public ReplayFrame()
        {
        }

        public ReplayFrame(double time)
        {
            Time = time;
        }

        public ReplayFrame DeepClone(IDictionary<object, object> referenceLookup)
        {
            if (referenceLookup.TryGetValue(this, out object? existing))
                return (ReplayFrame)existing;

            var clone = new ReplayFrame(Time)
            {
                Header = Header?.DeepClone(referenceLookup)
            };

            referenceLookup[this] = clone;

            return clone;
        }
    }
}

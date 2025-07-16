// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using MessagePack;
using osu.Game.Online.Spectator;

namespace osu.Game.Rulesets.Replays
{
    [MessagePackObject]
    public class ReplayFrame
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

        /// <summary>
        /// Whether this frame is equivalent to <paramref name="other"/> with respect to replay recording.
        /// </summary>
        public virtual bool IsEquivalentTo(ReplayFrame other) => Time == other.Time;
    }
}

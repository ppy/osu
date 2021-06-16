// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using MessagePack;

namespace osu.Game.Rulesets.Replays
{
    [MessagePackObject]
    public class ReplayFrame
    {
        [Key(0)]
        public double Time;

        public ReplayFrame()
        {
        }

        public ReplayFrame(double time)
        {
            Time = time;
        }
    }
}

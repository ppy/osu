// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Replays
{
    public class Replay
    {
        /// <summary>
        /// Whether all frames for this replay have been received.
        /// If false, gameplay would be paused to wait for further data, for instance.
        /// </summary>
        public bool HasReceivedAllFrames = true;

        public List<ReplayFrame> Frames = new List<ReplayFrame>();
    }
}

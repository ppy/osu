// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Replays.Legacy;

namespace osu.Game.Online.Spectator
{
    [Serializable]
    public class FrameDataBundle
    {
        public IEnumerable<LegacyReplayFrame> Frames { get; set; }

        public FrameDataBundle(IEnumerable<LegacyReplayFrame> frames)
        {
            Frames = frames;
        }
    }
}

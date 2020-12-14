// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Replays.Legacy;
using osu.Game.Scoring;

namespace osu.Game.Online.Spectator
{
    [Serializable]
    public class FrameDataBundle
    {
        public FrameHeader Header { get; set; }

        public IEnumerable<LegacyReplayFrame> Frames { get; set; }

        public FrameDataBundle(ScoreInfo score, IEnumerable<LegacyReplayFrame> frames)
        {
            Frames = frames;
            Header = new FrameHeader(score);
        }

        [JsonConstructor]
        public FrameDataBundle(FrameHeader header, IEnumerable<LegacyReplayFrame> frames)
        {
            Header = header;
            Frames = frames;
        }
    }
}

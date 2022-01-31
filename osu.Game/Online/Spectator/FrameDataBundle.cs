// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Collections.Generic;
using MessagePack;
using Newtonsoft.Json;
using osu.Game.Replays.Legacy;
using osu.Game.Scoring;

namespace osu.Game.Online.Spectator
{
    [Serializable]
    [MessagePackObject]
    public class FrameDataBundle
    {
        [Key(0)]
        public FrameHeader Header { get; set; }

        [Key(1)]
        public IList<LegacyReplayFrame> Frames { get; set; }

        public FrameDataBundle(ScoreInfo score, IList<LegacyReplayFrame> frames)
        {
            Frames = frames;
            Header = new FrameHeader(score);
        }

        [JsonConstructor]
        public FrameDataBundle(FrameHeader header, IList<LegacyReplayFrame> frames)
        {
            Header = header;
            Frames = frames;
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using MessagePack;
using Newtonsoft.Json;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets.Scoring;
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

        /// <summary>
        /// The sequence number of this frame bundle.
        /// Used to determine ordering of frame bundles, and for server-side checks that server received all frame bundles it was supposed to.
        /// </summary>
        [Key(2)]
        public long? SequenceNumber { get; set; }

        public FrameDataBundle(ScoreInfo score, ScoreProcessor scoreProcessor, IList<LegacyReplayFrame> frames)
        {
            Frames = frames;
            Header = new FrameHeader(score, scoreProcessor.GetScoreProcessorStatistics());
        }

        [JsonConstructor]
        public FrameDataBundle(FrameHeader header, IList<LegacyReplayFrame> frames, long? sequenceNumber)
        {
            Header = header;
            Frames = frames;
            SequenceNumber = sequenceNumber;
        }
    }
}

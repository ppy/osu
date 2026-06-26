// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using MessagePack;

namespace osu.Game.Online.Spectator
{
    /// <summary>
    /// Sent by the server to retrieve frame bundles that the client sent but never reached the server, if any.
    /// </summary>
    /// <param name="ScoreTokenId">The ID of the score token associated with the score with missing bundles.</param>
    /// <param name="FrameBundleSequenceNumbers">The sequence numbers of frame bundles that the server never received.</param>
    [Serializable]
    [MessagePackObject]
    public record CompleteReplayRequest(
        [property: Key(0)] long ScoreTokenId,
        [property: Key(1)] IEnumerable<long> FrameBundleSequenceNumbers
    );
}

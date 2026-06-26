// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using MessagePack;

namespace osu.Game.Online.Spectator
{
    /// <summary>
    /// Response to <see cref="CompleteReplayRequest"/>.
    /// </summary>
    /// <param name="FrameBundles">
    /// The frame bundles requested by sequence number in the corresponding <see cref="CompleteReplayRequest"/>.
    /// Can be blank (contain no frames) if the client does not have the frames available any more.
    /// </param>
    [Serializable]
    [MessagePackObject]
    public record CompleteReplayResponse(
        [property: Key(0)] IEnumerable<FrameDataBundle> FrameBundles
    );
}

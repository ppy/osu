// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;

namespace osu.Game.Online.RankedPlay
{
    [Serializable]
    [MessagePackObject]
    public readonly record struct RankedPlayCardState
    {
        [Key(0)]
        public required bool Hovered { get; init; }

        [Key(1)]
        public required bool Pressed { get; init; }

        [Key(2)]
        public required bool Selected { get; init; }
    }
}

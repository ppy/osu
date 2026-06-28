// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;
using osuTK;

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

        [Key(3)]
        public required bool Dragged { get; init; }

        [Key(4)]
        public required int Order { get; init; }

        [Key(5)]
        public float DragX { get; init; }

        [Key(6)]
        public float DragY { get; init; }

        [IgnoreMember]
        public Vector2 DragPosition
        {
            get => new Vector2(DragX, DragY);
            init
            {
                DragX = value.X;
                DragY = value.Y;
            }
        }
    }
}

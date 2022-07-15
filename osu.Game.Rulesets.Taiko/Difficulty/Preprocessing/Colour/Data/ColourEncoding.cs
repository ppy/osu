// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour.Data
{
    /// <summary>
    /// Encodes a list of <see cref="MonoEncoding"/>s.
    /// <see cref="MonoEncoding"/>s with the same <see cref="MonoEncoding.RunLength"/> are grouped together.
    /// </summary>
    public class ColourEncoding
    {
        /// <summary>
        /// <see cref="MonoEncoding"/>s that are grouped together within this <see cref="ColourEncoding"/>.
        /// </summary>
        public List<MonoEncoding> Payload { get; private set; } = new List<MonoEncoding>();

        /// <summary>
        /// Determine if this <see cref="ColourEncoding"/> is a repetition of another <see cref="ColourEncoding"/>. This
        /// is a strict comparison and is true if and only if the colour sequence is exactly the same.
        /// This does not require the <see cref="ColourEncoding"/>s to have the same amount of <see cref="MonoEncoding"/>s.
        /// </summary>
        public bool IsRepetitionOf(ColourEncoding other)
        {
            return HasIdenticalMonoLength(other) &&
                   other.Payload.Count == Payload.Count &&
                   (other.Payload[0].EncodedData[0].BaseObject as Hit)?.Type ==
                   (Payload[0].EncodedData[0].BaseObject as Hit)?.Type;
        }

        /// <summary>
        /// Determine if this <see cref="ColourEncoding"/> has the same mono length of another <see cref="ColourEncoding"/>.
        /// </summary>
        public bool HasIdenticalMonoLength(ColourEncoding other)
        {
            return other.Payload[0].RunLength == Payload[0].RunLength;
        }
    }
}

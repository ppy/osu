// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using Newtonsoft.Json;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Objects
{
    public class PathControlPoint : IEquatable<PathControlPoint>
    {
        private Vector2 position;

        /// <summary>
        /// The position of this <see cref="PathControlPoint"/>.
        /// </summary>
        [JsonProperty]
        public Vector2 Position
        {
            get => position;
            set
            {
                if (value == position)
                    return;

                position = value;
                Changed?.Invoke();
            }
        }

        private PathType? type;

        /// <summary>
        /// The type of path segment starting at this <see cref="PathControlPoint"/>.
        /// If null, this <see cref="PathControlPoint"/> will be a part of the previous path segment.
        /// </summary>
        [JsonProperty]
        public PathType? Type
        {
            get => type;
            set
            {
                if (value == type)
                    return;

                type = value;
                Changed?.Invoke();
            }
        }

        /// <summary>
        /// Invoked when any property of this <see cref="PathControlPoint"/> is changed.
        /// </summary>
        public event Action Changed;

        /// <summary>
        /// Creates a new <see cref="PathControlPoint"/>.
        /// </summary>
        public PathControlPoint()
        {
        }

        /// <summary>
        /// Creates a new <see cref="PathControlPoint"/> with a provided position and type.
        /// </summary>
        /// <param name="position">The initial position.</param>
        /// <param name="type">The initial type.</param>
        public PathControlPoint(Vector2 position, PathType? type = null)
            : this()
        {
            Position = position;
            Type = type;
        }

        public bool Equals(PathControlPoint other) => Position == other?.Position && Type == other.Type;

        public override string ToString() => type == null
            ? $"Position={Position}"
            : $"Position={Position}, Type={type}";
    }
}

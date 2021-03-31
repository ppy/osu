// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Objects
{
    public class PathControlPoint : IEquatable<PathControlPoint>
    {
        /// <summary>
        /// The position of this <see cref="PathControlPoint"/>.
        /// </summary>
        [JsonProperty]
        public readonly Bindable<Vector2> Position = new Bindable<Vector2>();

        /// <summary>
        /// The type of path segment starting at this <see cref="PathControlPoint"/>.
        /// If null, this <see cref="PathControlPoint"/> will be a part of the previous path segment.
        /// </summary>
        [JsonProperty]
        public readonly Bindable<PathType?> Type = new Bindable<PathType?>();

        /// <summary>
        /// Invoked when any property of this <see cref="PathControlPoint"/> is changed.
        /// </summary>
        internal event Action Changed;

        /// <summary>
        /// Creates a new <see cref="PathControlPoint"/>.
        /// </summary>
        public PathControlPoint()
        {
            Position.ValueChanged += _ => Changed?.Invoke();
            Type.ValueChanged += _ => Changed?.Invoke();
        }

        /// <summary>
        /// Creates a new <see cref="PathControlPoint"/> with a provided position and type.
        /// </summary>
        /// <param name="position">The initial position.</param>
        /// <param name="type">The initial type.</param>
        public PathControlPoint(Vector2 position, PathType? type = null)
            : this()
        {
            Position.Value = position;
            Type.Value = type;
        }

        public bool Equals(PathControlPoint other) => Position.Value == other?.Position.Value && Type.Value == other.Type.Value;
    }
}

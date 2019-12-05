using System;
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
        public readonly Bindable<Vector2> Position = new Bindable<Vector2>();

        /// <summary>
        /// The type of path segment starting at this <see cref="PathControlPoint"/>.
        /// If null, this <see cref="PathControlPoint"/> will be a part of the previous path segment.
        /// </summary>
        public readonly Bindable<PathType?> Type = new Bindable<PathType?>();

        /// <summary>
        /// Invoked when any property of this <see cref="PathControlPoint"/> is changed.
        /// </summary>
        internal event Action Changed;

        public PathControlPoint()
        {
            Position.ValueChanged += _ => Changed?.Invoke();
            Type.ValueChanged += _ => Changed?.Invoke();
        }

        public bool Equals(PathControlPoint other) => Position.Value == other.Position.Value && Type.Value == other.Type.Value;
    }
}

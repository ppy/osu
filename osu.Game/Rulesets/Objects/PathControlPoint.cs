using System;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Objects
{
    public class PathControlPoint : IEquatable<PathControlPoint>
    {
        public readonly Bindable<Vector2> Position = new Bindable<Vector2>();

        public readonly Bindable<PathType?> Type = new Bindable<PathType?>();

        public bool Equals(PathControlPoint other) => Position.Value == other.Position.Value && Type.Value == other.Type.Value;
    }
}

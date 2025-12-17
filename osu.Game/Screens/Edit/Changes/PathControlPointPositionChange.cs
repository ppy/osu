// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Screens.Edit.Changes
{
    public class PathControlPointPositionChange : PropertyChange<PathControlPoint, Vector2>
    {
        public PathControlPointPositionChange(PathControlPoint target, Vector2 value)
            : base(target, value)
        {
        }

        protected override Vector2 ReadValue(PathControlPoint target) => target.Position;

        protected override void WriteValue(PathControlPoint target, Vector2 value) => target.Position = value;
    }
}

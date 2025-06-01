// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Screens.Edit.Changes
{
    public class PositionChange : PropertyChange<IHasPosition, Vector2>
    {
        public PositionChange(IHasPosition target, Vector2 value)
            : base(target, value)
        {
        }

        protected override Vector2 ReadValue(IHasPosition target) => target.Position;

        protected override void WriteValue(IHasPosition target, Vector2 value) => target.Position = value;
    }
}

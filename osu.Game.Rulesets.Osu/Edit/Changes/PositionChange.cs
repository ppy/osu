// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Changes;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Changes
{
    public class PositionChange : PropertyChange<OsuHitObject, Vector2>
    {
        public PositionChange(OsuHitObject target, Vector2 value)
            : base(target, value)
        {
        }

        protected override Vector2 ReadValue(OsuHitObject target) => target.Position;

        protected override void WriteValue(OsuHitObject target, Vector2 value) => target.Position = value;
    }
}

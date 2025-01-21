// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Screens.Edit.Changes;

namespace osu.Game.Rulesets.Catch.Edit.Changes
{
    public class OriginalXChange : PropertyChange<CatchHitObject, float>
    {
        public OriginalXChange(CatchHitObject target, float value)
            : base(target, value)
        {
        }

        protected override float ReadValue(CatchHitObject target) => target.OriginalX;

        protected override void WriteValue(CatchHitObject target, float value) => target.OriginalX = value;
    }
}

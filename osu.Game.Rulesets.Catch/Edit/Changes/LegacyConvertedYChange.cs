// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Screens.Edit.Changes;

namespace osu.Game.Rulesets.Catch.Edit.Changes
{
    public class LegacyConvertedYChange : PropertyChange<CatchHitObject, float>
    {
        public LegacyConvertedYChange(CatchHitObject target, float value)
            : base(target, value)
        {
        }

        protected override float ReadValue(CatchHitObject target) => target.LegacyConvertedY;

        protected override void WriteValue(CatchHitObject target, float value) => target.LegacyConvertedY = value;
    }
}

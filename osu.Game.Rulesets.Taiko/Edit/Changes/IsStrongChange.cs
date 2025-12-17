// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Screens.Edit.Changes;

namespace osu.Game.Rulesets.Taiko.Edit.Changes
{
    public class IsStrongChange : PropertyChange<TaikoStrongableHitObject, bool>
    {
        public IsStrongChange(TaikoStrongableHitObject target, bool value)
            : base(target, value)
        {
        }

        protected override bool ReadValue(TaikoStrongableHitObject target) => target.IsStrong;

        protected override void WriteValue(TaikoStrongableHitObject target, bool value) => target.IsStrong = value;
    }
}

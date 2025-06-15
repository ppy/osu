// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Screens.Edit.Changes;

namespace osu.Game.Rulesets.Taiko.Edit.Changes
{
    public class HitTypeChange : PropertyChange<Hit, HitType>
    {
        public HitTypeChange(Hit target, HitType value)
            : base(target, value)
        {
        }

        protected override HitType ReadValue(Hit target) => target.Type;

        protected override void WriteValue(Hit target, HitType value) => target.Type = value;
    }
}

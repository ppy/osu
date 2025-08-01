// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Screens.Edit.Changes;

namespace osu.Game.Rulesets.Mania.Edit.Changes
{
    public class ColumnChange : PropertyChange<ManiaHitObject, int>
    {
        public ColumnChange(ManiaHitObject target, int value)
            : base(target, value)
        {
        }

        protected override int ReadValue(ManiaHitObject target) => target.Column;

        protected override void WriteValue(ManiaHitObject target, int value) => target.Column = value;
    }
}

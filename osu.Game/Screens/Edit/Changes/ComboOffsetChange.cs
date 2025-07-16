// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Screens.Edit.Changes
{
    public class ComboOffsetChange : PropertyChange<IHasComboInformation, int>
    {
        public ComboOffsetChange(IHasComboInformation target, int value)
            : base(target, value)
        {
        }

        protected override int ReadValue(IHasComboInformation target) => target.ComboOffset;

        protected override void WriteValue(IHasComboInformation target, int value) => target.ComboOffset = value;
    }
}

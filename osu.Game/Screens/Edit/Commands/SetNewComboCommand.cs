// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Screens.Edit.Commands
{
    public class SetNewComboCommand : PropertyChangeCommand<IHasComboInformation, bool>
    {
        public SetNewComboCommand(IHasComboInformation target, bool value)
            : base(target, value)
        {
        }

        protected override bool ReadValue(IHasComboInformation target) => target.NewCombo;

        protected override void WriteValue(IHasComboInformation target, bool value) => target.NewCombo = value;

        protected override SetNewComboCommand CreateInstance(IHasComboInformation target, bool value) => new SetNewComboCommand(target, value);
    }
}

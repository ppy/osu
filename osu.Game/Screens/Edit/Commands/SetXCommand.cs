// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Screens.Edit.Commands
{
    public class SetXCommand : PropertyChangeCommand<IHasMutableXPosition, float>
    {
        public SetXCommand(IHasMutableXPosition target, float value)
            : base(target, value)
        {
        }

        protected override float ReadValue(IHasMutableXPosition target) => target.X;

        protected override void WriteValue(IHasMutableXPosition target, float value) => target.X = value;

        protected override SetXCommand CreateInstance(IHasMutableXPosition target, float value) => new SetXCommand(target, value);
    }
}

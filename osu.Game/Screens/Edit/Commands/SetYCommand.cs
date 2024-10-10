// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Screens.Edit.Commands
{
    public class SetYCommand : PropertyChangeCommand<IHasMutableYPosition, float>
    {
        public SetYCommand(IHasMutableYPosition target, float value)
            : base(target, value)
        {
        }

        protected override float ReadValue(IHasMutableYPosition target) => target.Y;

        protected override void WriteValue(IHasMutableYPosition target, float value) => target.Y = value;

        protected override SetYCommand CreateInstance(IHasMutableYPosition target, float value) => new SetYCommand(target, value);
    }
}

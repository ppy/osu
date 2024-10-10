// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Screens.Edit.Commands
{
    public class SetPositionCommand : PropertyChangeCommand<IHasMutablePosition, Vector2>
    {
        public SetPositionCommand(IHasMutablePosition target, Vector2 value)
            : base(target, value)
        {
        }

        protected override Vector2 ReadValue(IHasMutablePosition target) => target.Position;

        protected override void WriteValue(IHasMutablePosition target, Vector2 value) => target.Position = value;

        protected override SetPositionCommand CreateInstance(IHasMutablePosition target, Vector2 value) => new SetPositionCommand(target, value);
    }
}

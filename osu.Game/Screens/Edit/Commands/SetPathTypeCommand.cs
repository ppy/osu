// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Screens.Edit.Commands
{
    public class SetPathTypeCommand : PropertyChangeCommand<PathControlPoint, PathType?>
    {
        public SetPathTypeCommand(PathControlPoint target, PathType? value)
            : base(target, value)
        {
        }

        protected override PathType? ReadValue(PathControlPoint target) => target.Type;

        protected override void WriteValue(PathControlPoint target, PathType? value) => target.Type = value;

        protected override SetPathTypeCommand CreateInstance(PathControlPoint target, PathType? value) => new SetPathTypeCommand(target, value);
    }
}

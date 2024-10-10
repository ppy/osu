// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Screens.Edit.Commands
{
    public class UpdateControlPointCommand : IEditorCommand
    {
        public PathControlPoint ControlPoint;

        public Vector2 Position;

        public PathType? Type;

        public UpdateControlPointCommand(PathControlPoint controlPoint)
        {
            ControlPoint = controlPoint;
            Position = controlPoint.Position;
            Type = controlPoint.Type;
        }

        public UpdateControlPointCommand(PathControlPoint controlPoint, Vector2 position, PathType? type)
        {
            ControlPoint = controlPoint;
            Position = position;
            Type = type;
        }

        public void Apply()
        {
            ControlPoint.Position = Position;
            ControlPoint.Type = Type;
        }

        public IEditorCommand CreateUndo()
        {
            return new UpdateControlPointCommand(ControlPoint, ControlPoint.Position, ControlPoint.Type);
        }
    }
}

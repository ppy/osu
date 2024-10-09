// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Commands;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Commands
{
    public class PathControlPointCommandProxy : CommandProxy
    {
        public PathControlPointCommandProxy(EditorCommandHandler? commandHandler, PathControlPoint controlPoint)
            : base(commandHandler)
        {
            ControlPoint = controlPoint;
        }

        public readonly PathControlPoint ControlPoint;

        public Vector2 Position
        {
            get => ControlPoint.Position;
            set => Submit(new UpdateControlPointCommand(ControlPoint) { Position = Position });
        }

        public PathType? Type
        {
            get => ControlPoint.Type;
            set => Submit(new UpdateControlPointCommand(ControlPoint) { Type = Type });
        }
    }
}

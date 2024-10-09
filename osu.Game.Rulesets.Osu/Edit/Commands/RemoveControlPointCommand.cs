// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Edit.Commands;

namespace osu.Game.Rulesets.Osu.Edit.Commands
{
    public class RemoveControlPointCommand : IEditorCommand
    {
        public readonly BindableList<PathControlPoint> ControlPoints;

        public readonly int Index;

        public RemoveControlPointCommand(BindableList<PathControlPoint> controlPoints, int index)
        {
            ControlPoints = controlPoints;
            Index = index;
        }

        public RemoveControlPointCommand(BindableList<PathControlPoint> controlPoints, PathControlPoint controlPoint)
        {
            ControlPoints = controlPoints;
            Index = controlPoints.IndexOf(controlPoint);
        }

        public void Apply() => ControlPoints.RemoveAt(Index);

        public IEditorCommand CreateUndo() => new AddControlPointCommand(ControlPoints, Index, ControlPoints[Index]);
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Edit.Commands;

namespace osu.Game.Rulesets.Osu.Edit.Commands
{
    public class AddControlPointCommand : IEditorCommand
    {
        public readonly BindableList<PathControlPoint> ControlPoints;

        public readonly int InsertionIndex;

        public readonly PathControlPoint ControlPoint;

        public AddControlPointCommand(BindableList<PathControlPoint> controlPoints, int insertionIndex, PathControlPoint controlPoint)
        {
            ControlPoints = controlPoints;
            InsertionIndex = insertionIndex;
            ControlPoint = controlPoint;
        }

        public void Apply() => ControlPoints.Insert(InsertionIndex, ControlPoint);

        public IEditorCommand CreateUndo() => new RemoveControlPointCommand(ControlPoints, InsertionIndex);
    }
}

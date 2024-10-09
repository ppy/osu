// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit.Commands
{
    public class SetExpectedDistanceCommand : IEditorCommand
    {
        public readonly SliderPath Path;

        public readonly double? ExpectedDistance;

        public SetExpectedDistanceCommand(SliderPath path, double? expectedDistance)
        {
            Path = path;
            ExpectedDistance = expectedDistance;
        }

        public void Apply() => Path.ExpectedDistance.Value = ExpectedDistance;

        public IEditorCommand CreateUndo() => new SetExpectedDistanceCommand(Path, Path.ExpectedDistance.Value);
    }
}

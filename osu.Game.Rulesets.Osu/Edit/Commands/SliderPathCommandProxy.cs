// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Commands;

namespace osu.Game.Rulesets.Osu.Edit.Commands
{
    public class SliderPathCommandProxy : CommandProxy
    {
        public SliderPathCommandProxy(EditorCommandHandler? commandHandler, SliderPath path)
            : base(commandHandler)
        {
            Path = path;
        }

        public readonly SliderPath Path;

        public double? ExpectedDistance
        {
            get => Path.ExpectedDistance.Value;
            set => Submit(new SetExpectedDistanceCommand(Path, value));
        }

        public PathControlPointsCommandProxy ControlPoints => new PathControlPointsCommandProxy(CommandHandler, Path.ControlPoints);

        public IEnumerable<double> GetSegmentEnds() => Path.GetSegmentEnds();
    }
}

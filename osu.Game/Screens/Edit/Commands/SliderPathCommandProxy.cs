// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit.Commands
{
    public static class SliderPathCommandProxy
    {
        public static double? ExpectedDistance(this CommandProxy<SliderPath> proxy) => proxy.Target.ExpectedDistance.Value;

        public static void SetExpectedDistance(this CommandProxy<SliderPath> proxy, double? value) => proxy.Submit(new SetExpectedDistanceCommand(proxy.Target, value));

        public static ListCommandProxy<BindableList<PathControlPoint>, PathControlPoint, CommandProxy<PathControlPoint>> ControlPoints(this CommandProxy<SliderPath> proxy) =>
            new ListCommandProxy<BindableList<PathControlPoint>, PathControlPoint, CommandProxy<PathControlPoint>>(proxy.CommandHandler, proxy.Target.ControlPoints);

        public static IEnumerable<double> GetSegmentEnds(this ICommandProxy<SliderPath> proxy) => proxy.Target.GetSegmentEnds();
    }
}

// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Timing;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests
{
    public abstract class ManiaSelectionBlueprintTestCase : SelectionBlueprintTestCase
    {
        [Cached(Type = typeof(IAdjustableClock))]
        private readonly IAdjustableClock clock = new StopwatchClock();
    }
}

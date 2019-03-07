// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

namespace osu.Game.Screens.Charts
{
    public class ChartListing : ScreenWhiteBox
    {
        protected override IEnumerable<Type> PossibleChildren => new[]
        {
            typeof(ChartInfo)
        };
    }
}

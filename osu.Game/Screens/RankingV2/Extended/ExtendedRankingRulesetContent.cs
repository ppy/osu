// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;

namespace osu.Game.Screens.RankingV2.Extended
{
    public record ExtendedRankingRulesetContent(
        Func<IEnumerable<Drawable>> LeftColumn,
        Func<IEnumerable<Drawable>> RightColumn);
}

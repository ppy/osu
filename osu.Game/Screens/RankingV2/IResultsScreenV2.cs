// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Screens.Footer;

namespace osu.Game.Screens.RankingV2
{
    /// <summary>
    /// Common interface for results screen implementations.
    /// </summary>
    [Cached]
    public interface IResultsScreenV2
    {
        Bindable<ScreenFooterContent?> FooterContent { get; }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Rankings
{
    public partial class RankingsSortTabControl : OverlaySortTabControl<RankingsSortCriteria>
    {
        public RankingsSortTabControl()
        {
            Title = RankingsStrings.FilterTitle.ToUpper();
        }
    }

    public enum RankingsSortCriteria
    {
        [LocalisableDescription(typeof(SortStrings), nameof(SortStrings.All))]
        All,

        [LocalisableDescription(typeof(SortStrings), nameof(SortStrings.Friends))]
        Friends
    }
}

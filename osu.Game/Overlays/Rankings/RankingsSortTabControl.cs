// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Rankings
{
    public class RankingsSortTabControl : OverlaySortTabControl<RankingsSortCriteria>
    {
        public RankingsSortTabControl()
        {
            Title = RankingsStrings.FilterTitle.ToUpper();
        }
    }

    [LocalisableEnum(typeof(RankingsSortCriteriaEnumLocalisationMapper))]
    public enum RankingsSortCriteria
    {
        All,
        Friends
    }

    public class RankingsSortCriteriaEnumLocalisationMapper : EnumLocalisationMapper<RankingsSortCriteria>
    {
        public override LocalisableString Map(RankingsSortCriteria value)
        {
            switch (value)
            {
                case RankingsSortCriteria.All:
                    return SortStrings.All;

                case RankingsSortCriteria.Friends:
                    return SortStrings.Friends;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}

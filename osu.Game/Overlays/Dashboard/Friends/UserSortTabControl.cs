// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.ComponentModel;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Dashboard.Friends
{
    public class UserSortTabControl : OverlaySortTabControl<UserSortCriteria>
    {
    }

    [LocalisableEnum(typeof(UserSortCriteriaEnumLocalisationMappper))]
    public enum UserSortCriteria
    {
        [Description(@"Recently Active")]
        LastVisit,
        Rank,
        Username
    }

    public class UserSortCriteriaEnumLocalisationMappper : EnumLocalisationMapper<UserSortCriteria>
    {
        public override LocalisableString Map(UserSortCriteria value)
        {
            switch (value)
            {
                case UserSortCriteria.LastVisit:
                    return SortStrings.LastVisit;

                case UserSortCriteria.Rank:
                    return SortStrings.Rank;

                case UserSortCriteria.Username:
                    return SortStrings.Username;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}

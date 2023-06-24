// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Dashboard.Friends
{
    public partial class UserSortTabControl : OverlaySortTabControl<UserSortCriteria>
    {
    }

    public enum UserSortCriteria
    {
        [LocalisableDescription(typeof(SortStrings), nameof(SortStrings.LastVisit))]
        [Description(@"Recently Active")]
        LastVisit,

        [LocalisableDescription(typeof(SortStrings), nameof(SortStrings.Rank))]
        Rank,

        [LocalisableDescription(typeof(SortStrings), nameof(SortStrings.Username))]
        Username
    }
}

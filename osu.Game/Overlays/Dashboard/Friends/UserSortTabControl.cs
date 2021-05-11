// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Overlays.Dashboard.Friends
{
    public class UserSortTabControl : OverlaySortTabControl<UserSortCriteria>
    {
    }

    public enum UserSortCriteria
    {
        [Description(@"Recently Active")]
        LastVisit,
        Rank,
        Username
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Dashboard.Friends
{
    [LocalisableEnum(typeof(OnlineStatusEnumLocalisationMapper))]
    public enum OnlineStatus
    {
        All,
        Online,
        Offline
    }

    public class OnlineStatusEnumLocalisationMapper : EnumLocalisationMapper<OnlineStatus>
    {
        public override LocalisableString Map(OnlineStatus value)
        {
            switch (value)
            {
                case OnlineStatus.All:
                    return SortStrings.All;

                case OnlineStatus.Online:
                    return UsersStrings.StatusOnline;

                case OnlineStatus.Offline:
                    return UsersStrings.StatusOffline;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}

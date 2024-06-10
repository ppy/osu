// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.ComponentModel;
using osu.Framework.Localisation;
using osuTK.Graphics;
using osu.Game.Graphics;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Users
{
    public enum UserStatus
    {
        [LocalisableDescription(typeof(UsersStrings), nameof(UsersStrings.StatusOffline))]
        Offline,

        [Description("Do not disturb")]
        DoNotDisturb,

        [LocalisableDescription(typeof(UsersStrings), nameof(UsersStrings.StatusOnline))]
        Online,
    }

    public static class UserStatusExtensions
    {
        public static Color4 GetAppropriateColour(this UserStatus userStatus, OsuColour colours)
        {
            switch (userStatus)
            {
                case UserStatus.Offline:
                    return Color4.Black;

                case UserStatus.DoNotDisturb:
                    return colours.RedDark;

                case UserStatus.Online:
                    return colours.GreenDark;

                default:
                    throw new ArgumentOutOfRangeException(nameof(userStatus), userStatus, "Unsupported user status");
            }
        }
    }
}

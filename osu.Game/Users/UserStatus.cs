// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Users
{
    public enum UserStatus
    {
        DND,
        AVAILABLE,
        BUSY,
        OFFLINE,
        ONLINE,
        INLOBBY,
        SPECTATING,
        INSOLO,
        MULTIPLAYING,
        MODDING
    }
    static class UserStatusMethods
    {
        private static Color4 AvailableColor(OsuColour colours) => colours.BlueDarker;
        private static Color4 BusyColor(OsuColour colours) => colours.YellowDark;

        public static Color4? GetAppropriateColour(this UserStatus status, OsuColour colours) {
            switch (status) {
                case UserStatus.ONLINE:
                    return AvailableColor(colours);
                    break;
                case UserStatus.OFFLINE:
                    return colours.Gray7;
                    break;
                case UserStatus.MODDING:
                    return colours.PurpleDark;
                    break;
                case UserStatus.INSOLO:
                    return BusyColor(colours);
                    break;
                case UserStatus.SPECTATING:
                    return AvailableColor(colours);
                    break;
                case UserStatus.INLOBBY:
                    return AvailableColor(colours);
                    break;
                case UserStatus.DND:
                    return colours.RedDark;
                    break;
                case UserStatus.MULTIPLAYING:
                    return BusyColor(colours);
                    break;
            }
            return null;
        }

        public static string GetMessage(this UserStatus status) {
            switch (status) {
                case UserStatus.ONLINE:
                    return "Online";
                    break;
                case UserStatus.OFFLINE:
                    return "Offline";
                    break;
                case UserStatus.MODDING:
                    return "Modding A Map";
                    break;
                case UserStatus.INSOLO:
                    return "Solo Game";
                    break;
                case UserStatus.SPECTATING:
                    return "Spectating a game";
                    break;
                case UserStatus.INLOBBY:
                    return "in Multiplayer Lobby";
                    break;
                case UserStatus.DND:
                    return "Do not disturb";
                    break;
                case UserStatus.MULTIPLAYING:
                    return "Multiplaying";
                    break;
            }
            return "Invalid Status";
        }
    }
}

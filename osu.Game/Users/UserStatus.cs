// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Users
{
    public abstract class UserStatus
    {
        public abstract string Message { get; }
        public abstract Color4 GetAppropriateColour(OsuColour colours);
    }

    public abstract class UserStatusAvailable : UserStatus
    {
        public override Color4 GetAppropriateColour(OsuColour colours) => colours.BlueDarker;
    }

    public abstract class UserStatusBusy : UserStatus
    {
        public override Color4 GetAppropriateColour(OsuColour colours) => colours.YellowDark;
    }

    public class UserStatusOffline : UserStatus
    {
        public override string Message => @"Offline";
        public override Color4 GetAppropriateColour(OsuColour colours) => colours.Gray7;
    }

    public class UserStatusOnline : UserStatusAvailable
    {
        public override string Message => @"Online";
    }

    public class UserStatusSpectating : UserStatusAvailable
    {
        public override string Message => @"Spectating a game";
    }

    public class UserStatusInLobby : UserStatusAvailable
    {
        public override string Message => @"in Multiplayer Lobby";
    }

    public class UserStatusSoloGame :  UserStatusBusy
    {
        public override string Message => @"Solo Game";
    }

    public class UserStatusMultiplayerGame : UserStatusBusy
    {
        public override string Message => @"Multiplaying";
    }

    public class UserStatusModding : UserStatus
    {
        public override string Message => @"Modding a map";
        public override Color4 GetAppropriateColour(OsuColour colours) => colours.PurpleDark;
    }

    public class UserStatusDoNotDisturb : UserStatus
    {
        public override string Message => @"Do not disturb";
        public override Color4 GetAppropriateColour(OsuColour colours) => colours.RedDark;
    }
}

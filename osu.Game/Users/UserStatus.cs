// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Users
{
    public abstract class UserStatus
    {
        public abstract string Message { get; }
        public abstract Color4 GetAppropriateColour(OsuColour colours);
    }

    public class UserStatusOnline : UserStatus
    {
        public override string Message => @"Online";
        public override Color4 GetAppropriateColour(OsuColour colours) => colours.BlueDarker;
    }

    public abstract class UserStatusBusy : UserStatusOnline
    {
        public override Color4 GetAppropriateColour(OsuColour colours) => colours.YellowDark;
    }

    public class UserStatusOffline : UserStatus
    {
        public override string Message => @"Offline";
        public override Color4 GetAppropriateColour(OsuColour colours) => colours.Gray7;
    }

    public class UserStatusDoNotDisturb : UserStatus
    {
        public override string Message => @"Do not disturb";
        public override Color4 GetAppropriateColour(OsuColour colours) => colours.RedDark;
    }
}

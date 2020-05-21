// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Users
{
    public abstract class UserStatus
    {
        public abstract string Message { get; }
        public abstract Colour4 GetAppropriateColour(OsuColour colours);
    }

    public class UserStatusOnline : UserStatus
    {
        public override string Message => @"Online";
        public override Colour4 GetAppropriateColour(OsuColour colours) => colours.GreenLight;
    }

    public abstract class UserStatusBusy : UserStatusOnline
    {
        public override Colour4 GetAppropriateColour(OsuColour colours) => colours.YellowDark;
    }

    public class UserStatusOffline : UserStatus
    {
        public override string Message => @"Offline";
        public override Colour4 GetAppropriateColour(OsuColour colours) => Colour4.Black;
    }

    public class UserStatusDoNotDisturb : UserStatus
    {
        public override string Message => @"Do not disturb";
        public override Colour4 GetAppropriateColour(OsuColour colours) => colours.RedDark;
    }
}

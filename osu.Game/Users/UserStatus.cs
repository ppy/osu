// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osuTK.Graphics;
using osu.Game.Graphics;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Users
{
    public abstract class UserStatus
    {
        public abstract LocalisableString Message { get; }
        public abstract Color4 GetAppropriateColour(OsuColour colours);
    }

    public class UserStatusOnline : UserStatus
    {
        public override LocalisableString Message => UsersStrings.StatusOnline;
        public override Color4 GetAppropriateColour(OsuColour colours) => colours.GreenLight;
    }

    public abstract class UserStatusBusy : UserStatusOnline
    {
        public override Color4 GetAppropriateColour(OsuColour colours) => colours.YellowDark;
    }

    public class UserStatusOffline : UserStatus
    {
        public override LocalisableString Message => UsersStrings.StatusOffline;
        public override Color4 GetAppropriateColour(OsuColour colours) => Color4.Black;
    }

    public class UserStatusDoNotDisturb : UserStatus
    {
        public override LocalisableString Message => "Do not disturb";
        public override Color4 GetAppropriateColour(OsuColour colours) => colours.RedDark;
    }
}

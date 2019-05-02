// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Graphics;
using osu.Game.Graphics;
using osu.Game.Beatmaps;

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

    public class UserStatusSpectating : UserStatusOnline
    {
        public override string Message => @"Spectating a game";
    }

    public class UserStatusInLobby : UserStatusOnline
    {
        public override string Message => @"in Multiplayer Lobby";
    }

    public class UserStatusSoloGame : UserStatusBusy
    {
        public UserStatusSoloGame(BeatmapInfo info, Rulesets.RulesetInfo ruleset)
        {
            Beatmap = info;
            Ruleset = ruleset;
        }

        public override string Message => @"Solo Game";

        public BeatmapInfo Beatmap { get; }

        public Rulesets.RulesetInfo Ruleset { get; }
    }

    public class UserStatusEditing : UserStatusBusy
    {
        public UserStatusEditing(BeatmapInfo info)
        {
            Beatmap = info;
        }

        public override string Message => @"Editing a beatmap";

        public override Color4 GetAppropriateColour(OsuColour colours) => colours.GreenDarker;

        public BeatmapInfo Beatmap { get; }
    }

    public class UserStatusChoosingBeatmap : UserStatusOnline
    {
        public override string Message => @"Choosing a beatmap";
    }

    public class UserStatusMultiplayerGame : UserStatusBusy
    {
        public override string Message => @"Multiplaying";
    }

    public class UserStatusModding : UserStatusOnline
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

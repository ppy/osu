// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osuTK.Graphics;

namespace osu.Game.Users
{
    public abstract class UserActivity
    {
        public abstract string Status { get; }
        public virtual Color4 GetAppropriateColour(OsuColour colours) => colours.GreenDarker;

        public class Modding : UserActivity
        {
            public override string Status => "正在摸图";
            public override Color4 GetAppropriateColour(OsuColour colours) => colours.PurpleDark;
        }

        public class ChoosingBeatmap : UserActivity
        {
            public override string Status => "正在选图";
        }

        public abstract class InGame : UserActivity
        {
            public BeatmapInfo Beatmap { get; }

            public RulesetInfo Ruleset { get; }

            protected InGame(BeatmapInfo info, RulesetInfo ruleset)
            {
                Beatmap = info;
                Ruleset = ruleset;
            }

            public override string Status => Ruleset.CreateInstance().PlayingVerb;
        }

        public class InMvis : InGame
        {
            public InMvis(BeatmapInfo info)
                : base(info, info.Ruleset)
            {
            }

            public override string Status => "正在听歌";
        }

        public class InMultiplayerGame : InGame
        {
            public InMultiplayerGame(BeatmapInfo beatmap, RulesetInfo ruleset)
                : base(beatmap, ruleset)
            {
            }

            public override string Status => $@"正和别人一起{base.Status}";
        }

        public class InPlaylistGame : InGame
        {
            public InPlaylistGame(BeatmapInfo beatmap, RulesetInfo ruleset)
                : base(beatmap, ruleset)
            {
            }
        }

        public class InSoloGame : InGame
        {
            public InSoloGame(BeatmapInfo info, RulesetInfo ruleset)
                : base(info, ruleset)
            {
            }
        }

        public class Editing : UserActivity
        {
            public BeatmapInfo Beatmap { get; }

            public Editing(BeatmapInfo info)
            {
                Beatmap = info;
            }

            public override string Status => @"正在编辑谱面";
        }

        public class Spectating : UserActivity
        {
            public override string Status => @"正在旁观别人";
        }

        public class SearchingForLobby : UserActivity
        {
            public override string Status => @"正在寻找多人游戏房间";
        }

        public class InLobby : UserActivity
        {
            public override string Status => @"在多人游戏大厅中";

            public readonly Room Room;

            public InLobby(Room room)
            {
                Room = room;
            }
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osuTK.Graphics;

namespace osu.Game.Users
{
    public abstract class UserActivity
    {
        public abstract string GetStatus(bool hideIdentifiableInformation = false);

        public virtual Color4 GetAppropriateColour(OsuColour colours) => colours.GreenDarker;

        public class ModdingBeatmap : EditingBeatmap
        {
            public override string GetStatus(bool hideIdentifiableInformation = false) => "Modding a beatmap";
            public override Color4 GetAppropriateColour(OsuColour colours) => colours.PurpleDark;

            public ModdingBeatmap(IBeatmapInfo info)
                : base(info)
            {
            }
        }

        public class ChoosingBeatmap : UserActivity
        {
            public override string GetStatus(bool hideIdentifiableInformation = false) => "Choosing a beatmap";
        }

        public abstract class InGame : UserActivity
        {
            public IBeatmapInfo BeatmapInfo { get; }

            public IRulesetInfo Ruleset { get; }

            protected InGame(IBeatmapInfo beatmapInfo, IRulesetInfo ruleset)
            {
                BeatmapInfo = beatmapInfo;
                Ruleset = ruleset;
            }

            public override string GetStatus(bool hideIdentifiableInformation = false) => Ruleset.CreateInstance().PlayingVerb;
        }

        public class InMultiplayerGame : InGame
        {
            public InMultiplayerGame(IBeatmapInfo beatmapInfo, IRulesetInfo ruleset)
                : base(beatmapInfo, ruleset)
            {
            }

            public override string GetStatus(bool hideIdentifiableInformation = false) => $@"{base.GetStatus(hideIdentifiableInformation)} with others";
        }

        public class SpectatingMultiplayerGame : InGame
        {
            public SpectatingMultiplayerGame(IBeatmapInfo beatmapInfo, IRulesetInfo ruleset)
                : base(beatmapInfo, ruleset)
            {
            }

            public override string GetStatus(bool hideIdentifiableInformation = false) => $"Watching others {base.GetStatus(hideIdentifiableInformation).ToLowerInvariant()}";
        }

        public class InPlaylistGame : InGame
        {
            public InPlaylistGame(IBeatmapInfo beatmapInfo, IRulesetInfo ruleset)
                : base(beatmapInfo, ruleset)
            {
            }
        }

        public class InSoloGame : InGame
        {
            public InSoloGame(IBeatmapInfo beatmapInfo, IRulesetInfo ruleset)
                : base(beatmapInfo, ruleset)
            {
            }
        }

        public class TestingBeatmap : InGame
        {
            public override string GetStatus(bool hideIdentifiableInformation = false) => "Testing a beatmap";

            public TestingBeatmap(IBeatmapInfo beatmapInfo, IRulesetInfo ruleset)
                : base(beatmapInfo, ruleset)
            {
            }
        }

        public class EditingBeatmap : UserActivity
        {
            public IBeatmapInfo BeatmapInfo { get; }

            public EditingBeatmap(IBeatmapInfo info)
            {
                BeatmapInfo = info;
            }

            public override string GetStatus(bool hideIdentifiableInformation = false) => @"Editing a beatmap";
        }

        public class WatchingReplay : UserActivity
        {
            private readonly ScoreInfo score;

            protected string Username => score.User.Username;

            public BeatmapInfo? BeatmapInfo => score.BeatmapInfo;

            public WatchingReplay(ScoreInfo score)
            {
                this.score = score;
            }

            public override string GetStatus(bool hideIdentifiableInformation = false) => hideIdentifiableInformation ? @"Watching a replay" : $@"Watching {Username}'s replay";
        }

        public class SpectatingUser : WatchingReplay
        {
            public override string GetStatus(bool hideIdentifiableInformation = false) => hideIdentifiableInformation ? @"Spectating a user" : $@"Spectating {Username}";

            public SpectatingUser(ScoreInfo score)
                : base(score)
            {
            }
        }

        public class SearchingForLobby : UserActivity
        {
            public override string GetStatus(bool hideIdentifiableInformation = false) => @"Looking for a lobby";
        }

        public class InLobby : UserActivity
        {
            public override string GetStatus(bool hideIdentifiableInformation = false) => @"In a lobby";

            public readonly Room Room;

            public InLobby(Room room)
            {
                Room = room;
            }
        }
    }
}

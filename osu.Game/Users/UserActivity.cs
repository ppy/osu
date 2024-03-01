// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Online;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osuTK.Graphics;

namespace osu.Game.Users
{
    /// <summary>
    /// Base class for all structures describing the user's current activity.
    /// </summary>
    /// <remarks>
    /// Warning: keep <see cref="UnionAttribute"/> specs consistent with
    /// <see cref="SignalRWorkaroundTypes.BASE_TYPE_MAPPING"/>.
    /// </remarks>
    [Serializable]
    [MessagePackObject]
    [Union(11, typeof(ChoosingBeatmap))]
    [Union(12, typeof(InSoloGame))]
    [Union(13, typeof(WatchingReplay))]
    [Union(14, typeof(SpectatingUser))]
    [Union(21, typeof(SearchingForLobby))]
    [Union(22, typeof(InLobby))]
    [Union(23, typeof(InMultiplayerGame))]
    [Union(24, typeof(SpectatingMultiplayerGame))]
    [Union(31, typeof(InPlaylistGame))]
    [Union(41, typeof(EditingBeatmap))]
    [Union(42, typeof(ModdingBeatmap))]
    [Union(43, typeof(TestingBeatmap))]
    public abstract class UserActivity
    {
        public abstract string GetStatus(bool hideIdentifiableInformation = false);
        public virtual string? GetDetails(bool hideIdentifiableInformation = false) => null;

        public virtual Color4 GetAppropriateColour(OsuColour colours) => colours.GreenDarker;

        [MessagePackObject]
        public class ChoosingBeatmap : UserActivity
        {
            public override string GetStatus(bool hideIdentifiableInformation = false) => "Choosing a beatmap";
        }

        [MessagePackObject]
        public abstract class InGame : UserActivity
        {
            [Key(0)]
            public int BeatmapID { get; set; }

            [Key(1)]
            public string BeatmapDisplayTitle { get; set; } = string.Empty;

            [Key(2)]
            public int RulesetID { get; set; }

            [Key(3)]
            public string RulesetPlayingVerb { get; set; } = string.Empty; // TODO: i'm going with this for now, but this is wasteful

            protected InGame(IBeatmapInfo beatmapInfo, IRulesetInfo ruleset)
            {
                BeatmapID = beatmapInfo.OnlineID;
                BeatmapDisplayTitle = beatmapInfo.GetDisplayTitle();

                RulesetID = ruleset.OnlineID;
                RulesetPlayingVerb = ruleset.CreateInstance().PlayingVerb;
            }

            [SerializationConstructor]
            protected InGame() { }

            public override string GetStatus(bool hideIdentifiableInformation = false) => RulesetPlayingVerb;
            public override string GetDetails(bool hideIdentifiableInformation = false) => BeatmapDisplayTitle;
        }

        [MessagePackObject]
        public class InSoloGame : InGame
        {
            public InSoloGame(IBeatmapInfo beatmapInfo, IRulesetInfo ruleset)
                : base(beatmapInfo, ruleset)
            {
            }

            [SerializationConstructor]
            public InSoloGame() { }
        }

        [MessagePackObject]
        public class InMultiplayerGame : InGame
        {
            public InMultiplayerGame(IBeatmapInfo beatmapInfo, IRulesetInfo ruleset)
                : base(beatmapInfo, ruleset)
            {
            }

            [SerializationConstructor]
            public InMultiplayerGame()
            {
            }

            public override string GetStatus(bool hideIdentifiableInformation = false) => $@"{base.GetStatus(hideIdentifiableInformation)} with others";
        }

        [MessagePackObject]
        public class InPlaylistGame : InGame
        {
            public InPlaylistGame(IBeatmapInfo beatmapInfo, IRulesetInfo ruleset)
                : base(beatmapInfo, ruleset)
            {
            }

            [SerializationConstructor]
            public InPlaylistGame() { }
        }

        [MessagePackObject]
        public class TestingBeatmap : InGame
        {
            public TestingBeatmap(IBeatmapInfo beatmapInfo, IRulesetInfo ruleset)
                : base(beatmapInfo, ruleset)
            {
            }

            [SerializationConstructor]
            public TestingBeatmap() { }

            public override string GetStatus(bool hideIdentifiableInformation = false) => "Testing a beatmap";
        }

        [MessagePackObject]
        public class EditingBeatmap : UserActivity
        {
            [Key(0)]
            public int BeatmapID { get; set; }

            [Key(1)]
            public string BeatmapDisplayTitle { get; set; } = string.Empty;

            public EditingBeatmap(IBeatmapInfo info)
            {
                BeatmapID = info.OnlineID;
                BeatmapDisplayTitle = info.GetDisplayTitle();
            }

            [SerializationConstructor]
            public EditingBeatmap() { }

            public override string GetStatus(bool hideIdentifiableInformation = false) => @"Editing a beatmap";

            public override string GetDetails(bool hideIdentifiableInformation = false) => hideIdentifiableInformation
                // For now let's assume that showing the beatmap a user is editing could reveal unwanted information.
                ? string.Empty
                : BeatmapDisplayTitle;
        }

        [MessagePackObject]
        public class ModdingBeatmap : EditingBeatmap
        {
            public ModdingBeatmap(IBeatmapInfo info)
                : base(info)
            {
            }

            [SerializationConstructor]
            public ModdingBeatmap() { }

            public override string GetStatus(bool hideIdentifiableInformation = false) => "Modding a beatmap";
            public override Color4 GetAppropriateColour(OsuColour colours) => colours.PurpleDark;
        }

        [MessagePackObject]
        public class WatchingReplay : UserActivity
        {
            [Key(0)]
            public long ScoreID { get; set; }

            [Key(1)]
            public string PlayerName { get; set; } = string.Empty;

            [Key(2)]
            public int BeatmapID { get; set; }

            [Key(3)]
            public string? BeatmapDisplayTitle { get; set; }

            public WatchingReplay(ScoreInfo score)
            {
                ScoreID = score.OnlineID;
                PlayerName = score.User.Username;
                BeatmapID = score.BeatmapInfo?.OnlineID ?? -1;
                BeatmapDisplayTitle = score.BeatmapInfo?.GetDisplayTitle();
            }

            [SerializationConstructor]
            public WatchingReplay() { }

            public override string GetStatus(bool hideIdentifiableInformation = false) => hideIdentifiableInformation ? @"Watching a replay" : $@"Watching {PlayerName}'s replay";
            public override string? GetDetails(bool hideIdentifiableInformation = false) => BeatmapDisplayTitle;
        }

        [MessagePackObject]
        public class SpectatingUser : WatchingReplay
        {
            public SpectatingUser(ScoreInfo score)
                : base(score)
            {
            }

            [SerializationConstructor]
            public SpectatingUser() { }

            public override string GetStatus(bool hideIdentifiableInformation = false) => hideIdentifiableInformation ? @"Spectating a user" : $@"Spectating {PlayerName}";
        }

        [MessagePackObject]
        public class SpectatingMultiplayerGame : InGame
        {
            public SpectatingMultiplayerGame(IBeatmapInfo beatmapInfo, IRulesetInfo ruleset)
                : base(beatmapInfo, ruleset)
            {
            }

            [SerializationConstructor]
            public SpectatingMultiplayerGame() { }

            public override string GetStatus(bool hideIdentifiableInformation = false) => $"Watching others {base.GetStatus(hideIdentifiableInformation).ToLowerInvariant()}";
        }

        [MessagePackObject]
        public class SearchingForLobby : UserActivity
        {
            public override string GetStatus(bool hideIdentifiableInformation = false) => @"Looking for a lobby";
        }

        [MessagePackObject]
        public class InLobby : UserActivity
        {
            [Key(0)]
            public long RoomID { get; set; }

            [Key(1)]
            public string RoomName { get; set; } = string.Empty;

            public InLobby(Room room)
            {
                RoomID = room.RoomID.Value ?? -1;
                RoomName = room.Name.Value;
            }

            [SerializationConstructor]
            public InLobby() { }

            public override string GetStatus(bool hideIdentifiableInformation = false) => @"In a lobby";

            public override string? GetDetails(bool hideIdentifiableInformation = false) => hideIdentifiableInformation
                ? null
                : RoomName;
        }
    }
}

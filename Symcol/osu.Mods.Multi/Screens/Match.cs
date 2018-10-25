using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets;
using osu.Mods.Multi.Networking;
using osu.Mods.Multi.Networking.Packets.Lobby;
using osu.Mods.Multi.Networking.Packets.Match;
using osu.Mods.Multi.Screens.Pieces;
using Symcol.Networking.Packets;

namespace osu.Mods.Multi.Screens
{
    public class Match : MultiScreen
    {
        private readonly MatchListPacket.MatchInfo match;

        private BeatmapManager beatmaps;

        protected MatchTools MatchTools;

        private RulesetStore rulesets;

        public Match(OsuNetworkingHandler osuNetworkingHandler, JoinedMatchPacket joinedPacket)
            : base(osuNetworkingHandler)
        {
            Name = "Match";
            match = joinedPacket.MatchInfo;

            SendPacket(new GetMapPacket());

            MatchPlayerList playerList;
            Children = new Drawable[]
            {
                new SettingsButton
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.X,
                    Width = 0.35f,
                    Text = "Leave",
                    Action = Exit
                },
                new SettingsButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    Width = 0.3f,
                    Text = "Open Song Select",
                    Action = openSongSelect
                },
                new SettingsButton
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    RelativeSizeAxes = Axes.X,
                    Width = 0.35f,
                    Text = "Start Match",
                    Action = () => SendPacket(new StartMatchPacket())
                },
                playerList = new MatchPlayerList(OsuNetworkingHandler),
                MatchTools = new MatchTools(OsuNetworkingHandler, Ruleset),
                new Chat(OsuNetworkingHandler)
            };

            foreach (OsuUserInfo user in joinedPacket.MatchInfo.Users)
                playerList.Add(user);
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapManager beatmaps, RulesetStore rulesets)
        {
            this.beatmaps = beatmaps;
            this.rulesets = rulesets;
        }

        protected override void OnPacketRecieve(PacketInfo info)
        {
            if (info.Packet is MatchLoadingPacket loading)
                Load(loading.Users);
        }

        protected virtual void Load(List<OsuUserInfo> users)
        {
            if (MatchTools.SelectedBeatmap != null && !Beatmap.Disabled)
                Beatmap.Value = MatchTools.SelectedBeatmap;

            if (MatchTools.SelectedRuleset != null && !Ruleset.Disabled)
                Ruleset.Value = MatchTools.SelectedRuleset;

            Push(new MultiPlayer(OsuNetworkingHandler, users));
        }

        protected override void Dispose(bool isDisposing)
        {
            SendPacket(new LeavePacket());
            base.Dispose(isDisposing);
        }

        private void openSongSelect()
        {
            SongSelect songSelect = new SongSelect(OsuNetworkingHandler);
            Push(songSelect);
            songSelect.SelectionFinalised = map =>
            {
                try
                {
                    OsuNetworkingHandler.SendToServer(new SetMapPacket
                    {
                        OnlineBeatmapSetID = (int)map.BeatmapSetInfo.OnlineBeatmapSetID,
                        OnlineBeatmapID = (int)map.BeatmapInfo.OnlineBeatmapID,
                        BeatmapTitle = map.Metadata.Title,
                        BeatmapArtist = map.Metadata.Artist,
                        BeatmapMapper = map.Metadata.Author.Username,
                        BeatmapDifficulty = map.BeatmapInfo.Version,
                        RulesetID = Ruleset.Value.ID.Value
                    });
                }
                catch
                {
                    //try to fallback for old maps
                    OsuNetworkingHandler.SendToServer(new SetMapPacket
                    {
                        BeatmapTitle = map.Metadata.Title,
                        BeatmapArtist = map.Metadata.Artist,
                        BeatmapMapper = map.Metadata.Author.Username,
                        BeatmapDifficulty = map.BeatmapInfo.Version,
                        RulesetID = Ruleset.Value.ID.Value
                    });
                }
            };
        }
    }
}


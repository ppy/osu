using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Game;
using osu.Game.Beatmaps;
using osu.Game.Overlays.Settings;
using osu.Game.Screens;
using Symcol.Core.Networking;
using Symcol.Rulesets.Core.Multiplayer.Networking;
using Symcol.Rulesets.Core.Multiplayer.Pieces;
using System.Collections.Generic;

namespace Symcol.Rulesets.Core.Multiplayer.Screens
{
    public abstract class RulesetMatchScreen : OsuScreen
    {
        public readonly RulesetNetworkingClientHandler RulesetNetworkingClientHandler;

        private readonly MatchPlayerList playerList;

        private BeatmapManager beatmaps;

        protected MatchTools MatchTools;

        private readonly Chat chat;

        public RulesetMatchScreen(RulesetNetworkingClientHandler rulesetNetworkingClientHandler)
        {
            RulesetNetworkingClientHandler = rulesetNetworkingClientHandler;

            Children = new Drawable[]
            {
                new SettingsButton
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.X,
                    Width = 0.35f,
                    Text = "Leave",
                    Action = () => Exit()
                },
                new SettingsButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    Width = 0.3f,
                    Text = "Open Song Select",
                    Action = () => openSongSelect()
                },
                new SettingsButton
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    RelativeSizeAxes = Axes.X,
                    Width = 0.35f,
                    Text = "Start Match",
                    Action = () => RulesetNetworkingClientHandler.StartLoadingGame()
                },
                playerList = new MatchPlayerList(RulesetNetworkingClientHandler),
                MatchTools = new MatchTools(),
                chat = new Chat(RulesetNetworkingClientHandler)
            };

            RulesetNetworkingClientHandler.OnPacketReceive += (Packet packet) =>
            {
                if (packet is RulesetPacket rulesetPacket && rulesetPacket.OnlineBeatmapID != -1)
                    foreach (BeatmapSetInfo beatmapSet in beatmaps.GetAllUsableBeatmapSets())
                        if (beatmapSet.OnlineBeatmapSetID == rulesetPacket.OnlineBeatmapSetID)
                        {
                            foreach (BeatmapInfo beatmap in beatmapSet.Beatmaps)
                                if (beatmap.OnlineBeatmapID == rulesetPacket.OnlineBeatmapID)
                                {
                                    Beatmap.Value = beatmaps.GetWorkingBeatmap(beatmap, Beatmap.Value);
                                    Beatmap.Value.Track.Start();
                                    MatchTools.MapChange(Beatmap);
                                    RulesetNetworkingClientHandler.OnMapChange?.Invoke(Beatmap);
                                    break;
                                }
                            break;
                        }
                        else
                            MatchTools.MapChange(rulesetPacket.OnlineBeatmapSetID);
            };

            RulesetNetworkingClientHandler.OnMapChange += (beatmap) => MatchTools.MapChange(beatmap);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            playerList.Add(RulesetNetworkingClientHandler.RulesetClientInfo);
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapManager beatmaps)
        {
            this.beatmaps = beatmaps;
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);
            MakeCurrent();
            Add(RulesetNetworkingClientHandler);
            RulesetNetworkingClientHandler.OnLoadGame = (i) => Load(i);
        }

        protected override void OnResuming(Screen last)
        {
            base.OnResuming(last);
            MakeCurrent();
            if (RulesetNetworkingClientHandler != null)
                Add(RulesetNetworkingClientHandler);
        }

        protected override void OnSuspending(Screen next)
        {
            base.OnSuspending(next);
            Remove(RulesetNetworkingClientHandler);
        }

        protected override bool OnExiting(Screen next)
        {
            RulesetNetworkingClientHandler.Disconnect();
            Remove(RulesetNetworkingClientHandler);
            RulesetNetworkingClientHandler.Dispose();

            return base.OnExiting(next);
        }

        protected virtual void Load(List<ClientInfo> playerList)
        {
            MakeCurrent();
            Push(new MultiPlayer(RulesetNetworkingClientHandler, playerList));
        }

        private void openSongSelect()
        {
            MatchSongSelect songSelect = new MatchSongSelect(RulesetNetworkingClientHandler);
            MakeCurrent();
            Push(songSelect);
            songSelect.Action = () => RulesetNetworkingClientHandler.SetMap(songSelect.SelectedMap);
        }
    }
}

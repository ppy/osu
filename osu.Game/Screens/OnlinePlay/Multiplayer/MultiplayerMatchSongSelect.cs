// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Screens.Select;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public class MultiplayerMatchSongSelect : SongSelect, IOnlinePlaySubScreen
    {
        public string ShortTitle => "song selection";

        public override string Title => ShortTitle.Humanize();

        [Resolved(typeof(Room), nameof(Room.Playlist))]
        private BindableList<PlaylistItem> playlist { get; set; }

        [Resolved]
        private StatefulMultiplayerClient client { get; set; }

        private LoadingLayer loadingLayer;

        public MultiplayerMatchSongSelect()
        {
            Padding = new MarginPadding { Horizontal = HORIZONTAL_OVERFLOW_PADDING };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(loadingLayer = new LoadingLayer(Carousel));
        }

        protected override bool OnStart()
        {
            var item = new PlaylistItem();

            item.Beatmap.Value = Beatmap.Value.BeatmapInfo;
            item.Ruleset.Value = Ruleset.Value;

            item.RequiredMods.Clear();
            item.RequiredMods.AddRange(Mods.Value.Select(m => m.CreateCopy()));

            // If the client is already in a room, update via the client.
            // Otherwise, update the playlist directly in preparation for it to be submitted to the API on match creation.
            if (client.Room != null)
            {
                loadingLayer.Show();

                client.ChangeSettings(item: item).ContinueWith(t =>
                {
                    Schedule(() =>
                    {
                        loadingLayer.Hide();

                        if (t.IsCompletedSuccessfully)
                            this.Exit();
                        else
                        {
                            Logger.Log($"Could not use current beatmap ({t.Exception?.Message})", level: LogLevel.Important);
                            Carousel.AllowSelection = true;
                        }
                    });
                });
            }
            else
            {
                playlist.Clear();
                playlist.Add(item);
                this.Exit();
            }

            return true;
        }

        protected override BeatmapDetailArea CreateBeatmapDetailArea() => new PlayBeatmapDetailArea();
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.OnlinePlay.Components;

namespace osu.Game.Screens.Select
{
    public class MatchSongSelect : SongSelect, IOnlinePlaySubScreen
    {
        public Action<PlaylistItem> Selected;

        public string ShortTitle => "song selection";
        public override string Title => ShortTitle.Humanize();

        public override bool AllowEditing => false;

        [Resolved(typeof(Room), nameof(Room.Playlist))]
        protected BindableList<PlaylistItem> Playlist { get; private set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        public MatchSongSelect()
        {
            Padding = new MarginPadding { Horizontal = HORIZONTAL_OVERFLOW_PADDING };
        }

        protected override BeatmapDetailArea CreateBeatmapDetailArea() => new MatchBeatmapDetailArea
        {
            CreateNewItem = createNewItem
        };

        protected override bool OnStart()
        {
            switch (Playlist.Count)
            {
                case 0:
                    createNewItem();
                    break;

                case 1:
                    populateItemFromCurrent(Playlist.Single());
                    break;
            }

            this.Exit();

            return true;
        }

        private void createNewItem()
        {
            PlaylistItem item = new PlaylistItem
            {
                ID = Playlist.Count == 0 ? 0 : Playlist.Max(p => p.ID) + 1
            };

            populateItemFromCurrent(item);

            Playlist.Add(item);
        }

        private void populateItemFromCurrent(PlaylistItem item)
        {
            item.Beatmap.Value = Beatmap.Value.BeatmapInfo;
            item.Ruleset.Value = Ruleset.Value;

            item.RequiredMods.Clear();
            item.RequiredMods.AddRange(Mods.Value.Select(m => m.CreateCopy()));
        }
    }
}

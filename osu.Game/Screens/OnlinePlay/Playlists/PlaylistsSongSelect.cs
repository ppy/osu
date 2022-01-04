// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Screens;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.Select;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public class PlaylistsSongSelect : OnlinePlaySongSelect
    {
        public PlaylistsSongSelect(Room room)
            : base(room)
        {
        }

        protected override BeatmapDetailArea CreateBeatmapDetailArea() => new MatchBeatmapDetailArea
        {
            CreateNewItem = createNewItem
        };

        protected override void SelectItem(PlaylistItem item)
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
            item.RequiredMods.AddRange(Mods.Value.Select(m => m.DeepClone()));

            item.AllowedMods.Clear();
            item.AllowedMods.AddRange(FreeMods.Value.Select(m => m.DeepClone()));
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Screens;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.Select;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public partial class PlaylistsSongSelect : OnlinePlaySongSelect
    {
        private readonly Room room;

        public PlaylistsSongSelect(Room room)
            : base(room)
        {
            this.room = room;
        }

        protected override BeatmapDetailArea CreateBeatmapDetailArea() => new MatchBeatmapDetailArea(room)
        {
            CreateNewItem = () => room.Playlist = room.Playlist.Append(createNewItem()).ToArray()
        };

        protected override bool SelectItem(PlaylistItem item)
        {
            if (room.Playlist.Count <= 1)
                room.Playlist = [createNewItem()];

            this.Exit();
            return true;
        }

        private PlaylistItem createNewItem() => new PlaylistItem(Beatmap.Value.BeatmapInfo)
        {
            ID = room.Playlist.Count == 0 ? 0 : room.Playlist.Max(p => p.ID) + 1,
            RulesetID = Ruleset.Value.OnlineID,
            RequiredMods = Mods.Value.Select(m => new APIMod(m)).ToArray(),
            AllowedMods = FreeMods.Value.Select(m => new APIMod(m)).ToArray()
        };
    }
}

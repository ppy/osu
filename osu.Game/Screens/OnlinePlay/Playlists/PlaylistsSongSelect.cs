// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.Select;
using osu.Game.Utils;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public partial class PlaylistsSongSelect : OnlinePlaySongSelect
    {
        [Resolved]
        private INotificationOverlay? notificationOverlay { get; set; }

        public PlaylistsSongSelect(Room room)
            : base(room)
        {
        }

        protected override BeatmapDetailArea CreateBeatmapDetailArea() => new MatchBeatmapDetailArea
        {
            CreateNewItem = createNewItem
        };

        protected override bool SelectItem(PlaylistItem item)
        {
            switch (Playlist.Count)
            {
                case 0:
                    createNewItem();
                    break;

                case 1:
                    Playlist.Clear();
                    createNewItem();
                    break;
            }

            this.Exit();
            return true;
        }

        private void createNewItem()
        {
            var validMods = Mods.Value.ToArray();

            if (ModUtils.RemoveRedundantMods(validMods, out var removed, Beatmap.Value.Beatmap))
            {
                validMods = validMods.Except(removed).ToArray();

                string[] removedModsNames = new string[removed.Count];
                for (int i = 0; i < removed.Count; i++)
                {
                    removedModsNames[i] = removed[i].Name;
                }

                notificationOverlay?.Post(new RedundantModsNotification(removedModsNames));
            }

            Mods.Value = validMods;

            PlaylistItem item = new PlaylistItem(Beatmap.Value.BeatmapInfo)
            {
                ID = Playlist.Count == 0 ? 0 : Playlist.Max(p => p.ID) + 1,
                RulesetID = Ruleset.Value.OnlineID,
                RequiredMods = Mods.Value.Select(m => new APIMod(m)).ToArray(),
                AllowedMods = FreeMods.Value.Select(m => new APIMod(m)).ToArray()
            };

            Playlist.Add(item);
        }
    }
}

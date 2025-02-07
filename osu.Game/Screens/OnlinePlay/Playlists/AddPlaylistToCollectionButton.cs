// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Database;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using Realms;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public partial class AddPlaylistToCollectionButton : RoundedButton
    {
        private readonly Room room;

        [Resolved]
        private RealmAccess realmAccess { get; set; } = null!;

        [Resolved(canBeNull: true)]
        private INotificationOverlay? notifications { get; set; }

        public AddPlaylistToCollectionButton(Room room)
        {
            this.room = room;
            Text = "Add Maps to Collection";
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Action = () =>
            {
                int[] ids = room.Playlist.Select(item => item.Beatmap.OnlineID).Where(onlineId => onlineId > 0).ToArray();

                if (ids.Length == 0)
                {
                    notifications?.Post(new SimpleErrorNotification { Text = "Cannot add local beatmaps" });
                    return;
                }

                string filter = string.Join(" OR ", ids.Select(id => $"(OnlineID == {id})"));
                var beatmaps = realmAccess.Realm.All<BeatmapInfo>().Filter(filter).ToList();

                var collection = realmAccess.Realm.All<BeatmapCollection>().FirstOrDefault(c => c.Name == room.Name);

                if (collection == null)
                {
                    collection = new BeatmapCollection(room.Name, beatmaps.Select(i => i.MD5Hash).Distinct().ToList());
                    realmAccess.Realm.Write(() => realmAccess.Realm.Add(collection));
                    notifications?.Post(new SimpleNotification { Text = $"Created new collection: {room.Name}" });
                }
                else
                {
                    collection.ToLive(realmAccess).PerformWrite(c =>
                    {
                        beatmaps = beatmaps.Where(i => !c.BeatmapMD5Hashes.Contains(i.MD5Hash)).ToList();
                        foreach (var item in beatmaps)
                            c.BeatmapMD5Hashes.Add(item.MD5Hash);
                        notifications?.Post(new SimpleNotification { Text = $"Updated collection: {room.Name}" });
                    });
                }
            };
        }
    }
}

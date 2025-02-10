// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
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
        private readonly Bindable<int> downloadedBeatmapsCount = new Bindable<int>(0);
        private readonly Bindable<bool> collectionExists = new Bindable<bool>(false);
        private IDisposable? beatmapSubscription;
        private IDisposable? collectionSubscription;

        [Resolved]
        private RealmAccess realmAccess { get; set; } = null!;

        [Resolved(canBeNull: true)]
        private INotificationOverlay? notifications { get; set; }

        public AddPlaylistToCollectionButton(Room room)
        {
            this.room = room;
            Text = formatButtonText(downloadedBeatmapsCount.Value, collectionExists.Value);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Action = () =>
            {
                if (room.Playlist.Count == 0)
                {
                    notifications?.Post(new SimpleErrorNotification { Text = "Cannot add local beatmaps" });
                    return;
                }

                var beatmaps = realmAccess.Realm.All<BeatmapInfo>().Filter(formatFilterQuery(room.Playlist)).ToList();

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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmapSubscription = realmAccess.RegisterForNotifications(r => r.All<BeatmapInfo>().Filter(formatFilterQuery(room.Playlist)), (sender, _) => downloadedBeatmapsCount.Value = sender.Count);

            collectionSubscription = realmAccess.RegisterForNotifications(r => r.All<BeatmapCollection>().Where(c => c.Name == room.Name), (sender, _) => collectionExists.Value = sender.Count > 0);

            downloadedBeatmapsCount.BindValueChanged(_ => Text = formatButtonText(downloadedBeatmapsCount.Value, collectionExists.Value));

            collectionExists.BindValueChanged(_ => Text = formatButtonText(downloadedBeatmapsCount.Value, collectionExists.Value), true);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            beatmapSubscription?.Dispose();
            collectionSubscription?.Dispose();
        }

        private string formatFilterQuery(IReadOnlyList<PlaylistItem> playlistItems) => string.Join(" OR ", playlistItems.Select(item => $"(OnlineID == {item.Beatmap.OnlineID})").Distinct());

        private string formatButtonText(int count, bool collectionExists) => $"Add {count} {(count == 1 ? "beatmap" : "beatmaps")} to {(collectionExists ? "collection" : "new collection")}";
    }
}

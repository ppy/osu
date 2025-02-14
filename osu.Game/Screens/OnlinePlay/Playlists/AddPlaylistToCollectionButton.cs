// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Localisation;
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
    public partial class AddPlaylistToCollectionButton : RoundedButton, IHasTooltip
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
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Action = () =>
            {
                if (room.Playlist.Count == 0)
                    return;

                var beatmaps = getBeatmapsForPlaylist(realmAccess.Realm).ToArray();

                var collection = realmAccess.Realm.All<BeatmapCollection>().FirstOrDefault(c => c.Name == room.Name);

                if (collection == null)
                {
                    collection = new BeatmapCollection(room.Name);
                    realmAccess.Realm.Write(() => realmAccess.Realm.Add(collection));
                    notifications?.Post(new SimpleNotification { Text = $"Created new collection: {room.Name}" });
                }
                else
                {
                    notifications?.Post(new SimpleNotification { Text = $"Updated collection: {room.Name}" });
                }

                collection.ToLive(realmAccess).PerformWrite(c =>
                {
                    foreach (var item in beatmaps)
                    {
                        if (!c.BeatmapMD5Hashes.Contains(item.MD5Hash))
                            c.BeatmapMD5Hashes.Add(item.MD5Hash);
                    }
                });
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (room.Playlist.Count > 0)
            {
                beatmapSubscription =
                    realmAccess.RegisterForNotifications(getBeatmapsForPlaylist, (sender, _) => downloadedBeatmapsCount.Value = sender.Count);
            }

            collectionSubscription = realmAccess.RegisterForNotifications(r => r.All<BeatmapCollection>().Where(c => c.Name == room.Name), (sender, _) => collectionExists.Value = sender.Any());

            downloadedBeatmapsCount.BindValueChanged(_ => updateButtonText());
            collectionExists.BindValueChanged(_ => updateButtonText(), true);
        }

        private IQueryable<BeatmapInfo> getBeatmapsForPlaylist(Realm r)
        {
            return r.All<BeatmapInfo>().Filter(string.Join(" OR ", room.Playlist.Select(item => $"(OnlineID == {item.Beatmap.OnlineID})").Distinct()));
        }

        private void updateButtonText()
        {
            if (!collectionExists.Value)
                Text = $"Create new collection with {downloadedBeatmapsCount.Value} beatmaps";
            else
                Text = $"Update collection with {downloadedBeatmapsCount.Value} beatmaps";
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            beatmapSubscription?.Dispose();
            collectionSubscription?.Dispose();
        }

        public LocalisableString TooltipText => "Only downloaded beatmaps will be added to the collection";
    }
}

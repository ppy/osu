// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
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

        private IDisposable? beatmapSubscription;
        private IDisposable? collectionSubscription;

        private Live<BeatmapCollection>? collection;
        private HashSet<string> localBeatmapHashes = new HashSet<string>();

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

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

                var beatmaps = getBeatmapsForPlaylist(realm.Realm).ToArray();

                int countBefore = 0;
                int countAfter = 0;

                collection ??= realm.Realm.Write(() => realm.Realm.Add(new BeatmapCollection(room.Name)).ToLive(realm));
                collection.PerformWrite(c =>
                {
                    countBefore = c.BeatmapMD5Hashes.Count;

                    foreach (var item in beatmaps)
                    {
                        if (!c.BeatmapMD5Hashes.Contains(item.MD5Hash))
                            c.BeatmapMD5Hashes.Add(item.MD5Hash);
                    }

                    countAfter = c.BeatmapMD5Hashes.Count;
                });

                if (countBefore == 0)
                    notifications?.Post(new SimpleNotification { Text = $"Created new collection \"{room.Name}\" with {countAfter} beatmaps." });
                else
                    notifications?.Post(new SimpleNotification { Text = $"Added {countAfter - countBefore} beatmaps to collection \"{room.Name}\"." });
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Enabled.Value = false;

            if (room.Playlist.Count == 0)
                return;

            beatmapSubscription = realm.RegisterForNotifications(getBeatmapsForPlaylist, (sender, _) =>
            {
                localBeatmapHashes = sender.Select(b => b.MD5Hash).ToHashSet();
                Schedule(updateButtonState);
            });

            collectionSubscription = realm.RegisterForNotifications(r => r.All<BeatmapCollection>().Where(c => c.Name == room.Name), (sender, _) =>
            {
                collection = sender.FirstOrDefault()?.ToLive(realm);
                Schedule(updateButtonState);
            });
        }

        private void updateButtonState()
        {
            int countToAdd = getCountToBeAdded();

            if (collection == null)
                Text = $"Create new collection with {countToAdd} beatmaps";
            else
                Text = $"Update collection with {countToAdd} beatmaps";

            Enabled.Value = countToAdd > 0;
        }

        private int getCountToBeAdded()
        {
            if (collection == null)
                return localBeatmapHashes.Count;

            return collection.PerformRead(c =>
            {
                int count = localBeatmapHashes.Count;

                foreach (string hash in localBeatmapHashes)
                {
                    if (c.BeatmapMD5Hashes.Contains(hash))
                        count--;
                }

                return count;
            });
        }

        private IQueryable<BeatmapInfo> getBeatmapsForPlaylist(Realm r)
        {
            return r.All<BeatmapInfo>().Filter(string.Join(" OR ", room.Playlist.Select(item => $"(OnlineID == {item.Beatmap.OnlineID})").Distinct()));
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            beatmapSubscription?.Dispose();
            collectionSubscription?.Dispose();
        }

        public LocalisableString TooltipText
        {
            get
            {
                if (Enabled.Value)
                    return string.Empty;

                int currentCollectionCount = collection?.PerformRead(c => c.BeatmapMD5Hashes.Count) ?? 0;
                if (room.Playlist.DistinctBy(i => i.Beatmap.OnlineID).Count() == currentCollectionCount)
                    return "All beatmaps have been added!";

                return "Download some beatmaps first.";
            }
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Overlays.Notifications;
using Realms;

namespace osu.Game.Collections
{
    /// <summary>
    /// Handles user-defined collections of beatmaps.
    /// </summary>
    public class CollectionManager : Component, IPostNotifications
    {
        public readonly BindableList<BeatmapCollection> Collections = new BindableList<BeatmapCollection>();

        [Resolved]
        private RealmAccess realm { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            realm.RegisterForNotifications(r => r.All<RealmBeatmapCollection>(), collectionsChanged);
        }

        private void collectionsChanged(IRealmCollection<RealmBeatmapCollection> sender, ChangeSet changes, Exception error)
        {
            // TODO: hook up with realm changes.

            if (changes == null)
            {
                foreach (var collection in sender)
                    Collections.Add(new BeatmapCollection
                    {
                        Name = { Value = collection.Name },
                        BeatmapHashes = { Value = collection.BeatmapMD5Hashes },
                    });
            }
        }

        public Action<Notification> PostNotification { protected get; set; }

        public void DeleteAll()
        {
            Collections.Clear();
            PostNotification?.Invoke(new ProgressCompletionNotification { Text = "Deleted all collections!" });
        }
    }
}

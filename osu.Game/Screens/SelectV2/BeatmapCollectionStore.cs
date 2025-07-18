// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Collections;
using osu.Game.Database;
using Realms;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapCollectionStore : Component
    {
        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        private IDisposable? realmSubscription;

        private readonly BindableList<Live<BeatmapCollection>> liveCollections = new BindableList<Live<BeatmapCollection>>();
        private readonly List<BeatmapCollection> detachedCollections = new List<BeatmapCollection>();

        /// <summary>
        /// Gets a thread-safe bound copy to the list of <see cref="BeatmapCollection"/> present in the user's database, wrapped in <see cref="Live{T}"/> objects.
        /// </summary>
        public BindableList<Live<BeatmapCollection>> GetLiveCollections()
        {
            lock (liveCollections)
                return liveCollections.GetBoundCopy();
        }

        /// <summary>
        /// Gets a thread-safe copy to the list of <see cref="BeatmapCollection"/> present in the user's database, in a detached state.
        /// </summary>
        /// <remarks>
        /// Note this does not guarantee validity of the list if called too early,
        /// but the current usage of this is accompanied by listening to changes in <see cref="GetLiveCollections"/>,
        /// so this is not an issue for the current usage.
        /// </remarks>
        public List<BeatmapCollection> GetDetachedCollections()
        {
            lock (detachedCollections)
                return detachedCollections.ToList();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            realmSubscription = realm.RegisterForNotifications(r => r.All<BeatmapCollection>().OrderBy(c => c.Name), onChanged);
        }

        private void onChanged(IRealmCollection<BeatmapCollection> realmCollections, ChangeSet? changes)
        {
            processLiveCollections(realmCollections, changes);
            processDetachedCollections(realmCollections, changes);
        }

        private void processLiveCollections(IRealmCollection<BeatmapCollection> realmCollections, ChangeSet? changes)
        {
            lock (liveCollections)
            {
                if (changes == null)
                {
                    liveCollections.Clear();

                    foreach (var c in realmCollections)
                        liveCollections.Add(c.ToLive(realm));
                }
                else
                {
                    foreach (int i in changes.DeletedIndices.OrderDescending())
                        liveCollections.RemoveAt(i);

                    foreach (int i in changes.InsertedIndices)
                        liveCollections.Insert(i, realmCollections[i].ToLive(realm));

                    foreach (int i in changes.NewModifiedIndices)
                        liveCollections[i] = realmCollections[i].ToLive(realm);
                }
            }
        }

        private void processDetachedCollections(IRealmCollection<BeatmapCollection> realmCollections, ChangeSet? changes)
        {
            var detached = realmCollections.Detach();

            lock (detachedCollections)
            {
                detachedCollections.Clear();
                detachedCollections.AddRange(detached);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            realmSubscription?.Dispose();
        }
    }
}

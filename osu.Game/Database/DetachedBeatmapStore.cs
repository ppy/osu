// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using Realms;

namespace osu.Game.Database
{
    // TODO: handle realm migration
    public partial class DetachedBeatmapStore : Component
    {
        private readonly ManualResetEventSlim loaded = new ManualResetEventSlim();

        private List<BeatmapSetInfo> originalBeatmapSetsDetached = new List<BeatmapSetInfo>();

        private IDisposable? subscriptionSets;

        /// <summary>
        /// Track GUIDs of all sets in realm to allow handling deletions.
        /// </summary>
        private readonly List<Guid> realmBeatmapSets = new List<Guid>();

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        public IReadOnlyList<BeatmapSetInfo> GetDetachedBeatmaps()
        {
            if (!loaded.Wait(60000))
                Logger.Error(new TimeoutException("Beatmaps did not load in an acceptable time"), $"{nameof(DetachedBeatmapStore)} fell over");

            return originalBeatmapSetsDetached;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            subscriptionSets = realm.RegisterForNotifications(getBeatmapSets, beatmapSetsChanged);
        }

        private void beatmapSetsChanged(IRealmCollection<BeatmapSetInfo> sender, ChangeSet? changes)
        {
            if (changes == null)
            {
                if (originalBeatmapSetsDetached.Count > 0 && sender.Count == 0)
                {
                    // Usually we'd reset stuff here, but doing so triggers a silly flow which ends up deadlocking realm.
                    // Additionally, user should not be at song select when realm is blocking all operations in the first place.
                    //
                    // Note that due to the catch-up logic below, once operations are restored we will still be in a roughly
                    // correct state. The only things that this return will change is the carousel will not empty *during* the blocking
                    // operation.
                    return;
                }

                originalBeatmapSetsDetached = sender.Detach();

                realmBeatmapSets.Clear();
                realmBeatmapSets.AddRange(sender.Select(r => r.ID));

                loaded.Set();
                return;
            }

            HashSet<Guid> setsRequiringUpdate = new HashSet<Guid>();
            HashSet<Guid> setsRequiringRemoval = new HashSet<Guid>();

            foreach (int i in changes.DeletedIndices.OrderDescending())
            {
                Guid id = realmBeatmapSets[i];

                setsRequiringRemoval.Add(id);
                setsRequiringUpdate.Remove(id);

                realmBeatmapSets.RemoveAt(i);
            }

            foreach (int i in changes.InsertedIndices)
            {
                Guid id = sender[i].ID;

                setsRequiringRemoval.Remove(id);
                setsRequiringUpdate.Add(id);

                realmBeatmapSets.Insert(i, id);
            }

            foreach (int i in changes.NewModifiedIndices)
                setsRequiringUpdate.Add(sender[i].ID);

            // deletions
            foreach (Guid g in setsRequiringRemoval)
                originalBeatmapSetsDetached.RemoveAll(set => set.ID == g);

            // updates
            foreach (Guid g in setsRequiringUpdate)
            {
                originalBeatmapSetsDetached.RemoveAll(set => set.ID == g);
                originalBeatmapSetsDetached.Add(fetchFromID(g)!);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            subscriptionSets?.Dispose();
        }

        private IQueryable<BeatmapSetInfo> getBeatmapSets(Realm realm) => realm.All<BeatmapSetInfo>().Where(s => !s.DeletePending && !s.Protected);
    }
}

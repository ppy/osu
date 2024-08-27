// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using Realms;

namespace osu.Game.Database
{
    public partial class DetachedBeatmapStore : Component
    {
        private readonly ManualResetEventSlim loaded = new ManualResetEventSlim();

        private readonly BindableList<BeatmapSetInfo> detachedBeatmapSets = new BindableList<BeatmapSetInfo>();

        private IDisposable? realmSubscription;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        public IBindableList<BeatmapSetInfo> GetDetachedBeatmaps()
        {
            if (!loaded.Wait(60000))
                Logger.Error(new TimeoutException("Beatmaps did not load in an acceptable time"), $"{nameof(DetachedBeatmapStore)} fell over");

            return detachedBeatmapSets.GetBoundCopy();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            realmSubscription = realm.RegisterForNotifications(getBeatmapSets, beatmapSetsChanged);
        }

        private void beatmapSetsChanged(IRealmCollection<BeatmapSetInfo> sender, ChangeSet? changes)
        {
            if (changes == null)
            {
                if (detachedBeatmapSets.Count > 0 && sender.Count == 0)
                {
                    // Usually we'd reset stuff here, but doing so triggers a silly flow which ends up deadlocking realm.
                    // Additionally, user should not be at song select when realm is blocking all operations in the first place.
                    //
                    // Note that due to the catch-up logic below, once operations are restored we will still be in a roughly
                    // correct state. The only things that this return will change is the carousel will not empty *during* the blocking
                    // operation.
                    return;
                }

                detachedBeatmapSets.Clear();
                detachedBeatmapSets.AddRange(sender.Detach());

                loaded.Set();
                return;
            }

            foreach (int i in changes.DeletedIndices.OrderDescending())
                detachedBeatmapSets.RemoveAt(i);

            foreach (int i in changes.InsertedIndices)
            {
                detachedBeatmapSets.Insert(i, sender[i].Detach());
            }

            foreach (int i in changes.NewModifiedIndices)
                detachedBeatmapSets.ReplaceRange(i, 1, new[] { sender[i].Detach() });
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            realmSubscription?.Dispose();
        }

        private IQueryable<BeatmapSetInfo> getBeatmapSets(Realm realm) => realm.All<BeatmapSetInfo>().Where(s => !s.DeletePending && !s.Protected);
    }
}

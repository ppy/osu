// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
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

        public IBindableList<BeatmapSetInfo> GetDetachedBeatmaps(CancellationToken cancellationToken)
        {
            loaded.Wait(cancellationToken);
            return detachedBeatmapSets.GetBoundCopy();
        }

        [BackgroundDependencyLoader]
        private void load(CancellationToken cancellationToken)
        {
            realmSubscription = realm.RegisterForNotifications(r => r.All<BeatmapSetInfo>().Where(s => !s.DeletePending && !s.Protected), beatmapSetsChanged);
            loaded.Wait(cancellationToken);
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

                // Detaching beatmaps takes some time, so let's make sure it doesn't run on the update thread.
                var frozenSets = sender.Freeze();

                Task.Factory.StartNew(() =>
                {
                    realm.Run(_ =>
                    {
                        var detached = frozenSets.Detach();

                        detachedBeatmapSets.Clear();
                        detachedBeatmapSets.AddRange(detached);
                        loaded.Set();
                    });
                }, TaskCreationOptions.LongRunning);

                return;
            }

            foreach (int i in changes.DeletedIndices.OrderDescending())
                removeAt(i);

            foreach (int i in changes.InsertedIndices)
                insert(sender[i].Detach(), i);

            foreach (int i in changes.NewModifiedIndices)
                replaceRange(sender[i].Detach(), i);
        }

        private void replaceRange(BeatmapSetInfo set, int i)
        {
            if (loaded.IsSet)
                detachedBeatmapSets.ReplaceRange(i, 1, new[] { set });
            else
                Schedule(() => { detachedBeatmapSets.ReplaceRange(i, 1, new[] { set }); });
        }

        private void insert(BeatmapSetInfo set, int i)
        {
            if (loaded.IsSet)
                detachedBeatmapSets.Insert(i, set);
            else
                Schedule(() => { detachedBeatmapSets.Insert(i, set); });
        }

        private void removeAt(int i)
        {
            if (loaded.IsSet)
                detachedBeatmapSets.RemoveAt(i);
            else
                Schedule(() => { detachedBeatmapSets.RemoveAt(i); });
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            loaded.Set();
            realmSubscription?.Dispose();
        }
    }
}

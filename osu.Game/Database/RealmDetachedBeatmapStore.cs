// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using Realms;

namespace osu.Game.Database
{
    public partial class RealmDetachedBeatmapStore : BeatmapStore
    {
        private readonly ManualResetEventSlim loaded = new ManualResetEventSlim();

        private readonly BindableList<BeatmapSetInfo> detachedBeatmapSets = new BindableList<BeatmapSetInfo>();

        private IDisposable? realmSubscription;

        private readonly Queue<OperationArgs> pendingOperations = new Queue<OperationArgs>();

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        public override IBindableList<BeatmapSetInfo> GetBeatmapSets(CancellationToken? cancellationToken)
        {
            loaded.Wait(cancellationToken ?? CancellationToken.None);
            lock (detachedBeatmapSets)
                return detachedBeatmapSets.GetBoundCopy();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            realmSubscription = realm.RegisterForNotifications(r => r.All<BeatmapSetInfo>().Where(s => !s.DeletePending && !s.Protected), beatmapSetsChanged);
        }

        private void beatmapSetsChanged(IRealmCollection<BeatmapSetInfo> sender, ChangeSet? changes)
        {
            if (changes == null)
            {
                if (sender is RealmResetEmptySet<BeatmapSetInfo>)
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
                    try
                    {
                        realm.Run(_ =>
                        {
                            var detached = frozenSets.Detach();

                            lock (detachedBeatmapSets)
                            {
                                detachedBeatmapSets.Clear();
                                detachedBeatmapSets.AddRange(detached);
                            }
                        });
                    }
                    finally
                    {
                        loaded.Set();
                    }
                }, TaskCreationOptions.LongRunning).FireAndForget();

                return;
            }

            foreach (int i in changes.DeletedIndices.OrderDescending())
            {
                pendingOperations.Enqueue(new OperationArgs
                {
                    Type = OperationType.Remove,
                    Index = i,
                });
            }

            foreach (int i in changes.InsertedIndices)
            {
                pendingOperations.Enqueue(new OperationArgs
                {
                    Type = OperationType.Insert,
                    BeatmapSet = sender[i].Detach(),
                    Index = i,
                });
            }

            foreach (int i in changes.NewModifiedIndices)
            {
                pendingOperations.Enqueue(new OperationArgs
                {
                    Type = OperationType.Update,
                    BeatmapSet = sender[i].Detach(),
                    Index = i,
                });
            }
        }

        protected override void Update()
        {
            base.Update();

            // We can't start processing operations until we have finished detaching the initial list.
            if (!loaded.IsSet)
                return;

            if (pendingOperations.Count == 0)
                return;

            lock (detachedBeatmapSets)
            {
                // If this ever leads to performance issues, we could dequeue a limited number of operations per update frame.
                while (pendingOperations.TryDequeue(out var op))
                {
                    switch (op.Type)
                    {
                        case OperationType.Insert:
                            detachedBeatmapSets.Insert(op.Index, op.BeatmapSet!);
                            break;

                        case OperationType.Update:
                            detachedBeatmapSets.ReplaceRange(op.Index, 1, new[] { op.BeatmapSet! });
                            break;

                        case OperationType.Remove:
                            detachedBeatmapSets.RemoveAt(op.Index);
                            break;
                    }
                }
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            loaded.Set();
            loaded.Dispose();
            realmSubscription?.Dispose();
        }

        private record OperationArgs
        {
            public OperationType Type;
            public BeatmapSetInfo? BeatmapSet;
            public int Index;
        }

        private enum OperationType
        {
            Insert,
            Update,
            Remove
        }
    }
}

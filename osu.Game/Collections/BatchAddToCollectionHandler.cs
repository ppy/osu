// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Collections
{
    internal static partial class BatchAddToCollectionHandler
    {
        public enum BatchAddOperation
        {
            Added,
            Removed,
        }

        public readonly record struct BatchAddResult(Guid CollectionId, BatchAddOperation Operation, int Count);

        public static event Action<BatchAddResult>? OperationCompleted;

        private static readonly Dictionary<Guid, (BatchAddResult Result, DateTimeOffset Timestamp)> recent_results = new Dictionary<Guid, (BatchAddResult Result, DateTimeOffset Timestamp)>();
        private static readonly object recent_results_lock = new object();
        private static readonly TimeSpan recent_result_lifetime = TimeSpan.FromSeconds(6);

        public static bool TryGetRecentResult(Guid collectionId, out BatchAddResult result)
        {
            lock (recent_results_lock)
            {
                cleanupExpiredResults();

                if (recent_results.TryGetValue(collectionId, out var entry))
                {
                    result = entry.Result;
                    return true;
                }
            }

            result = default;
            return false;
        }

        public static void RequestSaveToCollection(
            Live<BeatmapCollection> collection,
            Func<IEnumerable<BeatmapInfo>>? filteredBeatmapsProvider,
            Action<PopupDialog> showDialog)
        {
            if (filteredBeatmapsProvider == null)
                return;

            var hashes = filteredBeatmapsProvider().Select(b => b.MD5Hash)
                                                   .Where(h => !string.IsNullOrEmpty(h))
                                                   .Distinct()
                                                   .ToList();

            if (hashes.Count == 0)
                return;

            var existing = collection.PerformRead(c => c.BeatmapMD5Hashes.ToList());
            var intersection = existing.Intersect(hashes).ToList();
            int overlapCount = intersection.Count;

            if (overlapCount == hashes.Count)
            {
                showDialog(new RemoveFilteredResultsDialog(
                    onRemove: () => runHashRemoval(collection, intersection, BatchAddOperation.Removed)));
                return;
            }

            if (overlapCount > 0)
            {
                var toAdd = hashes.Except(existing).ToList();
                var toRemove = intersection;

                showDialog(new PartialOverlapFilteredResultsDialog(
                    overlapCount,
                    onAddDifference: () => runHashAddition(collection, toAdd, BatchAddOperation.Added),
                    onRemoveIntersection: () => runHashRemoval(collection, toRemove, BatchAddOperation.Removed)));
                return;
            }

            string collectionName = collection.PerformRead(c => c.Name);

            showDialog(new AddFilteredResultsDialog(
                collectionName,
                hashes.Count,
                onAddAll: () => runHashAddition(collection, hashes, BatchAddOperation.Added)));
        }

        private static void runHashAddition(Live<BeatmapCollection> collection, IReadOnlyList<string> hashes, BatchAddOperation operation)
        {
            if (hashes.Count == 0)
                return;

            Task.Run(() => collection.PerformWrite(c =>
            {
                int affected = 0;

                foreach (string hash in hashes)
                {
                    if (c.BeatmapMD5Hashes.Contains(hash))
                        continue;

                    c.BeatmapMD5Hashes.Add(hash);
                    affected++;
                }

                if (affected > 0)
                    notifyOperationCompleted(c.ID, operation, affected);
            }));
        }

        private static void runHashRemoval(Live<BeatmapCollection> collection, IReadOnlyList<string> hashes, BatchAddOperation operation)
        {
            Task.Run(() => collection.PerformWrite(c =>
            {
                int affected = 0;

                foreach (string hash in hashes)
                {
                    if (c.BeatmapMD5Hashes.Remove(hash))
                        affected++;
                }

                if (affected > 0)
                    notifyOperationCompleted(c.ID, operation, affected);
            }));
        }

        private static void notifyOperationCompleted(Guid collectionId, BatchAddOperation operation, int count)
        {
            var result = new BatchAddResult(collectionId, operation, count);

            lock (recent_results_lock)
            {
                recent_results[collectionId] = (result, DateTimeOffset.UtcNow);
                cleanupExpiredResults();
            }

            OperationCompleted?.Invoke(result);
        }

        private static void cleanupExpiredResults()
        {
            var now = DateTimeOffset.UtcNow;

            foreach (var entry in recent_results.ToArray())
            {
                if (now - entry.Value.Timestamp > recent_result_lifetime)
                    recent_results.Remove(entry.Key);
            }
        }

        private partial class AddFilteredResultsDialog : DangerousActionDialog
        {
            public AddFilteredResultsDialog(string collectionName, int beatmapCount, Action onAddAll)
            {
                Icon = FontAwesome.Solid.Check;
                HeaderText = "Add all visible beatmaps to collection";
                BodyText = $"Add {beatmapCount:#,0} beatmaps to \"{collectionName}\"?";
                DangerousAction = onAddAll;
            }
        }

        private partial class RemoveFilteredResultsDialog : DangerousActionDialog
        {
            public RemoveFilteredResultsDialog(Action onRemove)
            {
                Icon = FontAwesome.Solid.Trash;
                HeaderText = "Remove all visible beatmaps from collection";
                BodyText = "The collection already contains all the visible beatmaps, do you want to remove these beatmaps from the collection?";
                DangerousAction = onRemove;
            }
        }

        private partial class PartialOverlapFilteredResultsDialog : DangerousActionDialog
        {
            public PartialOverlapFilteredResultsDialog(int overlapCount, Action onAddDifference, Action onRemoveIntersection)
            {
                Icon = FontAwesome.Solid.Question;
                HeaderText = "The visible beatmaps is partially overlapped with the collection";
                BodyText = $"{overlapCount} beatmaps already exist. Please select the action to perform:";
                Buttons = new PopupDialogButton[]
                {
                    new PopupDialogDangerousButton
                    {
                        Text = "Add difference",
                        Action = onAddDifference,
                    },
                    new PopupDialogDangerousButton
                    {
                        Text = "Remove intersection",
                        Action = onRemoveIntersection,
                    },
                    new PopupDialogCancelButton
                    {
                        Text = "Cancel"
                    }
                };
            }
        }
    }
}

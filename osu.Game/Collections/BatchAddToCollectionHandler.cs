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
                    onRemove: () => runHashRemoval(collection, intersection)));
                return;
            }

            if (overlapCount > 0)
            {
                var toAdd = hashes.Except(existing).ToList();
                var toRemove = intersection;

                showDialog(new PartialOverlapFilteredResultsDialog(
                    overlapCount,
                    onAddDifference: () => runHashAddition(collection, toAdd),
                    onRemoveIntersection: () => runHashRemoval(collection, toRemove)));
                return;
            }

            string collectionName = collection.PerformRead(c => c.Name);

            showDialog(new AddFilteredResultsDialog(
                collectionName,
                hashes.Count,
                onAddAll: () => runHashAddition(collection, hashes)));
        }

        private static void runHashAddition(Live<BeatmapCollection> collection, IReadOnlyList<string> hashes)
        {
            if (hashes.Count == 0)
                return;

            Task.Run(() => collection.PerformWrite(c =>
            {
                foreach (string hash in hashes)
                {
                    if (!c.BeatmapMD5Hashes.Contains(hash))
                        c.BeatmapMD5Hashes.Add(hash);
                }
            }));
        }

        private static void runHashRemoval(Live<BeatmapCollection> collection, IReadOnlyList<string> hashes)
        {
            Task.Run(() => collection.PerformWrite(c =>
            {
                foreach (string hash in hashes)
                    c.BeatmapMD5Hashes.Remove(hash);
            }));
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

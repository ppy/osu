// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Mania.Objects;

namespace osu.Game.Rulesets.Mania.Difficulty.Preprocessing
{
    /// <summary>
    /// Data processing class that makes it easy to traverse the whole map based on note type and column.
    /// </summary>
    public class ManiaMapState
    {
        private readonly List<ManiaDifficultyHitObject> heads = new List<ManiaDifficultyHitObject>();
        private readonly List<ManiaDifficultyHitObject> tails = new List<ManiaDifficultyHitObject>();
        private readonly List<ManiaDifficultyHitObject>[] columnHeads;
        private readonly List<ManiaDifficultyHitObject>[] columnTails;

        public int TotalColumns { get; }
        public int HeadCount => heads.Count;
        public int TailCount => tails.Count;

        public ManiaMapState(int totalColumns)
        {
            TotalColumns = totalColumns;
            columnHeads = new List<ManiaDifficultyHitObject>[totalColumns];
            columnTails = new List<ManiaDifficultyHitObject>[totalColumns];

            for (int i = 0; i < totalColumns; i++)
            {
                columnHeads[i] = new List<ManiaDifficultyHitObject>();
                columnTails[i] = new List<ManiaDifficultyHitObject>();
            }
        }

        public void Add(ManiaDifficultyHitObject obj)
        {
            if (obj.BaseObject is TailNote)
            {
                tails.Add(obj);
                columnTails[obj.Column].Add(obj);
            }
            else
            {
                heads.Add(obj);
                columnHeads[obj.Column].Add(obj);
            }
        }

        public int ColumnHeadCount(int column) => columnHeads[column].Count;
        public int ColumnTailCount(int column) => columnTails[column].Count;

        public ManiaDifficultyHitObject? GetHead(int index) => GetByIndex(heads, index);
        public ManiaDifficultyHitObject? GetTail(int index) => GetByIndex(tails, index);
        public ManiaDifficultyHitObject? GetColumnHead(int column, int index) => GetByIndex(columnHeads[column], index);
        public ManiaDifficultyHitObject? GetColumnTail(int column, int index) => GetByIndex(columnTails[column], index);

        public ManiaDifficultyHitObject? SearchColumnHead(int column, double time, int offset, bool inclusive, bool backward)
            => searchColumn(columnHeads, column, time, offset, inclusive, backward);

        public ManiaDifficultyHitObject? SearchColumnTail(int column, double time, int offset, bool inclusive, bool backward)
            => searchColumn(columnTails, column, time, offset, inclusive, backward);

        private ManiaDifficultyHitObject? searchColumn(List<ManiaDifficultyHitObject>[] lists, int column, double time, int offset, bool inclusive, bool backward)
        {
            if (column < 0 || column >= TotalColumns)
                return null;

            var list = lists[column];

            int foundIndex = binarySearch(list, time);

            // Whether the found note matches the time of this note exactly.
            bool exactMatch = foundIndex < list.Count && list[foundIndex].ActualTime == time;

            if (backward)
            {
                // foundIndex points to the first note at or after `time`.
                // Stepping back one gives us the last note strictly before `time`,
                // which is the correct base unless we're inclusive and landed exactly on `time`.
                int baseIndex = inclusive && exactMatch ? foundIndex : foundIndex - 1;
                return GetByIndex(list, baseIndex - offset);
            }
            else
            {
                // foundIndex points to the first note at or after `time`, which is already correct.
                // The only adjustment needed is stepping forward one when we're non-inclusive
                // and landed exactly on `time`.
                int baseIndex = !inclusive && exactMatch ? foundIndex + 1 : foundIndex;
                return GetByIndex(list, baseIndex + offset);
            }
        }

        public static ManiaDifficultyHitObject? GetByIndex(List<ManiaDifficultyHitObject> list, int index)
            => index >= 0 && index < list.Count ? list[index] : null;

        /// <summary>
        /// Returns the index of the first note at or after <paramref name="time"/>.
        /// If no such note exists, returns <see cref="List{T}.Count"/>.
        /// </summary>
        private static int binarySearch(List<ManiaDifficultyHitObject> list, double time)
        {
            int lo = 0, hi = list.Count;

            while (lo < hi)
            {
                int mid = (lo + hi) >> 1;
                if (list[mid].ActualTime < time) lo = mid + 1;
                else hi = mid;
            }

            return lo;
        }
    }
}

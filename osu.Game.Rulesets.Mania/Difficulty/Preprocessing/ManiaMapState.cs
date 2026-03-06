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
            int found = list.BinarySearch(null!, Comparer<ManiaDifficultyHitObject>.Create((x, _) => x.ActualTime.CompareTo(time)));

            if (found < 0) found = ~found;

            if (backward)
            {
                int baseIndex = inclusive && found < list.Count && list[found].ActualTime == time ? found : found - 1;
                return GetByIndex(list, baseIndex - offset);
            }
            else
            {
                int baseIndex = !inclusive && found < list.Count && list[found].ActualTime == time ? found + 1 : found;
                return GetByIndex(list, baseIndex + offset);
            }
        }

        public static ManiaDifficultyHitObject? GetByIndex(List<ManiaDifficultyHitObject> list, int index)
            => index >= 0 && index < list.Count ? list[index] : null;
    }
}

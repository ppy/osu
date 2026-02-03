// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Mania.Difficulty.Preprocessing
{
    public class ManiaDifficultyHitObject : DifficultyHitObject
    {
        public ManiaDifficultyHitObject? Head { get; private set; }
        public ManiaDifficultyHitObject? Tail { get; private set; }
        public bool IsHold => Tail is not null;

        public readonly double ActualTime;
        public new double StartTime { get; }
        public new double EndTime { get; private set; }

        /// <summary>
        /// The time difference to the last processed head note in any other column.
        /// </summary>
        public readonly double HeadDeltaTime;

        public new ManiaHitObject BaseObject => (ManiaHitObject)base.BaseObject;

        public readonly int Column;

        // Indices of the next notes
        private readonly int nextHeadObjectIndex;
        private readonly int nextTailObjectIndex;
        private readonly int columnNextHeadIndex;
        private readonly int columnNextTailIndex;

        // Indices of the previous notes
        private readonly int prevHeadObjectIndex;
        private readonly int prevTailObjectIndex;
        private readonly int columnPrevHeadIndex;
        private readonly int columnPrevTailIndex;

        // Lists of head and tail objects to make object-type specific traversal easier.
        private readonly List<ManiaDifficultyHitObject> headObjects;
        private readonly List<ManiaDifficultyHitObject> tailObjects;
        private readonly List<ManiaDifficultyHitObject>[] perColumnHeadObjects;
        private readonly List<ManiaDifficultyHitObject>[] perColumnTailObjects;

        /// <summary>
        /// The hit object earlier in time than this note in each column.
        /// </summary>
        public readonly ManiaDifficultyHitObject?[] PreviousHeadObjects;

        public readonly double ColumnHeadStrainTime;

        public ManiaDifficultyHitObject(HitObject hitObject, HitObject lastObject, double clockRate, List<DifficultyHitObject> objects,
                                        List<ManiaDifficultyHitObject> headObjects, List<ManiaDifficultyHitObject> tailObjects,
                                        List<ManiaDifficultyHitObject>[] perColumnHeadObjects, List<ManiaDifficultyHitObject>[] perColumnTailObjects, int index)
            : base(hitObject, lastObject, clockRate, objects, index)
        {
            int totalColumns = perColumnHeadObjects.Length;
            this.headObjects = headObjects;
            this.tailObjects = tailObjects;
            this.perColumnHeadObjects = perColumnHeadObjects;
            this.perColumnTailObjects = perColumnTailObjects;
            Column = BaseObject.Column;
            PreviousHeadObjects = new ManiaDifficultyHitObject[totalColumns];

            // Actual time is when the nested hit object takes place
            ActualTime = base.StartTime;

            int headObjectIndex = headObjects.Count;
            int tailObjectIndex = tailObjects.Count;
            int columnHeadIndex = perColumnHeadObjects[Column].Count;
            int columnTailIndex = perColumnTailObjects[Column].Count;

            prevHeadObjectIndex = headObjectIndex - 1;
            prevTailObjectIndex = tailObjectIndex - 1;
            columnPrevHeadIndex = columnHeadIndex - 1;
            columnPrevTailIndex = columnTailIndex - 1;

            // Add a reference to the related head/tail for long notes.
            if (BaseObject is TailNote)
            {
                Tail = this;

                // We process forward, so we need to set the tail value for the previous head while we process the tail for it.
                Head = perColumnHeadObjects[Column].LastOrDefault();

                if (Head is not null)
                {
                    Head.Tail = this;
                    Head.EndTime = Tail.ActualTime;
                }

                // We need to separate behaviour for future note indexing for heads and tails, since a tail's head index points to the next head, not itself.
                nextHeadObjectIndex = headObjectIndex;
                nextTailObjectIndex = tailObjectIndex + 1;
                columnNextHeadIndex = columnHeadIndex;
                columnNextTailIndex = columnTailIndex + 1;
            }
            else
            {
                Head = this;

                // Same for heads
                nextHeadObjectIndex = headObjectIndex + 1;
                nextTailObjectIndex = tailObjectIndex;
                columnNextHeadIndex = columnHeadIndex + 1;
                columnNextTailIndex = columnTailIndex;
            }

            StartTime = Head?.ActualTime ?? ActualTime;
            EndTime = ActualTime; // For LNs, we go back and populate this when we process the tail.

            HeadDeltaTime = StartTime - PrevHead(0)?.StartTime ?? StartTime;
            ColumnHeadStrainTime = StartTime - PrevHeadInColumn(0)?.StartTime ?? StartTime;

            for (int i = 0; i < perColumnHeadObjects.Length; i++)
            {
                ManiaDifficultyHitObject? columnObject = perColumnHeadObjects[i].LastOrDefault();

                if (columnObject is not null)
                {
                    // Get the last object before this time in each column.
                    PreviousHeadObjects[i] = columnObject.StartTime == StartTime ? columnObject.PrevHeadInColumn(0) : columnObject;
                }
            }

            ManiaDifficultyHitObject? prevHeadObj = PrevHead(0);

            if (prevHeadObj is not null)
            {
                for (int i = 0; i < prevHeadObj.PreviousHeadObjects.Length; i++)
                    PreviousHeadObjects[i] = prevHeadObj.PreviousHeadObjects[i];

                // intentionally depends on processing order to match live.
                PreviousHeadObjects[prevHeadObj.Column] = prevHeadObj;
            }
        }

        public ManiaDifficultyHitObject? PrevHead(int backwardsIndex) => getNoteByIndex(headObjects, prevHeadObjectIndex - backwardsIndex);
        public ManiaDifficultyHitObject? NextHead(int forwardsIndex) => getNoteByIndex(headObjects, nextHeadObjectIndex + forwardsIndex);

        public ManiaDifficultyHitObject? PrevTail(int backwardsIndex) => getNoteByIndex(tailObjects, prevTailObjectIndex - backwardsIndex);
        public ManiaDifficultyHitObject? NextTail(int forwardsIndex) => getNoteByIndex(tailObjects, nextTailObjectIndex + forwardsIndex);

        public ManiaDifficultyHitObject? PrevHeadInColumn(int backwardsIndex, int? column = null, bool inclusive = false)
        {
            if (column is null || column == Column)
            {
                return getNoteByIndex(perColumnHeadObjects[Column], columnPrevHeadIndex - backwardsIndex);
            }

            return getNoteInColumn(perColumnHeadObjects, column.Value, backwardsIndex, inclusive, true);
        }

        public ManiaDifficultyHitObject? NextHeadInColumn(int forwardsIndex, int? column = null, bool inclusive = false)
        {
            if (column is null || column == Column)
            {
                return getNoteByIndex(perColumnHeadObjects[Column], columnNextHeadIndex + forwardsIndex);
            }

            return getNoteInColumn(perColumnHeadObjects, column.Value, forwardsIndex, inclusive, false);
        }

        public ManiaDifficultyHitObject? PrevTailInColumn(int backwardsIndex, int? column = null, bool inclusive = false)
        {
            if (column is null || column == Column)
            {
                return getNoteByIndex(perColumnTailObjects[Column], columnPrevTailIndex - backwardsIndex);
            }

            return getNoteInColumn(perColumnTailObjects, column.Value, backwardsIndex, inclusive, true);
        }

        public ManiaDifficultyHitObject? NextTailInColumn(int forwardsIndex, int? column = null, bool inclusive = false)
        {
            if (column is null || column == Column)
            {
                return getNoteByIndex(perColumnTailObjects[Column], columnNextTailIndex + forwardsIndex);
            }

            return getNoteInColumn(perColumnTailObjects, column.Value, forwardsIndex, inclusive, false);
        }

        private ManiaDifficultyHitObject? getNoteInColumn(List<ManiaDifficultyHitObject>[] columnLists, int column, int offset, bool inclusive, bool backward)
        {
            if (column < 0 || column >= columnLists.Length)
                return null;

            List<ManiaDifficultyHitObject> columnList = columnLists[column];

            // The lists are ordered by ActualTime, so we can use binary search here.
            int foundIndex = columnList.BinarySearch(this, Comparer<ManiaDifficultyHitObject>.Create((x, y) => x.ActualTime.CompareTo(y.ActualTime)));

            if (foundIndex < 0) foundIndex = ~foundIndex;

            if (backward)
            {
                // Binary search index is always either the same or greater, so we subtract 1 if not inclusive.
                int baseIndex = inclusive && foundIndex < columnList.Count && columnList[foundIndex].ActualTime == ActualTime ? foundIndex : foundIndex - 1;
                return getNoteByIndex(columnList, baseIndex - offset);
            }
            else
            {
                // Binary search index is always either the same or greater, so we only add 1 if not inclusive but there is a note at the same time.
                int baseIndex = !inclusive && foundIndex < columnList.Count && columnList[foundIndex].ActualTime == ActualTime ? foundIndex + 1 : foundIndex;
                return getNoteByIndex(columnList, baseIndex + offset);
            }
        }

        private ManiaDifficultyHitObject? getNoteByIndex(List<ManiaDifficultyHitObject> list, int index) => index >= 0 && index < list.Count ? list[index] : null;
    }
}

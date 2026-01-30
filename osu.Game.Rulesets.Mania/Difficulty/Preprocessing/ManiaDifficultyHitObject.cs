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

        public new readonly double StartTime;
        public new readonly double EndTime;
        public readonly double ActualTime;

        public int? HeadIndex => Head?.headObjectIndex;
        public int? TailIndex => Tail?.headObjectIndex;

        /// <summary>
        /// The time difference to the last processed head note in any other column.
        /// </summary>
        public readonly double HeadDeltaTime;

        public new ManiaHitObject BaseObject => (ManiaHitObject)base.BaseObject;

        public readonly int Column;
        private readonly int headObjectIndex;
        private readonly int tailObjectIndex;
        private readonly int columnHeadIndex;
        private readonly int columnTailIndex;

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
            headObjectIndex = headObjects.Count;
            tailObjectIndex = tailObjects.Count;
            columnHeadIndex = perColumnHeadObjects[Column].Count;
            columnTailIndex = perColumnTailObjects[Column].Count;
            PreviousHeadObjects = new ManiaDifficultyHitObject[totalColumns];

            // Add a reference to the related head/tail for long notes.
            if (BaseObject is TailNote)
            {
                Tail = this;

                // We process forward, so we need to set the tail value for the previous head while we process the tail for it.
                Head = perColumnHeadObjects[Column].LastOrDefault();

                if (Head is not null)
                {
                    Head.Tail = this;
                }
            }
            else
            {
                Head = this;
            }

            // Actual time is when the nested hit object takes place
            ActualTime = base.StartTime;
            StartTime = Head?.ActualTime ?? ActualTime;
            EndTime = Tail?.ActualTime ?? ActualTime;

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

        public ManiaDifficultyHitObject? PrevHead(int backwardsIndex) => getNoteByIndex(headObjects, headObjectIndex - (backwardsIndex + 1));
        public ManiaDifficultyHitObject? NextHead(int forwardsIndex) => getNoteByIndex(headObjects, headObjectIndex + forwardsIndex + 1);

        public ManiaDifficultyHitObject? PrevTail(int backwardsIndex) => getNoteByIndex(tailObjects, tailObjectIndex - (backwardsIndex + 1));
        public ManiaDifficultyHitObject? NextTail(int forwardsIndex) => getNoteByIndex(tailObjects, tailObjectIndex + forwardsIndex + 1);

        public ManiaDifficultyHitObject? PrevHeadInColumn(int backwardsIndex, int? column = null, bool inclusive = false) => getRelative(perColumnHeadObjects, column, columnHeadIndex, -backwardsIndex - 1, inclusive, true);
        public ManiaDifficultyHitObject? NextHeadInColumn(int forwardsIndex, int? column = null, bool inclusive = false) => getRelative(perColumnHeadObjects, column, columnHeadIndex, forwardsIndex + 1, inclusive, false);

        public ManiaDifficultyHitObject? PrevTailInColumn(int backwardsIndex, int? column = null, bool inclusive = false) => getRelative(perColumnTailObjects, column, columnTailIndex, -backwardsIndex - 1, inclusive, true);
        public ManiaDifficultyHitObject? NextTailInColumn(int forwardsIndex, int? column = null, bool inclusive = false) => getRelative(perColumnTailObjects, column, columnTailIndex, forwardsIndex + 1, inclusive, false);

        private ManiaDifficultyHitObject? getRelative(List<ManiaDifficultyHitObject>[] perColumnLists, int? column, int currIndex, int offset, bool inclusive, bool backward)
        {
            int targetColumn = column ?? Column;
            if (targetColumn < 0 || targetColumn >= perColumnLists.Length) return null;

            var list = perColumnLists[targetColumn];

            // If we're in the same column, we know our column index already
            if (targetColumn == Column)
                return getNoteByIndex(list, currIndex + offset);

            // If we're looking in a different column, find the starting point via binary search
            int foundIndex = list.BinarySearch(this, Comparer<ManiaDifficultyHitObject>.Create((x, y) => x.StartTime.CompareTo(y.StartTime)));

            // If not found, BinarySearch returns the bitwise complement of the next larger element
            if (foundIndex < 0) foundIndex = ~foundIndex;

            // If inclusive and we found an exact match, start there. Otherwise, start before
            // We don't add 1 to the offsets here, since it returns the index of the prev object already
            if (backward)
            {
                int baseIndex = (inclusive && foundIndex < list.Count && list[foundIndex].StartTime == StartTime) ? foundIndex : foundIndex - 1;
                return getNoteByIndex(list, baseIndex + offset);
            }
            else
            {
                int baseIndex = (!inclusive && foundIndex < list.Count && list[foundIndex].StartTime == StartTime) ? foundIndex + 1 : foundIndex;
                return getNoteByIndex(list, baseIndex + offset);
            }
        }

        private ManiaDifficultyHitObject? getNoteByIndex(List<ManiaDifficultyHitObject> list, int index) => (index >= 0 && index < list.Count) ? list[index] : null;
    }
}

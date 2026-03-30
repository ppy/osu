// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
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

        private readonly int headIndex;
        private readonly int tailIndex;
        private readonly int columnHeadIndex;
        private readonly int columnTailIndex;
        private readonly bool isTail;

        private readonly ManiaMapState mapState;

        /// <summary>
        /// The hit object earlier in time than this note in each column.
        /// </summary>
        public readonly ManiaDifficultyHitObject?[] PreviousHeadObjects;

        public readonly double ColumnHeadStrainTime;

        public ManiaDifficultyHitObject(HitObject hitObject, HitObject lastObject, double clockRate,
                                        List<DifficultyHitObject> objects, ManiaMapState mapState, int index)
            : base(hitObject, lastObject, clockRate, objects, index)
        {
            this.mapState = mapState;

            Column = BaseObject.Column;
            PreviousHeadObjects = new ManiaDifficultyHitObject[mapState.TotalColumns];
            ActualTime = base.StartTime;

            // Get metrics up to this note from the current mapState, which only includes previous notes so far.
            int headCount = mapState.HeadCount;
            int tailCount = mapState.TailCount;
            int columnHeadCount = mapState.ColumnHeadCount(Column);
            int columnTailCount = mapState.ColumnTailCount(Column);

            headIndex = headCount;
            tailIndex = tailCount;
            columnHeadIndex = columnHeadCount;
            columnTailIndex = columnTailCount;
            isTail = BaseObject is TailNote;

            if (isTail)
            {
                Tail = this;
                Head = mapState.GetColumnHead(Column, columnHeadCount - 1);

                // If the first note happens to be an LN, the head won't end up being processed, leaving this null.
                if (Head is not null)
                {
                    Head.Tail = this;
                    Head.EndTime = ActualTime;
                }
            }
            else
            {
                Head = this;
            }

            StartTime = Head?.ActualTime ?? ActualTime;
            EndTime = ActualTime;

            HeadDeltaTime = StartTime - PrevHead(0)?.StartTime ?? StartTime;
            ColumnHeadStrainTime = StartTime - PrevHeadInColumn(0)?.StartTime ?? StartTime;

            for (int i = 0; i < mapState.TotalColumns; i++)
            {
                int lastColumnHeadIndex = mapState.ColumnHeadCount(i) - 1;
                ManiaDifficultyHitObject? columnObject = mapState.GetColumnHead(i, lastColumnHeadIndex);

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

        public ManiaDifficultyHitObject? PrevHead(int backwardsIndex) => mapState.GetHead(headIndex - 1 - backwardsIndex);
        public ManiaDifficultyHitObject? NextHead(int forwardsIndex) => mapState.GetHead(headIndex + (isTail ? 0 : 1) + forwardsIndex);

        public ManiaDifficultyHitObject? PrevTail(int backwardsIndex) => mapState.GetTail(tailIndex - 1 - backwardsIndex);
        public ManiaDifficultyHitObject? NextTail(int forwardsIndex) => mapState.GetTail(tailIndex + (isTail ? 1 : 0) + forwardsIndex);

        public ManiaDifficultyHitObject? PrevHeadInColumn(int backwardsIndex, int? otherColumn = null, bool inclusive = false)
        {
            if (otherColumn is null || otherColumn == Column)
                return mapState.GetColumnHead(Column, columnHeadIndex - 1 - backwardsIndex);

            return mapState.SearchColumnHead(otherColumn.Value, ActualTime, backwardsIndex, inclusive, backward: true);
        }

        public ManiaDifficultyHitObject? NextHeadInColumn(int forwardsIndex, int? otherColumn = null, bool inclusive = false)
        {
            if (otherColumn is null || otherColumn == Column)
                return mapState.GetColumnHead(Column, columnHeadIndex + (isTail ? 0 : 1) + forwardsIndex);

            return mapState.SearchColumnHead(otherColumn.Value, ActualTime, forwardsIndex, inclusive, backward: false);
        }

        public ManiaDifficultyHitObject? PrevTailInColumn(int backwardsIndex, int? otherColumn = null, bool inclusive = false)
        {
            if (otherColumn is null || otherColumn == Column)
                return mapState.GetColumnTail(Column, columnTailIndex - 1 - backwardsIndex);

            return mapState.SearchColumnTail(otherColumn.Value, ActualTime, backwardsIndex, inclusive, backward: true);
        }

        public ManiaDifficultyHitObject? NextTailInColumn(int forwardsIndex, int? otherColumn = null, bool inclusive = false)
        {
            if (otherColumn is null || otherColumn == Column)
                return mapState.GetColumnTail(Column, columnTailIndex + (isTail ? 1 : 0) + forwardsIndex);

            return mapState.SearchColumnTail(otherColumn.Value, ActualTime, forwardsIndex, inclusive, backward: false);
        }
    }
}

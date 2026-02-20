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

        // Note indices used to determine where to start searching the note index from this note
        private readonly int prevHeadIndex;
        private readonly int nextHeadIndex;
        private readonly int prevTailIndex;
        private readonly int nextTailIndex;
        private readonly int columnPrevHeadIndex;
        private readonly int columnNextHeadIndex;
        private readonly int columnPrevTailIndex;
        private readonly int columnNextTailIndex;

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

            int headCount = mapState.HeadCount;
            int tailCount = mapState.TailCount;
            int columnHeadCount = mapState.ColumnHeadCount(Column);
            int columnTailCount = mapState.ColumnTailCount(Column);

            if (BaseObject is TailNote)
            {
                Tail = this;
                Head = mapState.GetColumnHead(Column, columnHeadCount - 1);

                if (Head is not null)
                {
                    Head.Tail = this;
                    Head.EndTime = ActualTime;
                }

                // For a tail, the "next head" index is unchanged (this isn't a head),
                // and the "next tail" index points past itself once added.
                prevHeadIndex = headCount - 1;
                nextHeadIndex = headCount;
                prevTailIndex = tailCount - 1;
                nextTailIndex = tailCount + 1;

                columnPrevHeadIndex = columnHeadCount - 1;
                columnNextHeadIndex = columnHeadCount;
                columnPrevTailIndex = columnTailCount - 1;
                columnNextTailIndex = columnTailCount + 1;
            }
            else
            {
                Head = this;

                prevHeadIndex = headCount - 1;
                nextHeadIndex = headCount + 1;
                prevTailIndex = tailCount - 1;
                nextTailIndex = tailCount;

                columnPrevHeadIndex = columnHeadCount - 1;
                columnNextHeadIndex = columnHeadCount + 1;
                columnPrevTailIndex = columnTailCount - 1;
                columnNextTailIndex = columnTailCount;
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

        public ManiaDifficultyHitObject? PrevHead(int backwardsIndex) => mapState.GetHead(prevHeadIndex - backwardsIndex);
        public ManiaDifficultyHitObject? NextHead(int forwardsIndex) => mapState.GetHead(nextHeadIndex + forwardsIndex);

        public ManiaDifficultyHitObject? PrevTail(int backwardsIndex) => mapState.GetTail(prevTailIndex - backwardsIndex);
        public ManiaDifficultyHitObject? NextTail(int forwardsIndex) => mapState.GetTail(nextTailIndex + forwardsIndex);

        public ManiaDifficultyHitObject? PrevHeadInColumn(int backwardsIndex, int? column = null, bool inclusive = false)
        {
            if (column is null || column == Column)
                return mapState.GetColumnHead(Column, columnPrevHeadIndex - backwardsIndex);

            return mapState.SearchColumnHead(column.Value, ActualTime, backwardsIndex, inclusive, backward: true);
        }

        public ManiaDifficultyHitObject? NextHeadInColumn(int forwardsIndex, int? column = null, bool inclusive = false)
        {
            if (column is null || column == Column)
                return mapState.GetColumnHead(Column, columnNextHeadIndex + forwardsIndex);

            return mapState.SearchColumnHead(column.Value, ActualTime, forwardsIndex, inclusive, backward: false);
        }

        public ManiaDifficultyHitObject? PrevTailInColumn(int backwardsIndex, int? column = null, bool inclusive = false)
        {
            if (column is null || column == Column)
                return mapState.GetColumnTail(Column, columnPrevTailIndex - backwardsIndex);

            return mapState.SearchColumnTail(column.Value, ActualTime, backwardsIndex, inclusive, backward: true);
        }

        public ManiaDifficultyHitObject? NextTailInColumn(int forwardsIndex, int? column = null, bool inclusive = false)
        {
            if (column is null || column == Column)
                return mapState.GetColumnTail(Column, columnNextTailIndex + forwardsIndex);

            return mapState.SearchColumnTail(column.Value, ActualTime, forwardsIndex, inclusive, backward: false);
        }
    }
}

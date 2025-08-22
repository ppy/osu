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
        public new ManiaHitObject BaseObject => (ManiaHitObject)base.BaseObject;
        public List<DifficultyHitObject> Objects { get; }
        public double EndTime => BaseObject.GetEndTime();
        public int Column => BaseObject.Column;

        public ManiaDifficultyHitObject(HitObject hitObject, HitObject lastObject, double clockRate, List<DifficultyHitObject> objects, int index)
            : base(hitObject, lastObject, clockRate, objects, index)
        {
            this.Objects = objects;
        }

        public bool IsColumnActive(int column)
        {
            double currentTime = StartTime;

            if (Column == column)
                return true;

            for (int i = Index - 1; i >= 0; i--)
            {
                if (!(Objects[i] is ManiaDifficultyHitObject obj))
                    continue;

                if (System.Math.Abs(obj.StartTime - currentTime) >= 5)
                    break;

                if (obj.Column == column)
                    return true;
            }

            for (int i = Index + 1; i < Objects.Count; i++)
            {
                if (!(Objects[i] is ManiaDifficultyHitObject obj))
                    continue;

                if (System.Math.Abs(obj.StartTime - currentTime) >= 5)
                    break;

                if (obj.Column == column)
                    return true;
            }

            return false;
        }

        public int GetLongNoteCount()
        {
            double currentTime = StartTime;
            int lnCount = 0;

            // Check this object
            if (BaseObject is HoldNote)
                lnCount++;

            // Check previous objects at the same time
            for (int i = Index - 1; i >= 0; i--)
            {
                if (!(Objects[i] is ManiaDifficultyHitObject obj))
                    continue;

                if (System.Math.Abs(obj.StartTime - currentTime) >= 5)
                    break;

                if (obj.BaseObject is HoldNote)
                    lnCount++;
            }

            // Check next objects at the same time
            for (int i = Index + 1; i < Objects.Count; i++)
            {
                if (!(Objects[i] is ManiaDifficultyHitObject obj))
                    continue;

                if (System.Math.Abs(obj.StartTime - currentTime) >= 5)
                    break;

                if (obj.BaseObject is HoldNote)
                    lnCount++;
            }

            return lnCount;
        }

        public double GetAverageLongNoteLength()
        {
            double currentTime = StartTime;
            double totalLength = 0.0;
            int lnCount = 0;

            // Check this object
            if (BaseObject is HoldNote currentHold)
            {
                totalLength += currentHold.Duration;
                lnCount++;
            }

            // Check previous objects at the same time
            for (int i = Index - 1; i >= 0; i--)
            {
                if (!(Objects[i] is ManiaDifficultyHitObject obj))
                    continue;

                if (System.Math.Abs(obj.StartTime - currentTime) >= 5)
                    break;

                if (obj.BaseObject is HoldNote hold)
                {
                    totalLength += hold.Duration;
                    lnCount++;
                }
            }

            // Check next objects at the same time
            for (int i = Index + 1; i < Objects.Count; i++)
            {
                if (!(Objects[i] is ManiaDifficultyHitObject obj))
                    continue;

                if (System.Math.Abs(obj.StartTime - currentTime) >= 5)
                    break;

                if (obj.BaseObject is HoldNote hold)
                {
                    totalLength += hold.Duration;
                    lnCount++;
                }
            }

            return lnCount > 0 ? totalLength / lnCount : 0.0;
        }

        /// <summary>
        /// Checks if this chord contains any long notes
        /// </summary>
        public bool HasLongNotes() => GetLongNoteCount() > 0;
    }
    /*public class ManiaDifficultyHitObject : DifficultyHitObject
    {
        public new ManiaHitObject BaseObject => (ManiaHitObject)base.BaseObject;

        private readonly List<DifficultyHitObject>[] perColumnObjects;

        private readonly int columnIndex;

        public readonly int Column;

        // The hit object earlier in time than this note in each column
        public readonly ManiaDifficultyHitObject?[] PreviousHitObjects;

        public readonly double ColumnStrainTime;

        public ManiaDifficultyHitObject(HitObject hitObject, HitObject lastObject, double clockRate, List<DifficultyHitObject> objects, List<DifficultyHitObject>[] perColumnObjects, int index)
            : base(hitObject, lastObject, clockRate, objects, index)
        {
            int totalColumns = perColumnObjects.Length;
            this.perColumnObjects = perColumnObjects;
            Column = BaseObject.Column;
            columnIndex = perColumnObjects[Column].Count;
            PreviousHitObjects = new ManiaDifficultyHitObject[totalColumns];
            ColumnStrainTime = StartTime - PrevInColumn(0)?.StartTime ?? StartTime;

            if (index > 0)
            {
                ManiaDifficultyHitObject prevNote = (ManiaDifficultyHitObject)objects[index - 1];

                for (int i = 0; i < prevNote.PreviousHitObjects.Length; i++)
                    PreviousHitObjects[i] = prevNote.PreviousHitObjects[i];

                // intentionally depends on processing order to match live.
                PreviousHitObjects[prevNote.Column] = prevNote;
            }
        }

        /// <summary>
        /// The previous object in the same column as this <see cref="ManiaDifficultyHitObject"/>, exclusive of Long Note tails.
        /// </summary>
        /// <param name="backwardsIndex">The number of notes to go back.</param>
        /// <returns>The object in this column <paramref name="backwardsIndex"/> notes back, or null if this is the first note in the column.</returns>
        public ManiaDifficultyHitObject? PrevInColumn(int backwardsIndex)
        {
            int index = columnIndex - (backwardsIndex + 1);
            return index >= 0 && index < perColumnObjects[Column].Count ? (ManiaDifficultyHitObject)perColumnObjects[Column][index] : null;
        }

        /// <summary>
        /// The next object in the same column as this <see cref="ManiaDifficultyHitObject"/>, exclusive of Long Note tails.
        /// </summary>
        /// <param name="forwardsIndex">The number of notes to go forward.</param>
        /// <returns>The object in this column <paramref name="forwardsIndex"/> notes forward, or null if this is the last note in the column.</returns>
        public ManiaDifficultyHitObject? NextInColumn(int forwardsIndex)
        {
            int index = columnIndex + (forwardsIndex + 1);
            return index >= 0 && index < perColumnObjects[Column].Count ? (ManiaDifficultyHitObject)perColumnObjects[Column][index] : null;
        }
    }*/

}

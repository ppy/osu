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
        public int Column => BaseObject.Column;
        public bool IsLong => EndTime > StartTime;

        private readonly int columnIndex;
        public ManiaDifficultyContext PreprocessedDifficultyData;
        public readonly List<DifficultyHitObject>[] PerColumnObjects;

        public ManiaDifficultyHitObject(HitObject hitObject, HitObject lastObject, double clockRate, List<DifficultyHitObject> objects, List<DifficultyHitObject>[] perColumnObjects, int index)
            : base(hitObject, lastObject, clockRate, objects, index)
        {
            PreprocessedDifficultyData = new ManiaDifficultyContext();
            PerColumnObjects = perColumnObjects;
            columnIndex = perColumnObjects[Column].Count;
        }

        /// <summary>
        /// The previous object in the same column as this <see cref="ManiaDifficultyHitObject"/>, exclusive of Long Note tails.
        /// </summary>
        /// <param name="backwardsIndex">The number of notes to go back.</param>
        /// <returns>The object in this column <paramref name="backwardsIndex"/> notes back, or null if this is the first note in the column.</returns>
        public ManiaDifficultyHitObject? PrevInColumn(int backwardsIndex = 0)
        {
            int index = columnIndex - (backwardsIndex + 1);
            return index >= 0 && index < PerColumnObjects[Column].Count ? (ManiaDifficultyHitObject)PerColumnObjects[Column][index] : null;
        }

        /// <summary>
        /// The next object in the same column as this <see cref="ManiaDifficultyHitObject"/>, exclusive of Long Note tails.
        /// </summary>
        /// <param name="forwardsIndex">The number of notes to go forward.</param>
        /// <returns>The object in this column <paramref name="forwardsIndex"/> notes forward, or null if this is the last note in the column.</returns>
        public ManiaDifficultyHitObject? NextInColumn(int forwardsIndex = 0)
        {
            int index = columnIndex + (forwardsIndex + 1);
            return index >= 0 && index < PerColumnObjects[Column].Count ? (ManiaDifficultyHitObject)PerColumnObjects[Column][index] : null;
        }
    }
}

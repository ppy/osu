// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Difficulty.Utils
{
    /// <summary>
    /// Wraps a list of <see cref="DifficultyHitObject"/>s.
    /// Provides access to the current object in time in difficulty calculation, as well as it's preceding and succeeding objects using relative indices.
    /// </summary>
    public class DifficultyHitObjectIterator : IEnumerator
    {
        private readonly List<DifficultyHitObject> difficultyHitObjects;
        public readonly int Count;

        public int Position = -1;
        public DifficultyHitObject Current => difficultyHitObjects[Position];

        public DifficultyHitObjectIterator(List<DifficultyHitObject> objects)
        {
            difficultyHitObjects = objects;
            Count = objects.Count;
        }

        public DifficultyHitObject Previous(int reverseIndex)
        {
            if (reverseIndex - 1 > Position)
                throw new InvalidOperationException("Cannot index when there are no previous objects.");

            return difficultyHitObjects[Position - reverseIndex - 1];
        }

        public DifficultyHitObject Next(int index)
        {
            if (index + 1 > difficultyHitObjects.Count - Position)
                throw new InvalidOperationException("Cannot index when there are no future objects.");

            return difficultyHitObjects[Position + index + 1];
        }

        public bool MoveNext()
        {
            Position++;
            return Position < difficultyHitObjects.Count;
        }

        public void Reset()
        {
            Position = -1;
        }

        object IEnumerator.Current => Current;
    }
}

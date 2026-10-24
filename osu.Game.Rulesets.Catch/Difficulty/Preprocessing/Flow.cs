// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Catch.Difficulty.Preprocessing
{
    public class Flow
    {
        private readonly IReadOnlyList<Flow> flows;
        private readonly List<CatchDifficultyHitObject> hitObjects;

        public readonly int Index;
        public readonly int Direction;

        public float DistanceMoved { get; private set; }
        public double StrainTime { get; private set; }

        public Flow(List<CatchDifficultyHitObject> hitObjects, List<Flow> flows, int index)
        {
            // if (hitObjects.Select(h => h.Direction).Distinct().Count() != 1)
            //     throw new ArgumentException("hitObjects are not of the same Direction", nameof(hitObjects));

            this.flows = flows;
            this.hitObjects = hitObjects;
            Index = index;
            Direction = hitObjects.First().Direction;
            DistanceMoved = hitObjects.Aggregate(0.0f, (acc, h) => acc + h.DistanceMoved);
            StrainTime = Math.Max(hitObjects.Aggregate(0.0, (acc, h) => acc + h.DeltaTime), 40);
        }

        public void Add(CatchDifficultyHitObject hitObject)
        {
            hitObjects.Add(hitObject);
            DistanceMoved += hitObject.DistanceMoved;
            StrainTime = Math.Max(hitObjects.Aggregate(0.0, (acc, h) => acc + h.DeltaTime), 40);
        }

        // Return true whenever the previous object is a direction change.
        // We treat the first object of the map as a non-direction change.
        public bool IsFirst(CatchDifficultyHitObject hitObject) => hitObject.Index == hitObjects.First().Index;

        public Flow? Previous(int backwardsIndex)
        {
            int index = Index - (backwardsIndex + 1);
            return index >= 0 && index < flows.Count ? flows[index] : null;
        }
    }
}

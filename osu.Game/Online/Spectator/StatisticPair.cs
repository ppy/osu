// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Online.Spectator
{
    [Serializable]
    public struct StatisticPair
    {
        public HitResult Result;
        public int Count;

        public StatisticPair(HitResult result, int count)
        {
            Result = result;
            Count = count;
        }

        public override string ToString() => $"{Result}=>{Count}";
    }
}

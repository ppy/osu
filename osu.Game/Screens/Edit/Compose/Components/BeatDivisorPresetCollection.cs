// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public class BeatDivisorPresetCollection
    {
        public BeatDivisorType Type { get; }
        public IReadOnlyList<int> Presets { get; }

        private BeatDivisorPresetCollection(BeatDivisorType type, IEnumerable<int> presets)
        {
            Type = type;
            Presets = presets.ToArray();
        }

        public static readonly BeatDivisorPresetCollection COMMON = new BeatDivisorPresetCollection(BeatDivisorType.Common, new[] { 1, 2, 4, 8, 16 });

        public static readonly BeatDivisorPresetCollection TRIPLETS = new BeatDivisorPresetCollection(BeatDivisorType.Triplets, new[] { 1, 3, 6, 12 });

        public static BeatDivisorPresetCollection Custom(int maxDivisor)
        {
            var presets = new List<int>();

            for (int candidate = 1; candidate <= Math.Sqrt(maxDivisor); ++candidate)
            {
                if (maxDivisor % candidate != 0)
                    continue;

                presets.Add(candidate);
                presets.Add(maxDivisor / candidate);
            }

            return new BeatDivisorPresetCollection(BeatDivisorType.Custom, presets.Distinct().Order());
        }
    }
}

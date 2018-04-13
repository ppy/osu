// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Beatmaps
{
    public interface IBeatmapConverter
    {
        /// <summary>
        /// Invoked when a <see cref="HitObject"/> has been converted.
        /// The first argument contains the <see cref="HitObject"/> that was converted.
        /// The second argument contains the <see cref="HitObject"/>s that were output from the conversion process.
        /// </summary>
        event Action<HitObject, IEnumerable<HitObject>> ObjectConverted;

        /// <summary>
        /// Converts a Beatmap using this Beatmap Converter.
        /// </summary>
        /// <param name="beatmap">The un-converted Beatmap.</param>
        void Convert(Beatmap beatmap);
    }
}

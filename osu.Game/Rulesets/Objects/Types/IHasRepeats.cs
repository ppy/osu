// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Audio;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Objects.Types
{
    /// <summary>
    /// A HitObject that spans some length.
    /// </summary>
    public interface IHasRepeats : IHasEndTime
    {
        /// <summary>
        /// The amount of times the HitObject repeats.
        /// </summary>
        int RepeatCount { get; }

        /// <summary>
        /// The samples to be played when each repeat node is hit (0 -> first repeat node, 1 -> second repeat node, etc).
        /// </summary>
        List<List<SampleInfo>> RepeatSamples { get; }
    }

    public static class HasRepeatsExtensions
    {
        /// <summary>
        /// The amount of times the length of this <see cref="IHasRepeats"/> spans.
        /// </summary>
        /// <param name="obj">The object that has repeats.</param>
        public static int SpanCount(this IHasRepeats obj) => obj.RepeatCount + 1;
    }
}

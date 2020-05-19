// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
        int RepeatCount { get; set; }

        /// <summary>
        /// The samples to be played when each node of the <see cref="IHasRepeats"/> is hit.<br />
        /// 0: The first node.<br />
        /// 1: The first repeat.<br />
        /// 2: The second repeat.<br />
        /// ...<br />
        /// n-1: The last repeat.<br />
        /// n: The last node.
        /// </summary>
        List<IList<HitSampleInfo>> NodeSamples { get; }
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

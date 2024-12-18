// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Audio;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Objects.Types
{
    /// <summary>
    /// A HitObject that spans some length.
    /// </summary>
    public interface IHasRepeats : IHasDuration
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
        IList<IList<HitSampleInfo>> NodeSamples { get; }
    }

    public static class HasRepeatsExtensions
    {
        /// <summary>
        /// The amount of times the length of this <see cref="IHasRepeats"/> spans.
        /// </summary>
        /// <param name="obj">The object that has repeats.</param>
        public static int SpanCount(this IHasRepeats obj) => obj.RepeatCount + 1;

        /// <summary>
        /// Retrieves the samples at a particular node in a <see cref="IHasRepeats"/> object.
        /// </summary>
        /// <param name="obj">The <see cref="HitObject"/>.</param>
        /// <param name="nodeIndex">The node to attempt to retrieve the samples at.</param>
        /// <returns>The samples at the given node index, or <paramref name="obj"/>'s default samples if the given node doesn't exist.</returns>
        public static IList<HitSampleInfo> GetNodeSamples<T>(this T obj, int nodeIndex)
            where T : HitObject, IHasRepeats
            => nodeIndex < obj.NodeSamples.Count ? obj.NodeSamples[nodeIndex] : obj.Samples;

        /// <summary>
        /// Ensures that the list of node samples is at least as long as the number of nodes.
        /// </summary>
        /// <param name="obj">The <see cref="HitObject"/>.</param>
        public static void PopulateNodeSamples<T>(this T obj)
            where T : HitObject, IHasRepeats
        {
            if (obj.NodeSamples.Count >= obj.RepeatCount + 2)
                return;

            while (obj.NodeSamples.Count < obj.RepeatCount + 2)
                obj.NodeSamples.Add(obj.Samples.Select(o => o.With()).ToList());
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// A snap provider which given a reference hit object and proposed distance from it, offers a more correct duration or distance value.
    /// </summary>
    [Cached]
    public interface IDistanceSnapProvider
    {
        /// <summary>
        /// A multiplier which changes the ratio of distance travelled per time unit.
        /// Importantly, this is provided for manual usage, and not multiplied into any of the methods exposed by this interface.
        /// </summary>
        /// <seealso cref="IBeatmap.DistanceSpacing"/>
        Bindable<double> DistanceSpacingMultiplier { get; }

        /// <summary>
        /// Returns the spatial distance between objects which are temporally one beat apart.
        /// Depends on:
        /// <list type="bullet">
        /// <item>the slider velocity taken from <paramref name="withVelocity"/>,</item>
        /// <item>the beatmap's <see cref="IBeatmapDifficultyInfo.SliderMultiplier"/>,</item>,
        /// <item>the current beat divisor.</item>
        /// </list>
        /// Note that the returned value does <b>NOT</b> depend on <see cref="DistanceSpacingMultiplier"/>;
        /// consumers are expected to include that multiplier as they see fit.
        /// </summary>
        float GetBeatSnapDistance(IHasSliderVelocity? withVelocity = null);

        /// <summary>
        /// Converts a temporal duration into a spatial distance.
        /// Does not perform any snapping.
        /// Depends on:
        /// <list type="bullet">
        /// <item>the <paramref name="duration"/> provided,</item>
        /// <item>a <paramref name="timingReference"/> used to retrieve the beat length of the beatmap at that time,</item>
        /// <item>the slider velocity taken from <paramref name="withVelocity"/>,</item>
        /// <item>the beatmap's <see cref="IBeatmapDifficultyInfo.SliderMultiplier"/>,</item>,
        /// <item>the current beat divisor.</item>
        /// </list>
        /// Note that the returned value does <b>NOT</b> depend on <see cref="DistanceSpacingMultiplier"/>;
        /// consumers are expected to include that multiplier as they see fit.
        /// </summary>
        float DurationToDistance(double duration, double timingReference, IHasSliderVelocity? withVelocity = null);

        /// <summary>
        /// Converts a spatial distance into a temporal duration.
        /// Does not perform any snapping.
        /// Depends on:
        /// <list type="bullet">
        /// <item>the <paramref name="distance"/> provided,</item>
        /// <item>a <paramref name="timingReference"/> used to retrieve the beat length of the beatmap at that time,</item>
        /// <item>the slider velocity taken from <paramref name="withVelocity"/>,</item>
        /// <item>the beatmap's <see cref="IBeatmapDifficultyInfo.SliderMultiplier"/>,</item>,
        /// <item>the current beat divisor.</item>
        /// </list>
        /// Note that the returned value does <b>NOT</b> depend on <see cref="DistanceSpacingMultiplier"/>;
        /// consumers are expected to include that multiplier as they see fit.
        /// </summary>
        double DistanceToDuration(float distance, double timingReference, IHasSliderVelocity? withVelocity = null);

        /// <summary>
        /// Snaps a spatial distance to the beat, relative to <paramref name="snapReferenceTime"/>.
        /// Depends on:
        /// <list type="bullet">
        /// <item>the <paramref name="distance"/> provided,</item>
        /// <item>a <paramref name="snapReferenceTime"/> used to retrieve the beat length of the beatmap at that time,</item>
        /// <item>the slider velocity taken from <paramref name="withVelocity"/>,</item>
        /// <item>the beatmap's <see cref="IBeatmapDifficultyInfo.SliderMultiplier"/>,</item>,
        /// <item>the current beat divisor.</item>
        /// </list>
        /// Note that the returned value does <b>NOT</b> depend on <see cref="DistanceSpacingMultiplier"/>;
        /// consumers are expected to include that multiplier as they see fit.
        /// </summary>
        float FindSnappedDistance(float distance, double snapReferenceTime, IHasSliderVelocity? withVelocity = null);
    }
}

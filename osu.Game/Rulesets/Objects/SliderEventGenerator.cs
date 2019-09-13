// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osuTK;

namespace osu.Game.Rulesets.Objects
{
    public static class SliderEventGenerator
    {
        public static IEnumerable<SliderEventDescriptor> Generate(double startTime, double spanDuration, double velocity, double tickDistance, double totalDistance, int spanCount,
                                                                  double? legacyLastTickOffset)
        {
            // A very lenient maximum length of a slider for ticks to be generated.
            // This exists for edge cases such as /b/1573664 where the beatmap has been edited by the user, and should never be reached in normal usage.
            const double max_length = 100000;

            var length = Math.Min(max_length, totalDistance);
            tickDistance = MathHelper.Clamp(tickDistance, 0, length);

            var minDistanceFromEnd = velocity * 10;

            yield return new SliderEventDescriptor
            {
                Type = SliderEventType.Head,
                SpanIndex = 0,
                SpanStartTime = startTime,
                Time = startTime,
                PathProgress = 0,
            };

            if (tickDistance != 0)
            {
                for (var span = 0; span < spanCount; span++)
                {
                    var spanStartTime = startTime + span * spanDuration;
                    var reversed = span % 2 == 1;

                    var ticks = generateTicks(span, spanStartTime, spanDuration, reversed, length, tickDistance, minDistanceFromEnd);

                    if (reversed)
                    {
                        // For repeat spans, ticks are returned in reverse-StartTime order, which is undesirable for some rulesets
                        ticks = ticks.Reverse();
                    }

                    foreach (var e in ticks)
                        yield return e;

                    if (span < spanCount - 1)
                    {
                        yield return new SliderEventDescriptor
                        {
                            Type = SliderEventType.Repeat,
                            SpanIndex = span,
                            SpanStartTime = startTime + span * spanDuration,
                            Time = spanStartTime + spanDuration,
                            PathProgress = (span + 1) % 2,
                        };
                    }
                }
            }

            double totalDuration = spanCount * spanDuration;

            // Okay, I'll level with you. I made a mistake. It was 2007.
            // Times were simpler. osu! was but in its infancy and sliders were a new concept.
            // A hack was made, which has unfortunately lived through until this day.
            //
            // This legacy tick is used for some calculations and judgements where audio output is not required.
            // Generally we are keeping this around just for difficulty compatibility.
            // Optimistically we do not want to ever use this for anything user-facing going forwards.

            int finalSpanIndex = spanCount - 1;
            double finalSpanStartTime = startTime + finalSpanIndex * spanDuration;
            double finalSpanEndTime = Math.Max(startTime + totalDuration / 2, (finalSpanStartTime + spanDuration) - (legacyLastTickOffset ?? 0));
            double finalProgress = (finalSpanEndTime - finalSpanStartTime) / spanDuration;

            if (spanCount % 2 == 0) finalProgress = 1 - finalProgress;

            yield return new SliderEventDescriptor
            {
                Type = SliderEventType.LegacyLastTick,
                SpanIndex = finalSpanIndex,
                SpanStartTime = finalSpanStartTime,
                Time = finalSpanEndTime,
                PathProgress = finalProgress,
            };

            yield return new SliderEventDescriptor
            {
                Type = SliderEventType.Tail,
                SpanIndex = finalSpanIndex,
                SpanStartTime = startTime + (spanCount - 1) * spanDuration,
                Time = startTime + totalDuration,
                PathProgress = spanCount % 2,
            };
        }

        /// <summary>
        /// Generates the ticks for a span of the slider.
        /// </summary>
        /// <param name="spanIndex">The span index.</param>
        /// <param name="spanStartTime">The start time of the span.</param>
        /// <param name="spanDuration">The duration of the span.</param>
        /// <param name="reversed">Whether the span is reversed.</param>
        /// <param name="length">The length of the path.</param>
        /// <param name="tickDistance">The distance between each tick.</param>
        /// <param name="minDistanceFromEnd">The distance from the end of the path at which ticks are not allowed to be added.</param>
        /// <returns>A <see cref="SliderEventDescriptor"/> for each tick. If <paramref name="reversed"/> is true, the ticks will be returned in reverse-StartTime order.</returns>
        private static IEnumerable<SliderEventDescriptor> generateTicks(int spanIndex, double spanStartTime, double spanDuration, bool reversed, double length, double tickDistance,
                                                                        double minDistanceFromEnd)
        {
            for (var d = tickDistance; d <= length; d += tickDistance)
            {
                if (d >= length - minDistanceFromEnd)
                    break;

                // Always generate ticks from the start of the path rather than the span to ensure that ticks in repeat spans are positioned identically to those in non-repeat spans
                var pathProgress = d / length;
                var timeProgress = reversed ? 1 - pathProgress : pathProgress;

                yield return new SliderEventDescriptor
                {
                    Type = SliderEventType.Tick,
                    SpanIndex = spanIndex,
                    SpanStartTime = spanStartTime,
                    Time = spanStartTime + timeProgress * spanDuration,
                    PathProgress = pathProgress,
                };
            }
        }
    }

    /// <summary>
    /// Describes a point in time on a slider given special meaning.
    /// Should be used by rulesets to visualise the slider.
    /// </summary>
    public struct SliderEventDescriptor
    {
        /// <summary>
        /// The type of event.
        /// </summary>
        public SliderEventType Type;

        /// <summary>
        /// The time of this event.
        /// </summary>
        public double Time;

        /// <summary>
        /// The zero-based index of the span. In the case of repeat sliders, this will increase after each <see cref="SliderEventType.Repeat"/>.
        /// </summary>
        public int SpanIndex;

        /// <summary>
        /// The time at which the contained <see cref="SpanIndex"/> begins.
        /// </summary>
        public double SpanStartTime;

        /// <summary>
        /// The progress along the slider's <see cref="SliderPath"/> at which this event occurs.
        /// </summary>
        public double PathProgress;
    }

    public enum SliderEventType
    {
        Tick,
        LegacyLastTick,
        Head,
        Tail,
        Repeat
    }
}

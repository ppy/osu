// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osuTK;

namespace osu.Game.Beatmaps.Formats
{
    public static class SliderEventGenerator
    {
        public static IEnumerable<SliderEventDescriptor> Generate(double startTime, double spanDuration, double velocity, double tickDistance, double totalDistance, int spanCount, double? legacyLastTickOffset)
        {
            List<SliderEventDescriptor> events = new List<SliderEventDescriptor>();

            // A very lenient maximum length of a slider for ticks to be generated.
            // This exists for edge cases such as /b/1573664 where the beatmap has been edited by the user, and should never be reached in normal usage.
            const double max_length = 100000;

            var length = Math.Min(max_length, totalDistance);
            tickDistance = MathHelper.Clamp(tickDistance, 0, length);

            {
                var minDistanceFromEnd = velocity * 10;

                events.Add(new SliderEventDescriptor
                {
                    Type = SliderEventType.Head,
                    SpanIndex = 0,
                    SpanStartTime = startTime,
                    StartTime = startTime,
                    PathProgress = 0,
                });

                if (tickDistance != 0)
                {
                    for (var span = 0; span < spanCount; span++)
                    {
                        var spanStartTime = startTime + span * spanDuration;
                        var reversed = span % 2 == 1;

                        for (var d = tickDistance; d <= length; d += tickDistance)
                        {
                            if (d > length - minDistanceFromEnd)
                                break;

                            var pathProgress = d / length;
                            var timeProgress = reversed ? 1 - pathProgress : pathProgress;

                            events.Add(new SliderEventDescriptor
                            {
                                Type = SliderEventType.Tick,
                                SpanIndex = span,
                                SpanStartTime = spanStartTime,
                                StartTime = spanStartTime + timeProgress * spanDuration,
                                PathProgress = pathProgress,
                            });
                        }

                        if (span < spanCount - 1)
                        {
                            events.Add(new SliderEventDescriptor
                            {
                                Type = SliderEventType.Repeat,
                                SpanIndex = span,
                                SpanStartTime = startTime + span * spanDuration,
                                StartTime = spanStartTime + (span + 1) * spanDuration,
                                PathProgress = 1,
                            });
                        }
                    }
                }

                double totalDuration = spanCount * spanDuration;

                var tail = new SliderEventDescriptor
                {
                    Type = SliderEventType.Tail,
                    SpanIndex = spanCount - 1,
                    SpanStartTime = startTime + (spanCount - 1) * spanDuration,
                    StartTime = startTime + totalDuration,
                    PathProgress = 1,
                };

                if (legacyLastTickOffset != null)
                    tail.StartTime = Math.Max(startTime + totalDuration / 2, tail.StartTime - legacyLastTickOffset.Value);

                events.Add(tail);

                return events;
            }
        }
    }

    public class SliderEventDescriptor
    {
        public SliderEventType Type;

        public int SpanIndex;

        public double SpanStartTime;

        public double StartTime;

        public double PathProgress;
    }

    public enum SliderEventType
    {
        Tick,
        Head,
        Tail,
        Repeat
    }
}

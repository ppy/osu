// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Threading;
using osuTK;

namespace osu.Game.Graphics.UserInterface
{
    public abstract partial class SegmentedGraph<T> : Container
        where T : struct, IComparable<T>, IConvertible, IEquatable<T>
    {
        private BufferedContainer? rectSegments;
        private float previousDrawWidth;
        private bool graphNeedsUpdate;

        private T[]? values;
        private int[] tiers = Array.Empty<int>();
        private readonly SegmentManager segments;

        private readonly int tierCount;

        protected SegmentedGraph(int tierCount)
        {
            this.tierCount = tierCount;
            TierColours = new Colour4[tierCount];
            segments = new SegmentManager(tierCount);
        }

        public T[] Values
        {
            get => values ?? Array.Empty<T>();
            set
            {
                if (value == values) return;

                values = value;
                recalculateTiers(values);
                graphNeedsUpdate = true;
            }
        }

        public readonly Colour4[] TierColours;

        private CancellationTokenSource? cts;
        private ScheduledDelegate? scheduledCreate;

        protected override void Update()
        {
            base.Update();

            if (graphNeedsUpdate || (values != null && DrawWidth != previousDrawWidth))
            {
                rectSegments?.FadeOut(150, Easing.OutQuint).Expire();

                scheduledCreate?.Cancel();
                scheduledCreate = Scheduler.AddDelayed(RecreateGraph, 150);

                previousDrawWidth = DrawWidth;
                graphNeedsUpdate = false;
            }
        }

        protected virtual void RecreateGraph()
        {
            var newSegments = new BufferedContainer(cachedFrameBuffer: true)
            {
                RedrawOnScale = false,
                RelativeSizeAxes = Axes.Both
            };

            cts?.Cancel();
            recalculateSegments();
            redrawSegments(newSegments);

            LoadComponentAsync(newSegments, s =>
            {
                Children = new Drawable[]
                {
                    rectSegments = s
                };

                s.FadeInFromZero(100);
            }, (cts = new CancellationTokenSource()).Token);
        }

        private void recalculateTiers(T[]? arr)
        {
            if (arr == null || arr.Length == 0)
            {
                tiers = Array.Empty<int>();
                return;
            }

            float[] floatValues = arr.Select(v => Convert.ToSingle(v)).ToArray();

            // Shift values to eliminate negative ones
            float min = floatValues.Min();

            if (min < 0)
            {
                for (int i = 0; i < floatValues.Length; i++)
                    floatValues[i] += min;
            }

            // Normalize values
            float max = floatValues.Max();

            for (int i = 0; i < floatValues.Length; i++)
                floatValues[i] /= max;

            // Deduce tiers from values
            tiers = floatValues.Select(v => (int)Math.Floor(v * tierCount)).ToArray();
        }

        private void recalculateSegments()
        {
            segments.Clear();

            if (tiers.Length == 0)
            {
                segments.Add(0, 0, 1);
                return;
            }

            for (int i = 0; i < tiers.Length; i++)
            {
                for (int tier = 0; tier < tierCount; tier++)
                {
                    if (tier < 0)
                        continue;

                    // One tier covers itself and all tiers above it.
                    // By layering multiple transparent boxes, higher tiers will be brighter.
                    // If using opaque colors, higher tiers will be on front, covering lower tiers.
                    if (tiers[i] >= tier)
                    {
                        if (!segments.IsTierStarted(tier))
                            segments.StartSegment(tier, i * 1f / tiers.Length);
                    }
                    else
                    {
                        if (segments.IsTierStarted(tier))
                            segments.EndSegment(tier, i * 1f / tiers.Length);
                    }
                }
            }

            segments.EndAllPendingSegments();
            segments.Sort();
        }

        private Colour4 tierToColour(int tier) => tier >= 0 ? TierColours[tier] : new Colour4(0, 0, 0, 0);

        // Base implementation, could be drawn with draw node if preferred
        private void redrawSegments(BufferedContainer container)
        {
            if (segments.Count == 0)
                return;

            foreach (SegmentInfo segment in segments) // Lower tiers will be drawn first, putting them in the back
            {
                float width = segment.Length * DrawWidth;

                // If the segment width exceeds the DrawWidth, just fill the rest
                if (width >= DrawWidth)
                    width = DrawWidth;

                container.Add(new Box
                {
                    Name = $"Tier {segment.Tier} segment",
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.Y,
                    Position = new Vector2(segment.Start * DrawWidth, 0),
                    Width = width,
                    Colour = tierToColour(segment.Tier)
                });
            }
        }

        protected struct SegmentInfo
        {
            /// <summary>
            /// The tier this segment is at.
            /// </summary>
            public int Tier;

            /// <summary>
            /// The progress at which this segment starts.
            /// </summary>
            /// <remarks>
            /// The value is a normalized float (from 0 to 1).
            /// </remarks>
            public float Start;

            /// <summary>
            /// The progress at which this segment ends.
            /// </summary>
            /// <remarks>
            /// The value is a normalized float (from 0 to 1).
            /// </remarks>
            public float End;

            /// <summary>
            /// The length of this segment.
            /// </summary>
            /// <remarks>
            /// The value is a normalized float (from 0 to 1).
            /// </remarks>
            public float Length => End - Start;
        }

        protected class SegmentManager : IEnumerable<SegmentInfo>
        {
            private readonly List<SegmentInfo> segments = new List<SegmentInfo>();

            private readonly SegmentInfo?[] pendingSegments;

            public SegmentManager(int tierCount)
            {
                pendingSegments = new SegmentInfo?[tierCount];
            }

            public void StartSegment(int tier, float start)
            {
                if (pendingSegments[tier] != null)
                    throw new InvalidOperationException($"Another {nameof(SegmentInfo)} of tier {tier.ToString()} has already been started.");

                pendingSegments[tier] = new SegmentInfo
                {
                    Tier = tier,
                    Start = Math.Clamp(start, 0, 1)
                };
            }

            public void EndSegment(int tier, float end)
            {
                SegmentInfo? pendingSegment = pendingSegments[tier];
                if (pendingSegment == null)
                    throw new InvalidOperationException($"Cannot end {nameof(SegmentInfo)} of tier {tier.ToString()} that has not been started.");

                SegmentInfo segment = pendingSegment.Value;
                segment.End = Math.Clamp(end, 0, 1);
                segments.Add(segment);
                pendingSegments[tier] = null;
            }

            public void EndAllPendingSegments()
            {
                foreach (SegmentInfo? pendingSegment in pendingSegments)
                {
                    if (pendingSegment != null)
                    {
                        SegmentInfo finalizedSegment = pendingSegment.Value;
                        finalizedSegment.End = 1;
                        segments.Add(finalizedSegment);
                    }
                }
            }

            public void Sort() =>
                segments.Sort((a, b) =>
                    a.Tier != b.Tier
                        ? a.Tier.CompareTo(b.Tier)
                        : a.Start.CompareTo(b.Start));

            public void Add(SegmentInfo segment) => segments.Add(segment);

            public void Clear()
            {
                segments.Clear();

                for (int i = 0; i < pendingSegments.Length; i++)
                    pendingSegments[i] = null;
            }

            public int Count => segments.Count;

            public void Add(int tier, float start, float end)
            {
                SegmentInfo segment = new SegmentInfo
                {
                    Tier = tier,
                    Start = Math.Clamp(start, 0, 1),
                    End = Math.Clamp(end, 0, 1)
                };

                if (segment.Start > segment.End)
                    throw new InvalidOperationException("Segment start cannot be after segment end.");

                Add(segment);
            }

            public bool IsTierStarted(int tier)
            {
                if (tier < 0)
                    return false;

                return pendingSegments[tier].HasValue;
            }

            public IEnumerator<SegmentInfo> GetEnumerator() => segments.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}

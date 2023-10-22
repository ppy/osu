// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using osuTK;

namespace osu.Game.Graphics.UserInterface
{
    public partial class SegmentedGraph<T> : Drawable
        where T : struct, IComparable<T>, IConvertible, IEquatable<T>
    {
        private bool graphNeedsUpdate;

        private T[]? values;
        private int[] tiers = Array.Empty<int>();
        private readonly SegmentManager segments;

        private int tierCount;

        public SegmentedGraph(int tierCount = 1)
        {
            this.tierCount = tierCount;
            tierColours = new[]
            {
                new Colour4(0, 0, 0, 0)
            };
            segments = new SegmentManager(tierCount);
        }

        public T[] Values
        {
            get => values ?? Array.Empty<T>();
            set
            {
                if (value == values) return;

                values = value;
                graphNeedsUpdate = true;
            }
        }

        private IReadOnlyList<Colour4> tierColours;

        public IReadOnlyList<Colour4> TierColours
        {
            get => tierColours;
            set
            {
                tierCount = value.Count;
                tierColours = value;

                graphNeedsUpdate = true;
            }
        }

        private Texture texture = null!;
        private IShader shader = null!;

        [BackgroundDependencyLoader]
        private void load(IRenderer renderer, ShaderManager shaders)
        {
            texture = renderer.WhitePixel;
            shader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE);
        }

        protected override void Update()
        {
            base.Update();

            if (graphNeedsUpdate)
            {
                recalculateTiers(values);
                recalculateSegments();
                Invalidate(Invalidation.DrawNode);
                graphNeedsUpdate = false;
            }
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
                    floatValues[i] += Math.Abs(min);
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

        protected override DrawNode CreateDrawNode() => new SegmentedGraphDrawNode(this);

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

            public override string ToString()
            {
                return $"({Tier}, {Start * 100}%, {End * 100}%)";
            }
        }

        private class SegmentedGraphDrawNode : DrawNode
        {
            public new SegmentedGraph<T> Source => (SegmentedGraph<T>)base.Source;

            private Texture texture = null!;
            private IShader shader = null!;
            private readonly List<SegmentInfo> segments = new List<SegmentInfo>();
            private Vector2 drawSize;
            private readonly List<Colour4> tierColours = new List<Colour4>();

            public SegmentedGraphDrawNode(SegmentedGraph<T> source)
                : base(source)
            {
            }

            public override void ApplyState()
            {
                base.ApplyState();

                texture = Source.texture;
                shader = Source.shader;
                drawSize = Source.DrawSize;

                segments.Clear();
                segments.AddRange(Source.segments.Where(s => s.Length * drawSize.X > 1));

                tierColours.Clear();
                tierColours.AddRange(Source.tierColours);
            }

            public override void Draw(IRenderer renderer)
            {
                base.Draw(renderer);

                shader.Bind();

                foreach (SegmentInfo segment in segments)
                {
                    Vector2 topLeft = new Vector2(segment.Start * drawSize.X, 0);
                    Vector2 topRight = new Vector2(segment.End * drawSize.X, 0);
                    Vector2 bottomLeft = new Vector2(segment.Start * drawSize.X, drawSize.Y);
                    Vector2 bottomRight = new Vector2(segment.End * drawSize.X, drawSize.Y);

                    renderer.DrawQuad(
                        texture,
                        new Quad(
                            Vector2Extensions.Transform(topLeft, DrawInfo.Matrix),
                            Vector2Extensions.Transform(topRight, DrawInfo.Matrix),
                            Vector2Extensions.Transform(bottomLeft, DrawInfo.Matrix),
                            Vector2Extensions.Transform(bottomRight, DrawInfo.Matrix)),
                        getSegmentColour(segment));
                }

                shader.Unbind();
            }

            private ColourInfo getSegmentColour(SegmentInfo segment)
            {
                var segmentColour = DrawColourInfo.Colour.Interpolate(new Quad(segment.Start, 0f, segment.End - segment.Start, 1f));

                var tierColour = segment.Tier >= 0 ? tierColours[segment.Tier] : new Colour4(0, 0, 0, 0);
                segmentColour.ApplyChild(tierColour);

                return segmentColour;
            }
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
                SegmentInfo? pendingSegment = pendingSegments[tier] ?? throw new InvalidOperationException($"Cannot end {nameof(SegmentInfo)} of tier {tier.ToString()} that has not been started.");
                SegmentInfo segment = pendingSegment.Value;
                segment.End = Math.Clamp(end, 0, 1);
                segments.Add(segment);
                pendingSegments[tier] = null;
            }

            public void EndAllPendingSegments()
            {
                foreach (SegmentInfo? pendingSegment in pendingSegments)
                {
                    if (pendingSegment == null)
                        continue;

                    SegmentInfo finalizedSegment = pendingSegment.Value;
                    finalizedSegment.End = 1;
                    segments.Add(finalizedSegment);
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

            public bool IsTierStarted(int tier) => tier >= 0 && pendingSegments[tier].HasValue;

            public IEnumerator<SegmentInfo> GetEnumerator() => segments.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}

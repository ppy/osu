// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osuTK;
using osu.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Utils;
using System;

namespace osu.Game.Graphics.UserInterface
{
    public class BarGraph : Drawable
    {
        private const int resize_duration = 250;
        private const Easing easing = Easing.InOutCubic;

        /// <summary>
        /// Manually sets the max value, if null <see cref="Enumerable.Max(IEnumerable{float})"/> is instead used
        /// </summary>
        public float? MaxValue { get; set; }

        private BarDirection direction = BarDirection.BottomToTop;

        public BarDirection Direction
        {
            get => direction;
            set
            {
                if (direction == value)
                    return;

                direction = value;
                Invalidate(Invalidation.DrawNode);
            }
        }

        private readonly BarsInfo bars = new BarsInfo();

        /// <summary>
        /// A list of floats that defines the length of each <see cref="Bar"/>
        /// </summary>
        public IEnumerable<float> Values
        {
            set
            {
                if (!value.Any())
                {
                    bars.Clear();
                    Invalidate(Invalidation.DrawNode);
                    return;
                }

                int newCount = value.Count();

                bars.Breadth = 1.0f / newCount;

                float maxLength = MaxValue ?? value.Max();

                foreach (var bar in value.Select((length, index) => (Value: length, Index: index)))
                {
                    float length = maxLength == 0 ? 0 : Math.Max(0f, bar.Value / maxLength);

                    if (bar.Index < bars.Count)
                    {
                        bars.UpdateLength(bar.Index, length);
                        continue;
                    }

                    bars.AddBar(length);
                }

                if (bars.Count > newCount)
                    bars.RemoveRange(newCount, bars.Count - newCount);

                animationStartTime = Clock.CurrentTime;
                animationComplete = false;
            }
        }

        private double animationStartTime;
        private bool animationComplete;

        private IShader shader = null!;
        private Texture texture = null!;

        [BackgroundDependencyLoader]
        private void load(IRenderer renderer, ShaderManager shaders)
        {
            texture = renderer.WhitePixel;
            shader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE);
        }

        protected override void Update()
        {
            base.Update();

            if (!bars.Any)
                return;

            double currentTime = Clock.CurrentTime;

            if (currentTime < animationStartTime + resize_duration)
            {
                bars.Animate(animationStartTime, currentTime);
                Invalidate(Invalidation.DrawNode);
            }
            else if (!animationComplete)
            {
                bars.FinishAnimation();
                Invalidate(Invalidation.DrawNode);

                animationComplete = true;
            }
        }

        protected override DrawNode CreateDrawNode() => new BarGraphDrawNode(this);

        private class BarGraphDrawNode : DrawNode
        {
            public new BarGraph Source => (BarGraph)base.Source;

            public BarGraphDrawNode(BarGraph source)
                : base(source)
            {
            }

            private IShader shader = null!;
            private Texture texture = null!;
            private Vector2 drawSize;
            private BarDirection direction;
            private float barBreadth;

            private readonly List<float> lengths = new List<float>();

            public override void ApplyState()
            {
                base.ApplyState();

                shader = Source.shader;
                texture = Source.texture;
                drawSize = Source.DrawSize;
                direction = Source.direction;
                barBreadth = Source.bars.Breadth;

                lengths.Clear();
                lengths.AddRange(Source.bars.InstantaneousLengths);
            }

            public override void Draw(IRenderer renderer)
            {
                base.Draw(renderer);

                shader.Bind();

                for (int i = 0; i < lengths.Count; i++)
                {
                    float barHeight = drawSize.Y * ((direction == BarDirection.TopToBottom || direction == BarDirection.BottomToTop) ? lengths[i] : barBreadth);
                    float barWidth = drawSize.X * ((direction == BarDirection.LeftToRight || direction == BarDirection.RightToLeft) ? lengths[i] : barBreadth);

                    Vector2 topLeft;

                    switch (direction)
                    {
                        default:
                        case BarDirection.LeftToRight:
                            topLeft = new Vector2(0, i * barHeight);
                            break;

                        case BarDirection.RightToLeft:
                            topLeft = new Vector2(drawSize.X - barWidth, i * barHeight);
                            break;

                        case BarDirection.TopToBottom:
                            topLeft = new Vector2(i * barWidth, 0);
                            break;

                        case BarDirection.BottomToTop:
                            topLeft = new Vector2(i * barWidth, drawSize.Y - barHeight);
                            break;
                    }

                    renderer.DrawQuad(
                        texture,
                        new Quad(
                            Vector2Extensions.Transform(topLeft, DrawInfo.Matrix),
                            Vector2Extensions.Transform(topLeft + new Vector2(barWidth, 0), DrawInfo.Matrix),
                            Vector2Extensions.Transform(topLeft + new Vector2(0, barHeight), DrawInfo.Matrix),
                            Vector2Extensions.Transform(topLeft + new Vector2(barWidth, barHeight), DrawInfo.Matrix)
                        ),
                        DrawColourInfo.Colour);
                }

                shader.Unbind();
            }
        }

        private class BarsInfo
        {
            public bool Any => Count > 0;

            public int Count { get; private set; }

            public float Breadth { get; set; }

            public List<float> InstantaneousLengths { get; } = new List<float>();

            private readonly List<float> initialLengths = new List<float>();
            private readonly List<float> finalLengths = new List<float>();

            public void UpdateLength(int index, float newLength)
            {
                initialLengths[index] = finalLengths[index];
                finalLengths[index] = newLength;
            }

            public void AddBar(float finalLength)
            {
                initialLengths.Add(0);
                finalLengths.Add(finalLength);
                InstantaneousLengths.Add(0);

                Count++;
            }

            public void Clear()
            {
                initialLengths.Clear();
                finalLengths.Clear();
                InstantaneousLengths.Clear();

                Count = 0;
            }

            public void RemoveRange(int index, int count)
            {
                initialLengths.RemoveRange(index, count);
                finalLengths.RemoveRange(index, count);
                InstantaneousLengths.RemoveRange(index, count);

                Count -= count;
            }

            public void Animate(double animationStartTime, double currentTime)
            {
                for (int i = 0; i < Count; i++)
                    InstantaneousLengths[i] = Interpolation.ValueAt(currentTime, initialLengths[i], finalLengths[i], animationStartTime, animationStartTime + resize_duration, easing);
            }

            public void FinishAnimation()
            {
                for (int i = 0; i < Count; i++)
                    InstantaneousLengths[i] = finalLengths[i];
            }
        }
    }
}

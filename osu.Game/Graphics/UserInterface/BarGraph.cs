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

        private readonly List<BarInfo> bars = new List<BarInfo>();

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

                float size = 1.0f / newCount;

                float maxLength = MaxValue ?? value.Max();

                foreach (var bar in value.Select((length, index) => (Value: length, Index: index)))
                {
                    float length = maxLength == 0 ? 0 : Math.Max(0f, bar.Value / maxLength);

                    if (bar.Index < bars.Count)
                    {
                        BarInfo b = bars[bar.Index];

                        b.InitialLength = b.FinalLength;
                        b.FinalLength = length;
                        b.Breadth = size;

                        bars[bar.Index] = b;
                    }
                    else
                    {
                        bars.Add(new BarInfo
                        {
                            FinalLength = length,
                            Breadth = size
                        });
                    }
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

            if (!bars.Any())
                return;

            double currentTime = Clock.CurrentTime;

            if (currentTime < animationStartTime + resize_duration)
            {
                for (int i = 0; i < bars.Count; i++)
                {
                    BarInfo bar = bars[i];
                    bar.InstantaneousLength = Interpolation.ValueAt(currentTime, bar.InitialLength, bar.FinalLength, animationStartTime, animationStartTime + resize_duration, easing);
                    bars[i] = bar;
                }

                Invalidate(Invalidation.DrawNode);
            }
            else if (!animationComplete)
            {
                for (int i = 0; i < bars.Count; i++)
                {
                    BarInfo bar = bars[i];
                    bar.InstantaneousLength = bar.FinalLength;
                    bars[i] = bar;
                }

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

            private readonly List<BarInfo> bars = new List<BarInfo>();

            public override void ApplyState()
            {
                base.ApplyState();

                shader = Source.shader;
                texture = Source.texture;
                drawSize = Source.DrawSize;
                direction = Source.direction;

                bars.Clear();
                bars.AddRange(Source.bars);
            }

            public override void Draw(IRenderer renderer)
            {
                base.Draw(renderer);

                if (!bars.Any())
                    return;

                shader.Bind();

                for (int i = 0; i < bars.Count; i++)
                {
                    var bar = bars[i];

                    float barHeight = drawSize.Y * ((direction == BarDirection.TopToBottom || direction == BarDirection.BottomToTop) ? bar.InstantaneousLength : bar.Breadth);
                    float barWidth = drawSize.X * ((direction == BarDirection.LeftToRight || direction == BarDirection.RightToLeft) ? bar.InstantaneousLength : bar.Breadth);

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

                    Vector2 topRight = topLeft + new Vector2(barWidth, 0);
                    Vector2 bottomLeft = topLeft + new Vector2(0, barHeight);
                    Vector2 bottomRight = bottomLeft + new Vector2(barWidth, 0);

                    var drawQuad = new Quad(
                        Vector2Extensions.Transform(topLeft, DrawInfo.Matrix),
                        Vector2Extensions.Transform(topRight, DrawInfo.Matrix),
                        Vector2Extensions.Transform(bottomLeft, DrawInfo.Matrix),
                        Vector2Extensions.Transform(bottomRight, DrawInfo.Matrix)
                    );

                    renderer.DrawQuad(texture, drawQuad, DrawColourInfo.Colour);
                }

                shader.Unbind();
            }
        }

        private struct BarInfo
        {
            public float InitialLength { get; set; }
            public float FinalLength { get; set; }
            public float InstantaneousLength { get; set; }
            public float Breadth { get; set; }
        }
    }
}

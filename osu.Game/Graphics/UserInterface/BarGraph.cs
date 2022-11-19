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

        private readonly List<BarStruct> bars = new List<BarStruct>();

        /// <summary>
        /// A list of floats that defines the length of each <see cref="Bar"/>
        /// </summary>
        public IEnumerable<float> Values
        {
            set
            {
                int newCount = value.Count();

                float size = newCount;
                if (size != 0)
                    size = 1.0f / size;

                float maxLength = MaxValue ?? value.Max();

                foreach (var bar in value.Select((length, index) => new { Value = length, Index = index }))
                {
                    float length = maxLength == 0 ? 0 : Math.Max(0f, bar.Value / maxLength);

                    if (bar.Index < bars.Count)
                    {
                        BarStruct b = bars[bar.Index];

                        b.OldValue = b.Value;
                        b.Value = length;
                        b.ShortSide = size;

                        bars[bar.Index] = b;
                    }
                    else
                    {
                        bars.Add(new BarStruct
                        {
                            Value = length,
                            ShortSide = size
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
                for (int i = 0; i < bars.Count(); i++)
                {
                    BarStruct bar = bars[i];
                    bar.IntermediateValue = Interpolation.ValueAt(currentTime, bar.OldValue, bar.Value, animationStartTime, animationStartTime + resize_duration, easing);
                    bars[i] = bar;
                }

                Invalidate(Invalidation.DrawNode);
            }
            else if (!animationComplete)
            {
                for (int i = 0; i < bars.Count(); i++)
                {
                    BarStruct bar = bars[i];
                    bar.IntermediateValue = bar.Value;
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

            private readonly List<BarStruct> bars = new List<BarStruct>();

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

                    float barHeight = drawSize.Y * ((direction == BarDirection.TopToBottom || direction == BarDirection.BottomToTop) ? bar.IntermediateValue : bar.ShortSide);
                    float barWidth = drawSize.X * ((direction == BarDirection.LeftToRight || direction == BarDirection.RightToLeft) ? bar.IntermediateValue : bar.ShortSide);

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

        private struct BarStruct
        {
            public float OldValue { get; set; }
            public float Value { get; set; }
            public float IntermediateValue { get; set; }
            public float ShortSide { get; set; }
        }
    }
}

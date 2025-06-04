// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapMetadataWedge
    {
        private partial class FailRetryDisplay : CompositeDrawable
        {
            private readonly GraphDrawable retriesGraph;
            private readonly GraphDrawable failsGraph;

            public APIFailTimes Data
            {
                set
                {
                    int[] retries = value.Retries ?? Array.Empty<int>();
                    int[] fails = value.Fails ?? Array.Empty<int>();
                    int[] total = retries.Zip(fails, (r, f) => r + f).ToArray();

                    int maximum = total.DefaultIfEmpty(0).Max();

                    retriesGraph.Data = total.Select(r => maximum == 0 ? 0 : (float)r / maximum).ToArray();
                    failsGraph.Data = fails.Select(r => maximum == 0 ? 0 : (float)r / maximum).ToArray();
                }
            }

            public FailRetryDisplay()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                InternalChild = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0f, 4f),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = BeatmapsetsStrings.ShowInfoPointsOfFailure,
                            Font = OsuFont.Style.Caption1.With(weight: FontWeight.SemiBold),
                            Margin = new MarginPadding { Bottom = 4f },
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 65f,
                            Children = new[]
                            {
                                retriesGraph = new GraphDrawable { RelativeSizeAxes = Axes.Both, Y = -1f },
                                failsGraph = new GraphDrawable { RelativeSizeAxes = Axes.Both },
                            },
                        },
                    },
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                retriesGraph.Colour = colours.Orange1;
                failsGraph.Colour = colours.DarkOrange2;
            }

            private partial class GraphDrawable : Drawable
            {
                private readonly float[] displayedData = new float[100];

                private float[] data = new float[100];

                public float[] Data
                {
                    get => data;
                    set
                    {
                        data = value;
                        Invalidate(Invalidation.DrawNode);
                    }
                }

                protected override void Update()
                {
                    base.Update();

                    bool changed = false;

                    for (int i = 0; i < displayedData.Length; i++)
                    {
                        float before = displayedData[i];
                        float value = data.ElementAtOrDefault(i);
                        displayedData[i] = (float)Interpolation.DampContinuously(displayedData[i], value, 40, Time.Elapsed);
                        changed |= displayedData[i] != before;
                    }

                    if (changed)
                        Invalidate(Invalidation.DrawNode);
                }

                protected override DrawNode CreateDrawNode() => new GraphDrawNode(this);

                // todo: consider integrating this with BarGraph
                // this is different from BarGraph since this displays each bar with corner radii applied.
                private class GraphDrawNode : DrawNode
                {
                    private readonly GraphDrawable source;

                    private Vector2 drawSize;
                    private float[] displayedData = null!;

                    public GraphDrawNode(GraphDrawable source)
                        : base(source)
                    {
                        this.source = source;
                    }

                    public override void ApplyState()
                    {
                        base.ApplyState();

                        drawSize = source.DrawSize;
                        displayedData = source.displayedData;
                    }

                    protected override void Draw(IRenderer renderer)
                    {
                        base.Draw(renderer);

                        const float spacing_constant = 1.5f;

                        float position = 0;
                        float barWidth = drawSize.X / displayedData.Length / spacing_constant;

                        float totalSpacing = drawSize.X - barWidth * displayedData.Length;
                        float spacing = totalSpacing / (displayedData.Length - 1);

                        for (int i = 0; i < displayedData.Length; i++)
                        {
                            float barHeight = MathF.Max(drawSize.Y * displayedData[i], barWidth);

                            drawBar(renderer, position, barWidth, barHeight);

                            position += barWidth + spacing;
                        }
                    }

                    private void drawBar(IRenderer renderer, float position, float width, float height)
                    {
                        float cornerRadius = width / 2f;

                        Vector3 scale = DrawInfo.MatrixInverse.ExtractScale();
                        float blendRange = (scale.X + scale.Y) / 2;

                        RectangleF drawRectangle = new RectangleF(new Vector2(position, drawSize.Y - height), new Vector2(width, height));
                        Quad screenSpaceDrawQuad = Quad.FromRectangle(drawRectangle) * DrawInfo.Matrix;

                        renderer.PushMaskingInfo(new MaskingInfo
                        {
                            ScreenSpaceAABB = screenSpaceDrawQuad.AABB,
                            MaskingRect = drawRectangle.Normalize(),
                            ConservativeScreenSpaceQuad = screenSpaceDrawQuad,
                            ToMaskingSpace = DrawInfo.MatrixInverse,
                            CornerRadius = cornerRadius,
                            CornerExponent = 2f,
                            // We are setting the linear blend range to the approximate size of a _pixel_ here.
                            // This results in the optimal trade-off between crispness and smoothness of the
                            // edges of the masked region according to sampling theory.
                            BlendRange = blendRange,
                            AlphaExponent = 1,
                        });

                        renderer.DrawQuad(renderer.WhitePixel, screenSpaceDrawQuad, DrawColourInfo.Colour);
                        renderer.PopMaskingInfo();
                    }
                }
            }
        }
    }
}

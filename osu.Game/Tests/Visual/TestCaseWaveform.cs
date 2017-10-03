// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES30;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual
{
    internal class TestCaseWaveform : OsuTestCase
    {
        private readonly Bindable<WorkingBeatmap> beatmapBacking = new Bindable<WorkingBeatmap>();

        private readonly List<WaveformDisplay> displays = new List<WaveformDisplay>();

        public TestCaseWaveform()
        {
            MusicController mc;
            FillFlowContainer flow;
            Child = flow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 10),
                Children = new Drawable[]
                {
                    mc = new MusicController
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Y = 100,
                        State = Visibility.Visible
                    },
                }
            };

            for (int i = 1; i <= 16; i *= 2)
            {
                var newDisplay = new WaveformDisplay(i) { RelativeSizeAxes = Axes.Both };

                displays.Add(newDisplay);

                flow.Add(new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 100,
                    Children = new Drawable[]
                    {
                        newDisplay,
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.Black,
                                    Alpha = 0.75f
                                },
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Text = $"Resolution: {(1f / i).ToString("0.00")}"
                                }
                            }
                        }
                    }
                });
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osuGame)
        {
            beatmapBacking.BindTo(osuGame.Beatmap);
            beatmapBacking.ValueChanged += b => b.Track.QueryWaveform(processWaveform);
        }

        private void processWaveform(Waveform waveform) => Schedule(() => displays.ForEach(d => d.Display(waveform)));

        private class WaveformDisplay : Drawable
        {
            private List<WaveformPoint> points;
            private int channels;

            private Shader shader;
            private readonly Texture texture;

            private readonly int resolution;

            public WaveformDisplay(int resolution)
            {
                this.resolution = resolution;

                texture = Texture.WhitePixel;
            }

            [BackgroundDependencyLoader]
            private void load(ShaderManager shaders)
            {
                shader = shaders?.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE_ROUNDED);
            }

            public void Display(Waveform waveform)
            {
                points = waveform.Generate((int)MathHelper.Clamp(Math.Ceiling(DrawWidth), 0, waveform.TotalPoints) / resolution);
                channels = waveform.Channels;
                Invalidate(Invalidation.DrawNode);
            }

            protected override DrawNode CreateDrawNode() => new WaveformDrawNode();

            private readonly WaveformDrawNodeSharedData sharedData = new WaveformDrawNodeSharedData();
            protected override void ApplyDrawNode(DrawNode node)
            {
                var n = (WaveformDrawNode)node;

                n.Shader = shader;
                n.Texture = texture;
                n.Points = points;
                n.Channels = channels;
                n.Size = DrawSize;
                n.Shared = sharedData;

                base.ApplyDrawNode(node);
            }

            private class WaveformDrawNodeSharedData
            {
                public readonly QuadBatch<TexturedVertex2D> VertexBatch = new QuadBatch<TexturedVertex2D>(1000, 10);
            }

            private class WaveformDrawNode : DrawNode
            {
                public Shader Shader;
                public Texture Texture;

                public WaveformDrawNodeSharedData Shared;

                public List<WaveformPoint> Points;
                public Vector2 Size;
                public int Channels;

                public override void Draw(Action<TexturedVertex2D> vertexAction)
                {
                    base.Draw(vertexAction);

                    if (Points == null || Points.Count == 0)
                        return;

                    Shader.Bind();
                    Texture.TextureGL.Bind();

                    float separation = Size.X / (Points.Count - 1);
                    Vector2 localInflationAmount = new Vector2(0, 1) * DrawInfo.MatrixInverse.ExtractScale().Xy;

                    for (int i = 0; i < Points.Count - 1; i++)
                    {
                        ColourInfo colour = DrawInfo.Colour;
                        Quad quadToDraw;

                        switch (Channels)
                        {
                            default:
                            case 2:
                                {
                                    float height = Size.Y / 2;
                                    quadToDraw = new Quad(
                                        new Vector2(i * separation, height - Points[i].Amplitude[0] * height),
                                        new Vector2((i + 1) * separation, height - Points[i + 1].Amplitude[0] * height),
                                        new Vector2(i * separation, height + Points[i].Amplitude[1] * height),
                                        new Vector2((i + 1) * separation, height + Points[i + 1].Amplitude[1] * height)
                                    );
                                }
                                break;
                            case 1:
                                {
                                    quadToDraw = new Quad(
                                        new Vector2(i * separation, Size.Y - Points[i].Amplitude[0] * Size.Y),
                                        new Vector2((i + 1) * separation, Size.Y - Points[i + 1].Amplitude[0] * Size.Y),
                                        new Vector2(i * separation, Size.Y),
                                        new Vector2((i + 1) * separation, Size.Y)
                                    );
                                    break;
                                }
                        }

                        Texture.DrawQuad(quadToDraw * DrawInfo.Matrix, colour, null, Shared.VertexBatch.Add, Vector2.Divide(localInflationAmount, quadToDraw.Size));
                    }

                    Shader.Unbind();
                }
            }
        }
    }
}

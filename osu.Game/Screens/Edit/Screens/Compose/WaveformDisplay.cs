// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;

namespace osu.Game.Screens.Edit.Screens.Compose
{
    public class WaveformDisplay : Drawable
    {
        /// <summary>
        /// The beatmap which the audio waveform should be displayed for.
        /// </summary>
        public readonly Bindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

        private Shader shader;
        private readonly Texture texture;

        public WaveformDisplay()
        {
            texture = Texture.WhitePixel;
            Beatmap.ValueChanged += generateWaveform;
        }

        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders)
        {
            shader = shaders?.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE_ROUNDED);
        }

        private float resolution = 1;
        /// <summary>
        /// Controls the amount of interpolation of the waveform into the width of this <see cref="WaveformDisplay"/>.
        /// Points in the waveform are interpolated between 1 / <see cref="Resolution"/> pixels of this <see cref="WaveformDisplay"/>.
        /// </summary>
        public float Resolution
        {
            get { return resolution; }
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentOutOfRangeException(nameof(value));

                if (resolution == value)
                    return;
                resolution = value;

                Invalidate(Invalidation.DrawNode);
            }
        }

        private Waveform waveform;
        private Track lastQueriedTrack;
        private void generateWaveform(WorkingBeatmap beatmap)
        {
            // Cancel the old query so we don't saturate the audio thread
            lastQueriedTrack?.CancelWaveformQuery();

            beatmap.Track.QueryWaveform(w =>
            {
                if (Beatmap.Value == beatmap)
                {
                    waveform = w;
                    Invalidate(Invalidation.DrawNode);
                }
            });

            lastQueriedTrack = beatmap.Track;
        }

        private readonly WaveformDrawNodeSharedData sharedData = new WaveformDrawNodeSharedData();
        protected override DrawNode CreateDrawNode() => new WaveformDrawNode();
        protected override void ApplyDrawNode(DrawNode node)
        {
            var n = (WaveformDrawNode)node;

            n.Shader = shader;
            n.Texture = texture;
            n.Size = DrawSize;
            n.Shared = sharedData;
            n.Points = waveform.Generate((int)(MathHelper.Clamp(Math.Ceiling(DrawWidth), 0, waveform.MaximumPoints) * resolution));
            n.Channels = waveform.Channels;

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

// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES30;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using System;

namespace osu.Game.Screens.Menu
{
    internal class LogoVisualisation : Drawable, IHasAccentColour
    {
        private readonly Bindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();
        private Color4 barColour;

        private const int index_change = 5;
        private const int bar_length = 1200;
        private const int bars_per_visualizer = 250;
        private const int visualizers = 5;

        /// <summary>
        /// How much should each bar go down each milisecond (based on a full bar)
        /// </summary>
        private const float decay_per_milisecond = 0.0024f;

        private int indexOffset;

        public Color4 AccentColour
        {
            get
            {
                return barColour;
            }
            set
            {
                //Half alpha makes it look really good!
                value.A = 0.5f;
                barColour = value;
            }
        }

        private readonly float[] frequencyAmplitudes = new float[256];

        public override bool HandleInput => false;

        private Shader shader;
        private readonly Texture texture;

        public LogoVisualisation()
        {
            texture = Texture.WhitePixel;
            AccentColour = Color4.White;
        }

        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders, OsuGame game)
        {
            beatmap.BindTo(game.Beatmap);
            shader = shaders?.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE_ROUNDED);
        }

        private void ensureAmplitudes()
        {
            float[] temporalAmplitudes = beatmap?.Value?.Track?.CurrentAmplitudes.FrequencyAmplitudes ?? new float[256];

            for (int i = 0; i < bars_per_visualizer; i++)
            {
                int index = (i + indexOffset) % bars_per_visualizer;
                if (beatmap?.Value?.Track?.IsRunning ?? false)
                {
                    if (temporalAmplitudes[index] > frequencyAmplitudes[i])
                        frequencyAmplitudes[i] = temporalAmplitudes[index];
                }
                else
                {
                    if (frequencyAmplitudes[(i + index_change) % bars_per_visualizer] > frequencyAmplitudes[i])
                        frequencyAmplitudes[i] = frequencyAmplitudes[(i + index_change) % bars_per_visualizer];
                }
            }
            indexOffset = (indexOffset + index_change) % bars_per_visualizer;
            Scheduler.AddDelayed(ensureAmplitudes, 50);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            ensureAmplitudes();
        }

        protected override void Update()
        {
            base.Update();

            float decayFactor = (float)Time.Elapsed * decay_per_milisecond;
            for (int i = 0; i < bars_per_visualizer; i++)
            {
                //0.03% of extra bar length to make it a little faster when bar is almost at it's minimum
                frequencyAmplitudes[i] -= decayFactor * (frequencyAmplitudes[i] + 0.03f);
                if (frequencyAmplitudes[i] < 0)
                    frequencyAmplitudes[i] = 0;
            }

            Invalidate(Invalidation.DrawNode, shallPropagate: false);
        }

        protected override DrawNode CreateDrawNode() => new VisualisationDrawNode();

        private readonly VisualizerSharedData sharedData = new VisualizerSharedData();
        protected override void ApplyDrawNode(DrawNode node)
        {
            base.ApplyDrawNode(node);

            var visNode = (VisualisationDrawNode)node;

            visNode.Shader = shader;
            visNode.Texture = texture;
            visNode.Size = DrawSize.X;
            visNode.Shared = sharedData;
            visNode.Colour = barColour;
            visNode.AudioData = frequencyAmplitudes;
        }

        private class VisualizerSharedData
        {
            public readonly LinearBatch<TexturedVertex2D> VertexBatch = new LinearBatch<TexturedVertex2D>(100 * 4, 10, PrimitiveType.Quads);
        }

        private class VisualisationDrawNode : DrawNode
        {
            public Shader Shader;
            public Texture Texture;
            public VisualizerSharedData Shared;
            //Asuming the logo is a circle, we don't need a second dimension.
            public float Size;

            public Color4 Colour;
            public float[] AudioData;

            public override void Draw(Action<TexturedVertex2D> vertexAction)
            {
                base.Draw(vertexAction);

                Shader.Bind();
                Texture.TextureGL.Bind();

                Vector2 inflation = DrawInfo.MatrixInverse.ExtractScale().Xy;

                ColourInfo colourInfo = DrawInfo.Colour;
                colourInfo.ApplyChild(Colour);

                if (AudioData != null)
                {
                    for (int i = 0; i < bars_per_visualizer * visualizers; i++)
                    {
                        float rotation = MathHelper.DegreesToRadians(i / (float)bars_per_visualizer * 360 + i / bars_per_visualizer * (360 / visualizers));
                        float rotationCos = (float)Math.Cos(rotation);
                        float rotationSin = (float)Math.Sin(rotation);
                        //taking the cos and sin to the 0..1 range
                        var barPosition = new Vector2(rotationCos / 2 + 0.5f, rotationSin / 2 + 0.5f) * Size;

                        var barSize = new Vector2(Size * (float)Math.Sqrt(2 * (1 - Math.Cos(MathHelper.DegreesToRadians(360f / bars_per_visualizer)))) / 2f, bar_length * AudioData[i % bars_per_visualizer]);
                        //The distance between the position and the sides of the bar.
                        var bottomOffset = new Vector2(-rotationSin * barSize.X / 2, rotationCos * barSize.X / 2);
                        //The distance between the bottom side of the bar and the top side.
                        var amplitudeOffset = new Vector2(rotationCos * barSize.Y, rotationSin * barSize.Y);

                        var rectangle = new Quad(
                            (barPosition - bottomOffset) * DrawInfo.Matrix,
                            (barPosition - bottomOffset + amplitudeOffset) * DrawInfo.Matrix,
                            (barPosition + bottomOffset) * DrawInfo.Matrix,
                            (barPosition + bottomOffset + amplitudeOffset) * DrawInfo.Matrix
                        );

                        Texture.DrawQuad(
                            rectangle,
                            colourInfo,
                            null,
                            Shared.VertexBatch.Add,
                            //barSize by itself will make it smooth more in the X axis than in the Y axis, this reverts that.
                            Vector2.Divide(inflation, barSize.Yx));
                    }
                }
                Shader.Unbind();
            }
        }
    }
}

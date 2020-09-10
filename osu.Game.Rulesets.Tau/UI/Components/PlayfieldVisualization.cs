using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osuTK;
using osuTK.Graphics;
using System.Diagnostics;

namespace osu.Game.Rulesets.Tau.UI.Components
{
    /// <summary>
    /// A LogoVisualisation modified to better work with gameplay
    /// </summary>
    public class PlayfieldVisualisation : Drawable, IHasAccentColour
    {
        private readonly IBindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

        /// <summary>
        /// The number of bars to jump each update iteration.
        /// </summary>
        private const int indexChange = 5;

        /// <summary>
        /// The maximum length of each bar in the visualiser. Will be reduced when kiai is not activated.
        /// </summary>
        private const float barLength = 600;

        /// <summary>
        /// The number of bars in one rotation of the visualiser.
        /// </summary>
        private const int barsPerVisualiser = 200;

        /// <summary>
        /// How many times we should stretch around the circumference (overlapping overselves).
        /// </summary>
        private const float visualiserRounds = 5;

        /// <summary>
        /// How much should each bar go down each millisecond (based on a full bar).
        /// </summary>
        private const float decayPerMilisecond = 0.0024f;

        /// <summary>
        /// Number of milliseconds between each amplitude update.
        /// </summary>
        private const float timeBetweenUpdates = 50;

        /// <summary>
        /// The minimum amplitude to show a bar.
        /// </summary>
        private const float amplitudeDeadZone = 1f / barLength;

        private int indexOffset;

        public Color4 AccentColour { get; set; }

        private readonly float[] frequencyAmplitudes = new float[256];

        private IShader shader;
        private readonly Texture texture;

        public PlayfieldVisualisation()
        {
            texture = Texture.WhitePixel;
            Blending = BlendingParameters.Additive;
        }

        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders, IBindable<WorkingBeatmap> beatmap)
        {
            this.beatmap.BindTo(beatmap);
            shader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE_ROUNDED);
        }

        private readonly float[] temporalAmplitudes = new float[ChannelAmplitudes.AMPLITUDES_SIZE];

        private void updateAmplitudes()
        {
            var track = beatmap.Value.TrackLoaded ? beatmap.Value.Track : null;
            var effect = beatmap.Value.BeatmapLoaded ? beatmap.Value.Beatmap?.ControlPointInfo.EffectPointAt(track?.CurrentTime ?? Time.Current) : null;

            if (!effect?.KiaiMode ?? false)
                return;

            var amplitudes = (track?.CurrentAmplitudes ?? ChannelAmplitudes.Empty).FrequencyAmplitudes.Span;

            for (int i = 0; i < barsPerVisualiser; i++)
            {
                float targetAmplitude = amplitudes[(i + indexOffset) % barsPerVisualiser];

                if (targetAmplitude > frequencyAmplitudes[i])
                    frequencyAmplitudes[i] = targetAmplitude;
            }

            indexOffset = (indexOffset + indexChange) % barsPerVisualiser;
        }

        private double lastUpdateTime = double.MinValue;

        protected override void Update()
        {
            base.Update();

            if (Math.Abs(lastUpdateTime - Time.Current) > 50){
                updateAmplitudes();
                lastUpdateTime = Time.Current;
            }

            float decayFactor = (float)Math.Abs(Time.Elapsed) * decayPerMilisecond;


            for (int i = 0; i < barsPerVisualiser; i++)
            {
                //3% of extra bar length to make it a little faster when bar is almost at it's minimum
                frequencyAmplitudes[i] -= decayFactor * (frequencyAmplitudes[i] + 0.03f);

                if (frequencyAmplitudes[i] < 0)
                    frequencyAmplitudes[i] = 0;
            }

            Invalidate(Invalidation.DrawNode);
        }

        protected override DrawNode CreateDrawNode() => new VisualisationDrawNode(this);

        private class VisualisationDrawNode : DrawNode
        {
            protected new PlayfieldVisualisation Source => (PlayfieldVisualisation)base.Source;

            private IShader shader;
            private Texture texture;

            // Assuming the logo is a circle, we don't need a second dimension.
            private float size;

            private Color4 colour;
            private float[] audioData;

            private readonly QuadBatch<TexturedVertex2D> vertexBatch = new QuadBatch<TexturedVertex2D>(100, 10);

            public VisualisationDrawNode(PlayfieldVisualisation source)
                : base(source)
            {
            }

            public override void ApplyState()
            {
                base.ApplyState();

                shader = Source.shader;
                texture = Source.texture;
                size = Source.DrawSize.X;
                colour = Source.AccentColour;
                audioData = Source.frequencyAmplitudes;
            }

            public override void Draw(Action<TexturedVertex2D> vertexAction)
            {
                base.Draw(vertexAction);

                shader.Bind();

                Vector2 inflation = DrawInfo.MatrixInverse.ExtractScale().Xy;

                ColourInfo colourInfo = DrawColourInfo.Colour;
                colourInfo.ApplyChild(colour);

                if (audioData != null)
                {
                    for (int j = 0; j < visualiserRounds; j++)
                    {
                        for (int i = 0; i < barsPerVisualiser; i++)
                        {
                            if (audioData[i] < amplitudeDeadZone)
                                continue;

                            float rotation = MathUtils.DegreesToRadians((i / (float)barsPerVisualiser * 360) + (j * 360 / visualiserRounds));
                            float rotationCos = MathF.Cos(rotation);
                            float rotationSin = MathF.Sin(rotation);
                            // taking the cos and sin to the 0..1 range
                            var barPosition = new Vector2((rotationCos / 2) + 0.5f, (rotationSin / 2) + 0.5f) * size;

                            var barSize = new Vector2(size * MathF.Sqrt(2 * (1 - MathF.Cos(MathUtils.DegreesToRadians(360f / barsPerVisualiser)))) / 2f, barLength * audioData[i]);
                            // The distance between the position and the sides of the bar.
                            var bottomOffset = new Vector2(-rotationSin * barSize.X / 2, rotationCos * barSize.X / 2);
                            // The distance between the bottom side of the bar and the top side.
                            var amplitudeOffset = new Vector2(rotationCos * barSize.Y, rotationSin * barSize.Y);

                            var rectangle = new Quad(
                                Vector2Extensions.Transform(barPosition - bottomOffset, DrawInfo.Matrix),
                                Vector2Extensions.Transform(barPosition - bottomOffset + amplitudeOffset, DrawInfo.Matrix),
                                Vector2Extensions.Transform(barPosition + bottomOffset, DrawInfo.Matrix),
                                Vector2Extensions.Transform(barPosition + bottomOffset + amplitudeOffset, DrawInfo.Matrix)
                            );

                            DrawQuad(
                                texture,
                                rectangle,
                                colourInfo,
                                null,
                                vertexBatch.AddAction,
                                // barSize by itself will make it smooth more in the X axis than in the Y axis, this reverts that.
                                Vector2.Divide(inflation, barSize.Yx));
                        }
                    }
                }

                shader.Unbind();
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);

                vertexBatch.Dispose();
            }
        }
    }
}

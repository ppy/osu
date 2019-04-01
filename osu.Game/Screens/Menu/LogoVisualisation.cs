// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osuTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Skinning;
using osu.Game.Online.API;
using osu.Game.Users;
using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;

namespace osu.Game.Screens.Menu
{
    public class LogoVisualisation : Drawable, IHasAccentColour
    {
        private readonly IBindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

        /// <summary>
        /// The number of bars to jump each update iteration.
        /// </summary>
        private const int index_change = 5;

        /// <summary>
        /// The maximum length of each bar in the visualiser. Will be reduced when kiai is not activated.
        /// </summary>
        private const float bar_length = 600;

        /// <summary>
        /// The number of bars in one rotation of the visualiser.
        /// </summary>
        private const int bars_per_visualiser = 200;

        /// <summary>
        /// How many times we should stretch around the circumference (overlapping overselves).
        /// </summary>
        private const float visualiser_rounds = 5;

        /// <summary>
        /// How much should each bar go down each milisecond (based on a full bar).
        /// </summary>
        private const float decay_per_milisecond = 0.0024f;

        /// <summary>
        /// Number of milliseconds between each amplitude update.
        /// </summary>
        private const float time_between_updates = 50;

        /// <summary>
        /// The minimum amplitude to show a bar.
        /// </summary>
        private const float amplitude_dead_zone = 1f / bar_length;

        private int indexOffset;

        public Color4 AccentColour { get; set; }

        private readonly float[] frequencyAmplitudes = new float[256];

        private IShader shader;
        private readonly Texture texture;

        private Bindable<User> user;
        private Bindable<Skin> skin;

        public LogoVisualisation()
        {
            texture = Texture.WhitePixel;
            Blending = BlendingMode.Additive;
        }

        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders, IBindable<WorkingBeatmap> beatmap, IAPIProvider api, SkinManager skinManager)
        {
            this.beatmap.BindTo(beatmap);
            shader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE_ROUNDED);
            user = api.LocalUser.GetBoundCopy();
            skin = skinManager.CurrentSkin.GetBoundCopy();

            user.ValueChanged += _ => updateColour();
            skin.BindValueChanged(_ => updateColour(), true);
        }

        private void updateAmplitudes()
        {
            var track = beatmap.Value.TrackLoaded ? beatmap.Value.Track : null;
            var effect = beatmap.Value.BeatmapLoaded ? beatmap.Value.Beatmap.ControlPointInfo.EffectPointAt(track?.CurrentTime ?? Time.Current) : null;

            float[] temporalAmplitudes = track?.CurrentAmplitudes.FrequencyAmplitudes ?? new float[256];

            for (int i = 0; i < bars_per_visualiser; i++)
            {
                if (track?.IsRunning ?? false)
                {
                    float targetAmplitude = temporalAmplitudes[(i + indexOffset) % bars_per_visualiser] * (effect?.KiaiMode == true ? 1 : 0.5f);
                    if (targetAmplitude > frequencyAmplitudes[i])
                        frequencyAmplitudes[i] = targetAmplitude;
                }
                else
                {
                    int index = (i + index_change) % bars_per_visualiser;
                    if (frequencyAmplitudes[index] > frequencyAmplitudes[i])
                        frequencyAmplitudes[i] = frequencyAmplitudes[index];
                }
            }

            indexOffset = (indexOffset + index_change) % bars_per_visualiser;
            Scheduler.AddDelayed(updateAmplitudes, time_between_updates);
        }

        private void updateColour()
        {
            Color4 defaultColour = Color4.White.Opacity(0.2f);

            if (user.Value?.IsSupporter ?? false)
                AccentColour = skin.Value.GetValue<SkinConfiguration, Color4?>(s => s.CustomColours.ContainsKey("MenuGlow") ? s.CustomColours["MenuGlow"] : (Color4?)null) ?? defaultColour;
            else
                AccentColour = defaultColour;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateAmplitudes();
        }

        protected override void Update()
        {
            base.Update();

            float decayFactor = (float)Time.Elapsed * decay_per_milisecond;
            for (int i = 0; i < bars_per_visualiser; i++)
            {
                //3% of extra bar length to make it a little faster when bar is almost at it's minimum
                frequencyAmplitudes[i] -= decayFactor * (frequencyAmplitudes[i] + 0.03f);
                if (frequencyAmplitudes[i] < 0)
                    frequencyAmplitudes[i] = 0;
            }

            Invalidate(Invalidation.DrawNode, shallPropagate: false);
        }

        protected override DrawNode CreateDrawNode() => new VisualisationDrawNode();

        protected override void ApplyDrawNode(DrawNode node)
        {
            base.ApplyDrawNode(node);

            var visNode = (VisualisationDrawNode)node;

            visNode.Shader = shader;
            visNode.Texture = texture;
            visNode.Size = DrawSize.X;
            visNode.Colour = AccentColour;
            visNode.AudioData = frequencyAmplitudes;
        }

        private class VisualisationDrawNode : DrawNode
        {
            public IShader Shader;
            public Texture Texture;

            //Asuming the logo is a circle, we don't need a second dimension.
            public float Size;

            public Color4 Colour;
            public float[] AudioData;

            private readonly QuadBatch<TexturedVertex2D> vertexBatch = new QuadBatch<TexturedVertex2D>(100, 10);

            public override void Draw(Action<TexturedVertex2D> vertexAction)
            {
                base.Draw(vertexAction);

                Shader.Bind();
                Texture.TextureGL.Bind();

                Vector2 inflation = DrawInfo.MatrixInverse.ExtractScale().Xy;

                ColourInfo colourInfo = DrawColourInfo.Colour;
                colourInfo.ApplyChild(Colour);

                if (AudioData != null)
                {
                    for (int j = 0; j < visualiser_rounds; j++)
                    {
                        for (int i = 0; i < bars_per_visualiser; i++)
                        {
                            if (AudioData[i] < amplitude_dead_zone)
                                continue;

                            float rotation = MathHelper.DegreesToRadians(i / (float)bars_per_visualiser * 360 + j * 360 / visualiser_rounds);
                            float rotationCos = (float)Math.Cos(rotation);
                            float rotationSin = (float)Math.Sin(rotation);
                            //taking the cos and sin to the 0..1 range
                            var barPosition = new Vector2(rotationCos / 2 + 0.5f, rotationSin / 2 + 0.5f) * Size;

                            var barSize = new Vector2(Size * (float)Math.Sqrt(2 * (1 - Math.Cos(MathHelper.DegreesToRadians(360f / bars_per_visualiser)))) / 2f, bar_length * AudioData[i]);
                            //The distance between the position and the sides of the bar.
                            var bottomOffset = new Vector2(-rotationSin * barSize.X / 2, rotationCos * barSize.X / 2);
                            //The distance between the bottom side of the bar and the top side.
                            var amplitudeOffset = new Vector2(rotationCos * barSize.Y, rotationSin * barSize.Y);

                            var rectangle = new Quad(
                                Vector2Extensions.Transform(barPosition - bottomOffset, DrawInfo.Matrix),
                                Vector2Extensions.Transform(barPosition - bottomOffset + amplitudeOffset, DrawInfo.Matrix),
                                Vector2Extensions.Transform(barPosition + bottomOffset, DrawInfo.Matrix),
                                Vector2Extensions.Transform(barPosition + bottomOffset + amplitudeOffset, DrawInfo.Matrix)
                            );

                            Texture.DrawQuad(
                                rectangle,
                                colourInfo,
                                null,
                                vertexBatch.AddAction,
                                //barSize by itself will make it smooth more in the X axis than in the Y axis, this reverts that.
                                Vector2.Divide(inflation, barSize.Yx));
                        }
                    }
                }

                Shader.Unbind();
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);

                vertexBatch.Dispose();
            }
        }
    }
}

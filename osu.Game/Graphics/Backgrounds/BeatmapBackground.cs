// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable


using System.Runtime.InteropServices;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shaders.Types;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.Backgrounds
{
    /// <summary>
    /// A background which offers dimming using a custom shader with ability to change dim colour.
    /// </summary>
    public partial class BeatmapBackground : Background
    {
        public readonly WorkingBeatmap Beatmap;

        private readonly string fallbackTextureName;

        private float dimLevel;
        private Color4 dimColour;

        public float DimLevel
        {
            get => dimLevel;
            set
            {
                dimLevel = value;

                if (BufferedContainer == null)
                {
                    (Sprite as BeatmapBackgroundSprite).DimLevel = dimLevel;
                }
                else
                {
                    (BufferedContainer as DimmableBufferedContainer).DimLevel = dimLevel;
                }
            }
        }

        public Color4 DimColour
        {
            get => dimColour;
            set
            {
                dimColour = value;

                if (BufferedContainer == null)
                {
                    (Sprite as BeatmapBackgroundSprite).DimColour = dimColour;
                }
                else
                {
                    (BufferedContainer as DimmableBufferedContainer).DimColour = dimColour;
                }
            }
        }

        public BeatmapBackground(WorkingBeatmap beatmap, string fallbackTextureName = @"Backgrounds/bg1")
        {
            Beatmap = beatmap;
            this.fallbackTextureName = fallbackTextureName;

            Sprite = new BeatmapBackgroundSprite
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                FillMode = FillMode.Fill,
            };

            DimLevel = 0.0f;
            DimColour = Color4.Black;
        }

        [BackgroundDependencyLoader]
        private void load(LargeTextureStore textures)
        {
            Sprite.Texture = Beatmap?.GetBackground() ?? textures.Get(fallbackTextureName);
        }

        public override bool Equals(Background other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return other.GetType() == GetType()
                   && ((BeatmapBackground)other).Beatmap == Beatmap;
        }

        public override void BlurTo(Vector2 newBlurSigma, double duration = 0, Easing easing = Easing.None)
        {
            if (BufferedContainer == null && newBlurSigma != Vector2.Zero)
            {
                RemoveInternal(Sprite, false);

                BeatmapBackgroundSprite sprite = (BeatmapBackgroundSprite)Sprite;
                sprite.DimColour = Color4.Black;
                sprite.DimLevel = 0.0f;

                AddInternal(BufferedContainer = new DimmableBufferedContainer(cachedFrameBuffer: true)
                {
                    RelativeSizeAxes = Axes.Both,
                    RedrawOnScale = false,
                    Child = Sprite,
                    DimColour = DimColour,
                    DimLevel = DimLevel
                });
            }
            base.BlurTo(newBlurSigma, duration, easing);
        }

        public partial class DimmableBufferedContainer : BufferedContainer
        {
            private float dimLevel;
            private Color4 dimColour;

            public float DimLevel
            {
                get => dimLevel;
                set
                {
                    dimLevel = value;
                    Invalidate(Invalidation.DrawNode);
                }
            }

            public Color4 DimColour
            {
                get => dimColour;
                set
                {
                    dimColour = value;
                    Invalidate(Invalidation.DrawNode);
                }
            }

            public DimmableBufferedContainer(RenderBufferFormat[] formats = null, bool pixelSnapping = false, bool cachedFrameBuffer = false)
            : base(formats, pixelSnapping, cachedFrameBuffer)
            {
                DimLevel = 0.0f;
                DimColour = Color4.Black;
            }

            [BackgroundDependencyLoader]
            private void load(ShaderManager shaders)
            {
                TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "BeatmapBackground");
            }

            protected override DrawNode CreateDrawNode() => new DimmableBufferedContainerDrawNode(this, SharedData);

            protected class DimmableBufferedContainerDrawNode : BufferedContainerDrawNode
            {
                public new DimmableBufferedContainer Source => (DimmableBufferedContainer)base.Source;

                public DimmableBufferedContainerDrawNode(DimmableBufferedContainer source, BufferedContainerDrawNodeSharedData sharedData)
                : base(source, sharedData)
                {
                }

                private float dimLevel;
                private Color4 dimColour;

                public override void ApplyState()
                {
                    base.ApplyState();

                    dimLevel = Source.DimLevel;
                    dimColour = Source.DimColour;
                }

                private IUniformBuffer<BeatmapBackgroundParameters> beatmapBackgroundParametersBuffer;

                protected override void BindUniformResources(IShader shader, IRenderer renderer)
                {
                    beatmapBackgroundParametersBuffer ??= renderer.CreateUniformBuffer<BeatmapBackgroundParameters>();

                    beatmapBackgroundParametersBuffer.Data = beatmapBackgroundParametersBuffer.Data with
                    {
                        DimColour = new Vector4(dimColour.R, dimColour.G, dimColour.B, dimColour.A),
                        DimLevel = dimLevel,
                    };

                    shader.BindUniformBlock("m_BeatmapBackgroundParameters", beatmapBackgroundParametersBuffer);
                }

                [StructLayout(LayoutKind.Sequential, Pack = 1)]
                private record struct BeatmapBackgroundParameters
                {
                    public UniformVector4 DimColour;
                    public UniformFloat DimLevel;
                    private readonly UniformPadding12 pad1;
                }
            }
        }

        public partial class BeatmapBackgroundSprite : Sprite
        {
            private float dimLevel;
            private Color4 dimColour;

            public float DimLevel
            {
                get => dimLevel;
                set
                {
                    dimLevel = value;
                    Invalidate(Invalidation.DrawNode);
                }
            }

            public Color4 DimColour
            {
                get => dimColour;
                set
                {
                    dimColour = value;
                    Invalidate(Invalidation.DrawNode);
                }
            }

            [BackgroundDependencyLoader]
            private void load(ShaderManager shaders)
            {
                TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "BeatmapBackground");
            }

            protected override DrawNode CreateDrawNode() => new BeatmapBackgroundSpriteDrawNode(this);

            public class BeatmapBackgroundSpriteDrawNode : SpriteDrawNode
            {
                public new BeatmapBackgroundSprite Source => (BeatmapBackgroundSprite)base.Source;

                public BeatmapBackgroundSpriteDrawNode(BeatmapBackgroundSprite source)
                    : base(source)
                {
                }

                private float dimLevel;
                private Color4 dimColour;

                public override void ApplyState()
                {
                    base.ApplyState();

                    dimLevel = Source.DimLevel;
                    dimColour = Source.DimColour;
                }

                private IUniformBuffer<BeatmapBackgroundParameters> beatmapBackgroundParametersBuffer;

                protected override void BindUniformResources(IShader shader, IRenderer renderer)
                {
                    beatmapBackgroundParametersBuffer ??= renderer.CreateUniformBuffer<BeatmapBackgroundParameters>();

                    beatmapBackgroundParametersBuffer.Data = beatmapBackgroundParametersBuffer.Data with
                    {
                        DimColour = new Vector4(dimColour.R, dimColour.G, dimColour.B, dimColour.A),
                        DimLevel = dimLevel,
                    };

                    shader.BindUniformBlock("m_BeatmapBackgroundParameters", beatmapBackgroundParametersBuffer);
                }

                [StructLayout(LayoutKind.Sequential, Pack = 1)]
                private record struct BeatmapBackgroundParameters
                {
                    public UniformVector4 DimColour;
                    public UniformFloat DimLevel;
                    private readonly UniformPadding12 pad1;
                }
            }
        }
    }
}

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

namespace osu.Game.Graphics.Backgrounds
{
    /// <summary>
    /// A background which offers dimming using a custom shader with ability to change dim colour.
    /// </summary>
    public partial class BeatmapBackground : Background
    {
        public readonly WorkingBeatmap Beatmap;

        private readonly string fallbackTextureName;

        protected DimmableSprite ColouredDimmableSprite { get; private set; }

        protected DimmableBufferedContainer ColouredDimmableBufferedContainer;

        private Colour4 dimColour;

        public Colour4 DimColour
        {
            get => dimColour;
            set => ColouredDimmable.DimColour = dimColour = value;
        }

        /// <summary>
        /// A drawable that currently handles dimming.
        /// </summary>
        protected IColouredDimmable ColouredDimmable => ColouredDimmableBufferedContainer != null ? ColouredDimmableBufferedContainer : ColouredDimmableSprite;

        public BeatmapBackground(WorkingBeatmap beatmap, string fallbackTextureName = @"Backgrounds/bg1")
        {
            Beatmap = beatmap;
            this.fallbackTextureName = fallbackTextureName;

            DimColour = Colour4.Black;
        }

        [BackgroundDependencyLoader]
        private void load(LargeTextureStore textures)
        {
            Sprite.Texture = Beatmap?.GetBackground() ?? textures.Get(fallbackTextureName);
        }

        protected override Sprite CreateSprite() => ColouredDimmableSprite = new DimmableSprite
        {
            RelativeSizeAxes = Axes.Both,
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            FillMode = FillMode.Fill,
        };

        protected override BufferedContainer CreateBufferedContainer()
        {
            ColouredDimmableSprite.DimColour = Colour4.Black;

            return ColouredDimmableBufferedContainer = new DimmableBufferedContainer(cachedFrameBuffer: true)
            {
                RelativeSizeAxes = Axes.Both,
                RedrawOnScale = false,
                Child = Sprite,
                DimColour = DimColour,
            };
        }

        public override bool Equals(Background other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return other.GetType() == GetType()
                   && ((BeatmapBackground)other).Beatmap == Beatmap;
        }

        public interface IColouredDimmable : IDrawable
        {
            Colour4 DimColour { get; set; }
        }

        public partial class DimmableBufferedContainer : BufferedContainer, IColouredDimmable
        {
            private Colour4 dimColour;

            public Colour4 DimColour
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
                DimColour = Colour4.Black;
            }

            [BackgroundDependencyLoader]
            private void load(ShaderManager shaders)
            {
                TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "ColouredDimmableTexture");
            }

            protected override DrawNode CreateDrawNode() => new DimmableBufferedContainerDrawNode(this, SharedData);

            protected class DimmableBufferedContainerDrawNode : BufferedContainerDrawNode
            {
                public new DimmableBufferedContainer Source => (DimmableBufferedContainer)base.Source;

                public DimmableBufferedContainerDrawNode(DimmableBufferedContainer source, BufferedContainerDrawNodeSharedData sharedData)
                    : base(source, sharedData)
                {
                }

                private Colour4 dimColour;

                public override void ApplyState()
                {
                    base.ApplyState();

                    dimColour = Source.DimColour;
                }

                private IUniformBuffer<DimParameters> dimParametersBuffer;

                protected override void BindUniformResources(IShader shader, IRenderer renderer)
                {
                    dimParametersBuffer ??= renderer.CreateUniformBuffer<DimParameters>();

                    dimParametersBuffer.Data = dimParametersBuffer.Data with
                    {
                        DimColour = new UniformVector4
                        {
                            X = dimColour.R,
                            Y = dimColour.G,
                            Z = dimColour.B,
                            W = dimColour.A
                        },
                    };

                    shader.BindUniformBlock("m_DimParameters", dimParametersBuffer);
                }

                protected override void Dispose(bool isDisposing)
                {
                    base.Dispose(isDisposing);
                    dimParametersBuffer?.Dispose();
                }

                [StructLayout(LayoutKind.Sequential, Pack = 1)]
                private record struct DimParameters
                {
                    public UniformVector4 DimColour;
                }
            }
        }

        public partial class DimmableSprite : Sprite, IColouredDimmable
        {
            private Colour4 dimColour;

            public Colour4 DimColour
            {
                get => dimColour;
                set
                {
                    dimColour = value;
                    Invalidate(Invalidation.DrawNode);
                }
            }

            public DimmableSprite()
            {
                DimColour = Colour4.Black;
            }

            [BackgroundDependencyLoader]
            private void load(ShaderManager shaders)
            {
                TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "ColouredDimmableTexture");
            }

            protected override DrawNode CreateDrawNode() => new BeatmapBackgroundSpriteDrawNode(this);

            public class BeatmapBackgroundSpriteDrawNode : SpriteDrawNode
            {
                public new DimmableSprite Source => (DimmableSprite)base.Source;

                public BeatmapBackgroundSpriteDrawNode(DimmableSprite source)
                    : base(source)
                {
                }

                private Colour4 dimColour;

                public override void ApplyState()
                {
                    base.ApplyState();

                    dimColour = Source.DimColour;
                }

                private IUniformBuffer<DimParameters> dimParametersBuffer;

                protected override void BindUniformResources(IShader shader, IRenderer renderer)
                {
                    dimParametersBuffer ??= renderer.CreateUniformBuffer<DimParameters>();

                    dimParametersBuffer.Data = dimParametersBuffer.Data with
                    {
                        DimColour = new UniformVector4
                        {
                            X = dimColour.R,
                            Y = dimColour.G,
                            Z = dimColour.B,
                            W = dimColour.A
                        },
                    };

                    shader.BindUniformBlock("m_DimParameters", dimParametersBuffer);
                }

                protected override void Dispose(bool isDisposing)
                {
                    base.Dispose(isDisposing);
                    dimParametersBuffer?.Dispose();
                }

                [StructLayout(LayoutKind.Sequential, Pack = 1)]
                private record struct DimParameters
                {
                    public UniformVector4 DimColour;
                }
            }
        }
    }
}

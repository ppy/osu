// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Runtime.InteropServices;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shaders.Types;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.Backgrounds
{
    /// <summary>
    /// A background which offers dimming using a custom shader with ability to change dim colour.
    /// </summary>
    public partial class DimmableBeatmapBackground : BeatmapBackground
    {
        private float dimLevel;
        private Color4 dimColour;

        public float DimLevel
        {
            get => dimLevel;
            set
            {
                dimLevel = value;
                DimmableBeatmapBackgroundSprite sprite = (DimmableBeatmapBackgroundSprite)Sprite;
                sprite.DimLevel = dimLevel;

                BufferedContainer?.ForceRedraw();
            }
        }

        public Color4 DimColour
        {
            get => dimColour;
            set
            {
                dimColour = value;
                DimmableBeatmapBackgroundSprite sprite = (DimmableBeatmapBackgroundSprite)Sprite;
                sprite.DimColour = dimColour;

                BufferedContainer?.ForceRedraw();
            }
        }

        public DimmableBeatmapBackground(WorkingBeatmap beatmap, string fallbackTextureName = @"Backgrounds/bg1") : base(beatmap, fallbackTextureName)
        {
            Sprite = new DimmableBeatmapBackgroundSprite
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                FillMode = FillMode.Fill,
            };
        }

        public partial class DimmableBeatmapBackgroundSprite : Sprite
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
                TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "DimmableBeatmapBackground");
            }

            protected override DrawNode CreateDrawNode() => new DimmableBeatmapBackgroundSpriteDrawNode(this);

            public class DimmableBeatmapBackgroundSpriteDrawNode : SpriteDrawNode
            {
                public new DimmableBeatmapBackgroundSprite Source => (DimmableBeatmapBackgroundSprite)base.Source;

                public DimmableBeatmapBackgroundSpriteDrawNode(DimmableBeatmapBackgroundSprite source)
                    : base(source)
                {
                }

                private float dimLevel;
                private Color4 dimColour;

                private IShader textureShader;

                public override void ApplyState()
                {
                    base.ApplyState();

                    dimLevel = Source.DimLevel;
                    dimColour = Source.DimColour;

                    textureShader = Source.TextureShader;
                }

                private IUniformBuffer<DimmableBeatmapBackgroundParameters> dimmableBeatmapBackgroundParametersBuffer;

                private void bindParametersBuffer(IRenderer renderer)
                {
                    dimmableBeatmapBackgroundParametersBuffer ??= renderer.CreateUniformBuffer<DimmableBeatmapBackgroundParameters>();

                    dimmableBeatmapBackgroundParametersBuffer.Data = dimmableBeatmapBackgroundParametersBuffer.Data with
                    {
                        DimColour = new Vector4(dimColour.R, dimColour.G, dimColour.B, dimColour.A),
                        DimLevel = dimLevel,
                    };

                    textureShader.BindUniformBlock("m_DimmableBeatmapBackgroundParameters", dimmableBeatmapBackgroundParametersBuffer);
                }

                protected override void Draw(IRenderer renderer)
                {
                    bindParametersBuffer(renderer);
                    base.Draw(renderer);
                }

                protected override void DrawOpaqueInterior(IRenderer renderer)
                {
                    bindParametersBuffer(renderer);
                    base.DrawOpaqueInterior(renderer);
                }

                protected override void Dispose(bool isDisposing)
                {
                    base.Dispose(isDisposing);
                    dimmableBeatmapBackgroundParametersBuffer?.Dispose();
                }

                [StructLayout(LayoutKind.Sequential, Pack = 1)]
                private record struct DimmableBeatmapBackgroundParameters
                {
                    public UniformVector4 DimColour;
                    public UniformFloat DimLevel;
                    private readonly UniformPadding12 pad1;
                }
            }
        }
    }
}

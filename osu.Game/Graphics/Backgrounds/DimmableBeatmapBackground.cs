// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shaders.Types;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Transforms;
using osu.Game.Beatmaps;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.Backgrounds
{
    /// <summary>
    /// A background which offers dimming using a custom shader with ability to change dim colour.
    /// </summary>
    public partial class DimmableBeatmapBackground : BeatmapBackground {
        private float dimLevel;
        private Color4 dimColour;

        public float DimLevel {
            get => dimLevel;
            set {
                dimLevel = value;
                DimmableBeatmapBackgroundSprite sprite = (DimmableBeatmapBackgroundSprite)Sprite;
                sprite.DimLevel = dimLevel;

                bufferedContainer?.ForceRedraw();
            }
        }

        public Color4 DimColour {
            get => dimColour;
            set {
                dimColour = value;
                DimmableBeatmapBackgroundSprite sprite = (DimmableBeatmapBackgroundSprite)Sprite;
                sprite.DimColour = dimColour;

                bufferedContainer?.ForceRedraw();
            }
        }

        public DimmableBeatmapBackground(WorkingBeatmap beatmap, string fallbackTextureName = @"Backgrounds/bg1") : base(beatmap, fallbackTextureName) {
            Sprite = new DimmableBeatmapBackgroundSprite {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                FillMode = FillMode.Fill,
            };
        }

        public partial class DimmableBeatmapBackgroundSprite : Sprite {
            public float DimLevel;
            public Color4 DimColour;

            [BackgroundDependencyLoader]
            private void load(ShaderManager shaders)
            {
                TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "DimmableBeatmapBackground");
            }

            protected override DrawNode CreateDrawNode() => new DimmableBeatmapBackgroundSpriteDrawNode(this);

            public class DimmableBeatmapBackgroundSpriteDrawNode : SpriteDrawNode {
                public new DimmableBeatmapBackgroundSprite Source => (DimmableBeatmapBackgroundSprite)base.Source;

                public DimmableBeatmapBackgroundSpriteDrawNode(DimmableBeatmapBackgroundSprite source)
                    : base(source)
                {
                }

                private float DimLevel;
                private Color4 DimColour;

                private IShader textureShader;

                public override void ApplyState()
                {
                    base.ApplyState();

                    DimLevel = Source.DimLevel;
                    DimColour = Source.DimColour;

                    textureShader = Source.TextureShader;
                }

                private IUniformBuffer<DimmableBeatmapBackgroundParameters> dimmableBeatmapBackgroundParametersBuffer;

                private void BindParametersBuffer(IRenderer renderer)
                {
                    dimmableBeatmapBackgroundParametersBuffer ??= renderer.CreateUniformBuffer<DimmableBeatmapBackgroundParameters>();

                    dimmableBeatmapBackgroundParametersBuffer.Data = dimmableBeatmapBackgroundParametersBuffer.Data with
                    {
                        DimColour = new Vector4(DimColour.R, DimColour.G, DimColour.B, DimColour.A),
                        DimLevel = DimLevel,
                    };

                    textureShader.BindUniformBlock("m_DimmableBeatmapBackgroundParameters", dimmableBeatmapBackgroundParametersBuffer);
                }

                protected override void Draw(IRenderer renderer)
                {
                    BindParametersBuffer(renderer);
                    base.Draw(renderer);
                }

                protected override void DrawOpaqueInterior(IRenderer renderer)
                {
                    BindParametersBuffer(renderer);
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

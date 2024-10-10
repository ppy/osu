// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Runtime.InteropServices;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shaders.Types;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Argon
{
    public partial class ArgonSpinnerProgressArc : CompositeDrawable
    {
        private const float arc_fill = 0.15f;
        private const float arc_radius = 0.12f;

        private ProgressFill fill = null!;

        private DrawableSpinner spinner = null!;

        private CircularProgress background = null!;

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableHitObject)
        {
            RelativeSizeAxes = Axes.Both;

            spinner = (DrawableSpinner)drawableHitObject;

            InternalChildren = new Drawable[]
            {
                background = new CircularProgress
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = Color4.White.Opacity(0.25f),
                    RelativeSizeAxes = Axes.Both,
                    Progress = arc_fill,
                    Rotation = 90 - arc_fill * 180,
                    InnerRadius = arc_radius,
                    RoundedCaps = true,
                },
                fill = new ProgressFill
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    InnerRadius = arc_radius,
                    RoundedCaps = true,
                    GlowColour = new Color4(171, 255, 255, 180)
                }
            };
        }

        protected override void Update()
        {
            base.Update();

            background.Alpha = spinner.Progress >= 1 ? 0 : 1;

            fill.Alpha = (float)Interpolation.DampContinuously(fill.Alpha, spinner.Progress > 0 && spinner.Progress < 1 ? 1 : 0, 40f, (float)Math.Abs(Time.Elapsed));
            fill.Progress = (float)Interpolation.DampContinuously(fill.Progress, spinner.Progress >= 1 ? 0 : arc_fill * spinner.Progress, 40f, (float)Math.Abs(Time.Elapsed));

            fill.Rotation = (float)(90 - fill.Progress * 180);
        }

        private partial class ProgressFill : CircularProgress
        {
            private Color4 glowColour = Color4.White;

            public Color4 GlowColour
            {
                get => glowColour;
                set
                {
                    glowColour = value;
                    Invalidate(Invalidation.DrawNode);
                }
            }

            private Texture glowTexture = null!;
            private IShader glowShader = null!;
            private float glowSize;

            [BackgroundDependencyLoader]
            private void load(TextureStore textures, ShaderManager shaders)
            {
                glowTexture = textures.Get("Gameplay/osu/spinner-glow");
                glowShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "SpinnerGlow");
                glowSize = Blur.KernelSize(50); // Half of the maximum blur sigma in the design (which is 100)
            }

            protected override DrawNode CreateDrawNode() => new ProgressFillDrawNode(this);

            private class ProgressFillDrawNode : CircularProgressDrawNode
            {
                protected new ProgressFill Source => (ProgressFill)base.Source;

                public ProgressFillDrawNode(CircularProgress source)
                    : base(source)
                {
                }

                private Texture glowTexture = null!;
                private IShader glowShader = null!;
                private Quad glowQuad;
                private float relativeGlowSize;
                private Color4 glowColour;

                public override void ApplyState()
                {
                    base.ApplyState();

                    glowTexture = Source.glowTexture;
                    glowShader = Source.glowShader;
                    glowColour = Source.glowColour;

                    // Inflated draw quad by the size of the blur.
                    glowQuad = Source.ToScreenSpace(DrawRectangle.Inflate(Source.glowSize));
                    relativeGlowSize = Source.glowSize / Source.DrawSize.X;
                }

                protected override void Draw(IRenderer renderer)
                {
                    base.Draw(renderer);
                    drawGlow(renderer);
                }

                private void drawGlow(IRenderer renderer)
                {
                    renderer.SetBlend(BlendingParameters.Additive);

                    glowShader.Bind();
                    bindGlowUniformResources(glowShader, renderer);

                    ColourInfo col = DrawColourInfo.Colour;
                    col.ApplyChild(glowColour);

                    renderer.DrawQuad(glowTexture, glowQuad, col);

                    glowShader.Unbind();
                }

                private IUniformBuffer<ProgressGlowParameters>? progressGlowParametersBuffer;

                private void bindGlowUniformResources(IShader shader, IRenderer renderer)
                {
                    progressGlowParametersBuffer ??= renderer.CreateUniformBuffer<ProgressGlowParameters>();
                    progressGlowParametersBuffer.Data = new ProgressGlowParameters
                    {
                        InnerRadius = InnerRadius,
                        Progress = Progress,
                        TexelSize = TexelSize,
                        GlowSize = relativeGlowSize
                    };

                    shader.BindUniformBlock("m_ProgressGlowParameters", progressGlowParametersBuffer);
                }

                protected override void Dispose(bool isDisposing)
                {
                    base.Dispose(isDisposing);
                    progressGlowParametersBuffer?.Dispose();
                }

                [StructLayout(LayoutKind.Sequential, Pack = 1)]
                private record struct ProgressGlowParameters
                {
                    public UniformFloat InnerRadius;
                    public UniformFloat Progress;
                    public UniformFloat TexelSize;
                    public UniformFloat GlowSize;
                }
            }
        }
    }
}

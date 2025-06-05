// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Runtime.InteropServices;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Rendering.Vertices;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shaders.Types;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace osu.Game.Graphics
{
    /// <summary>
    /// A (very cute) animated version of the <see cref="FontAwesome.Solid.Ghost"/> icon.
    /// </summary>
    public partial class GhostIcon : Drawable
    {
        private IShader ghostShader = null!;

        /// <summary>
        /// How long one complete loop of the ghost's animation takes, in milliseconds
        /// </summary>
        public float AnimationDuration = 2000;

        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders)
        {
            ghostShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "Ghost");
        }

        protected override void Update()
        {
            base.Update();

            Invalidate(Invalidation.DrawNode);
        }

        protected override DrawNode CreateDrawNode() => new GhostIconDrawNode(this);

        private class GhostIconDrawNode : DrawNode
        {
            protected new GhostIcon Source => (GhostIcon)base.Source;

            public GhostIconDrawNode(IDrawable source)
                : base(source)
            {
            }

            private Quad screenSpaceDrawQuad;
            private Vector4 drawRectangle;
            private Vector2 blend;
            private IShader shader = null!;
            private float time;

            public override void ApplyState()
            {
                base.ApplyState();

                screenSpaceDrawQuad = Source.ScreenSpaceDrawQuad;
                drawRectangle = new Vector4(0, 0, Source.DrawWidth, Source.DrawHeight);
                shader = Source.ghostShader;
                blend = new Vector2(Math.Min(Source.DrawWidth, Source.DrawHeight) / Math.Min(screenSpaceDrawQuad.Width, screenSpaceDrawQuad.Height));
                time = (float)(Source.Time.Current / Source.AnimationDuration) % 1f;
            }

            private IUniformBuffer<GhostParameters>? ghostParametersBuffer;

            private IVertexBatch<TexturedVertex2D>? quadBatch;

            protected override void Draw(IRenderer renderer)
            {
                base.Draw(renderer);

                if (!renderer.BindTexture(renderer.WhitePixel))
                    return;

                quadBatch ??= renderer.CreateQuadBatch<TexturedVertex2D>(1, 2);
                ghostParametersBuffer ??= renderer.CreateUniformBuffer<GhostParameters>();

                ghostParametersBuffer.Data = new GhostParameters
                {
                    Time = time
                };

                shader.Bind();
                shader.BindUniformBlock("m_GhostParameters", ghostParametersBuffer);

                var vertexAction = quadBatch.AddAction;

                vertexAction(new TexturedVertex2D(renderer)
                {
                    Position = screenSpaceDrawQuad.BottomLeft,
                    TexturePosition = new Vector2(0, 1),
                    TextureRect = drawRectangle,
                    BlendRange = blend,
                    Colour = DrawColourInfo.Colour.BottomLeft.SRGB,
                });
                vertexAction(new TexturedVertex2D(renderer)
                {
                    Position = screenSpaceDrawQuad.BottomRight,
                    TexturePosition = new Vector2(1, 1),
                    TextureRect = drawRectangle,
                    BlendRange = blend,
                    Colour = DrawColourInfo.Colour.BottomRight.SRGB,
                });
                vertexAction(new TexturedVertex2D(renderer)
                {
                    Position = screenSpaceDrawQuad.TopRight,
                    TexturePosition = new Vector2(1, 0),
                    TextureRect = drawRectangle,
                    BlendRange = blend,
                    Colour = DrawColourInfo.Colour.TopRight.SRGB,
                });
                vertexAction(new TexturedVertex2D(renderer)
                {
                    Position = screenSpaceDrawQuad.TopLeft,
                    TexturePosition = Vector2.Zero,
                    TextureRect = drawRectangle,
                    BlendRange = blend,
                    Colour = DrawColourInfo.Colour.TopLeft.SRGB,
                });

                shader.Unbind();
            }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            private record struct GhostParameters
            {
                public UniformFloat Time;
                private UniformPadding12 pad;
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);

                ghostParametersBuffer?.Dispose();
                quadBatch?.Dispose();
            }
        }
    }
}

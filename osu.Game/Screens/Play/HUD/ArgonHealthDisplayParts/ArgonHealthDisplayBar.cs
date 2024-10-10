// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Runtime.InteropServices;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shaders.Types;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD.ArgonHealthDisplayParts
{
    public partial class ArgonHealthDisplayBar : Box
    {
        private Vector2 progressRange = new Vector2(0f, 1f);

        public Vector2 ProgressRange
        {
            get => progressRange;
            set
            {
                if (progressRange == value)
                    return;

                progressRange = value;
                Invalidate(Invalidation.DrawNode);
            }
        }

        private float radius = 10f;

        public float PathRadius
        {
            get => radius;
            set
            {
                if (radius == value)
                    return;

                radius = value;
                Invalidate(Invalidation.DrawNode);
            }
        }

        private float glowPortion;

        public float GlowPortion
        {
            get => glowPortion;
            set
            {
                if (glowPortion == value)
                    return;

                glowPortion = value;
                Invalidate(Invalidation.DrawNode);
            }
        }

        private Colour4 barColour = Color4.White;

        public Colour4 BarColour
        {
            get => barColour;
            set
            {
                if (barColour == value)
                    return;

                barColour = value;
                Invalidate(Invalidation.DrawNode);
            }
        }

        private Colour4 glowColour = Color4.White.Opacity(0);

        public Colour4 GlowColour
        {
            get => glowColour;
            set
            {
                if (glowColour == value)
                    return;

                glowColour = value;
                Invalidate(Invalidation.DrawNode);
            }
        }

        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders)
        {
            TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "ArgonBarPath");
        }

        protected override DrawNode CreateDrawNode() => new ArgonBarPathDrawNode(this);

        private class ArgonBarPathDrawNode : SpriteDrawNode
        {
            protected new ArgonHealthDisplayBar Source => (ArgonHealthDisplayBar)base.Source;

            private IUniformBuffer<ArgonBarPathParameters>? parametersBuffer;

            public ArgonBarPathDrawNode(ArgonHealthDisplayBar source)
                : base(source)
            {
            }

            private Vector2 size;
            private Vector2 progressRange;
            private float pathRadius;
            private float glowPortion;
            private Color4 barColour;
            private Color4 glowColour;

            public override void ApplyState()
            {
                base.ApplyState();

                size = Source.DrawSize;
                progressRange = new Vector2(Math.Min(Source.progressRange.X, Source.progressRange.Y), Source.progressRange.Y);
                pathRadius = Source.PathRadius;
                glowPortion = Source.GlowPortion;
                barColour = Source.barColour;
                glowColour = Source.glowColour;
            }

            protected override void Draw(IRenderer renderer)
            {
                if (pathRadius == 0)
                    return;

                base.Draw(renderer);
            }

            protected override void BindUniformResources(IShader shader, IRenderer renderer)
            {
                base.BindUniformResources(shader, renderer);

                parametersBuffer ??= renderer.CreateUniformBuffer<ArgonBarPathParameters>();
                parametersBuffer.Data = new ArgonBarPathParameters
                {
                    BarColour = new Vector4(barColour.R, barColour.G, barColour.B, barColour.A),
                    GlowColour = new Vector4(glowColour.R, glowColour.G, glowColour.B, glowColour.A),
                    GlowPortion = glowPortion,
                    Size = size,
                    ProgressRange = progressRange,
                    PathRadius = pathRadius
                };

                shader.BindUniformBlock("m_ArgonBarPathParameters", parametersBuffer);
            }

            protected override bool CanDrawOpaqueInterior => false;

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);
                parametersBuffer?.Dispose();
            }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            private record struct ArgonBarPathParameters
            {
                public UniformVector4 BarColour;
                public UniformVector4 GlowColour;
                public UniformVector2 Size;
                public UniformVector2 ProgressRange;
                public UniformFloat PathRadius;
                public UniformFloat GlowPortion;
                private readonly UniformPadding8 pad;
            }
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Runtime.InteropServices;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shaders.Types;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace osu.Game.Screens.Play.HUD.ArgonHealthDisplayParts
{
    public partial class ArgonHealthDisplayBackground : Box
    {
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

        private float padding = 10f;

        public float PathPadding
        {
            get => padding;
            set
            {
                if (padding == value)
                    return;

                padding = value;
                Invalidate(Invalidation.DrawNode);
            }
        }

        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders)
        {
            TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "ArgonBarPathBackground");
        }

        protected override void Update()
        {
            base.Update();
            Invalidate(Invalidation.DrawNode);
        }

        protected override DrawNode CreateDrawNode() => new ArgonBarPathDrawNode(this);

        private class ArgonBarPathDrawNode : SpriteDrawNode
        {
            protected new ArgonHealthDisplayBackground Source => (ArgonHealthDisplayBackground)base.Source;

            public ArgonBarPathDrawNode(ArgonHealthDisplayBackground source)
                : base(source)
            {
            }

            private Vector2 size;
            private float pathRadius;
            private float padding;

            public override void ApplyState()
            {
                base.ApplyState();

                size = Source.DrawSize;
                pathRadius = Source.PathRadius;
                padding = Source.PathPadding;
            }

            protected override void Draw(IRenderer renderer)
            {
                if (pathRadius == 0)
                    return;

                base.Draw(renderer);
            }

            private IUniformBuffer<ArgonBarPathBackgroundParameters> parametersBuffer;

            protected override void BindUniformResources(IShader shader, IRenderer renderer)
            {
                base.BindUniformResources(shader, renderer);

                parametersBuffer ??= renderer.CreateUniformBuffer<ArgonBarPathBackgroundParameters>();
                parametersBuffer.Data = new ArgonBarPathBackgroundParameters
                {
                    Size = size,
                    PathRadius = pathRadius,
                    Padding = padding
                };

                shader.BindUniformBlock("m_ArgonBarPathBackgroundParameters", parametersBuffer);
            }

            protected override bool CanDrawOpaqueInterior => false;

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);
                parametersBuffer?.Dispose();
            }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            private record struct ArgonBarPathBackgroundParameters
            {
                public UniformVector2 Size;
                public UniformFloat PathRadius;
                public UniformFloat Padding;
            }
        }
    }
}

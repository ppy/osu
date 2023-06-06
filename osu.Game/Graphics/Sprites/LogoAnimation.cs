// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Runtime.InteropServices;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Rendering.Vertices;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shaders.Types;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using osuTK.Graphics.ES30;

namespace osu.Game.Graphics.Sprites
{
    public partial class LogoAnimation : Sprite
    {
        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders)
        {
            TextureShader = shaders.Load(@"LogoAnimation", @"LogoAnimation");
        }

        private float animationProgress;

        public float AnimationProgress
        {
            get => animationProgress;
            set
            {
                if (animationProgress == value) return;

                animationProgress = value;
                Invalidate(Invalidation.DrawInfo);
            }
        }

        public override bool IsPresent => true;

        protected override DrawNode CreateDrawNode() => new LogoAnimationDrawNode(this);

        private class LogoAnimationDrawNode : SpriteDrawNode
        {
            private LogoAnimation source => (LogoAnimation)Source;

            private readonly Action<TexturedVertex2D> addVertexAction;

            private float progress;

            public LogoAnimationDrawNode(LogoAnimation source)
                : base(source)
            {
                addVertexAction = v =>
                {
                    animationVertexBatch!.Add(new LogoAnimationVertex
                    {
                        Position = v.Position,
                        Colour = v.Colour,
                        TexturePosition = v.TexturePosition,
                    });
                };
            }

            public override void ApplyState()
            {
                base.ApplyState();

                progress = source.animationProgress;
            }

            private IUniformBuffer<AnimationData> animationDataBuffer;
            private IVertexBatch<LogoAnimationVertex> animationVertexBatch;

            protected override void BindUniformResources(IShader shader, IRenderer renderer)
            {
                base.BindUniformResources(shader, renderer);

                animationDataBuffer ??= renderer.CreateUniformBuffer<AnimationData>();
                animationVertexBatch ??= renderer.CreateQuadBatch<LogoAnimationVertex>(1, 2);

                animationDataBuffer.Data = animationDataBuffer.Data with { Progress = progress };

                shader.BindUniformBlock(@"m_AnimationData", animationDataBuffer);
            }

            protected override void Blit(IRenderer renderer)
            {
                if (DrawRectangle.Width == 0 || DrawRectangle.Height == 0)
                    return;

                base.Blit(renderer);

                renderer.DrawQuad(
                    Texture,
                    ScreenSpaceDrawQuad,
                    DrawColourInfo.Colour,
                    inflationPercentage: new Vector2(InflationAmount.X / DrawRectangle.Width, InflationAmount.Y / DrawRectangle.Height),
                    textureCoords: TextureCoords,
                    vertexAction: addVertexAction);
            }

            protected override bool CanDrawOpaqueInterior => false;

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);
                animationDataBuffer?.Dispose();
            }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            private record struct AnimationData
            {
                public UniformFloat Progress;
                private readonly UniformPadding12 pad1;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct LogoAnimationVertex : IEquatable<LogoAnimationVertex>, IVertex
            {
                [VertexMember(2, VertexAttribPointerType.Float)]
                public Vector2 Position;

                [VertexMember(4, VertexAttribPointerType.Float)]
                public Color4 Colour;

                [VertexMember(2, VertexAttribPointerType.Float)]
                public Vector2 TexturePosition;

                public readonly bool Equals(LogoAnimationVertex other) =>
                    Position.Equals(other.Position)
                    && TexturePosition.Equals(other.TexturePosition)
                    && Colour.Equals(other.Colour);
            }
        }
    }
}

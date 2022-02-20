// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Graphics.Sprites
{
    public class LogoAnimation : Sprite
    {
        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders)
        {
            TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, @"LogoAnimation");
            RoundedTextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, @"LogoAnimation"); // Masking isn't supported for now
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

            private float progress;

            public LogoAnimationDrawNode(LogoAnimation source)
                : base(source)
            {
            }

            public override void ApplyState()
            {
                base.ApplyState();

                progress = source.animationProgress;
            }

            protected override void Blit(Action<TexturedVertex2D> vertexAction)
            {
                Shader.GetUniform<float>("progress").UpdateValue(ref progress);

                base.Blit(vertexAction);
            }

            protected override bool CanDrawOpaqueInterior => false;
        }
    }
}

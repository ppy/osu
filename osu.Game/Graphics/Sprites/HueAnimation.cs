// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Graphics.Sprites
{
    public class HueAnimation : Sprite
    {
        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders, TextureStore textures)
        {
            TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, @"HueAnimation");
            RoundedTextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, @"HueAnimation"); // Masking isn't supported for now
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

        protected override DrawNode CreateDrawNode() => new HueAnimationDrawNode(this);

        private class HueAnimationDrawNode : SpriteDrawNode
        {
            private HueAnimation source => (HueAnimation)Source;

            private float progress;

            public HueAnimationDrawNode(HueAnimation source)
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

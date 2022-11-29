// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Graphics.Sprites
{
    public partial class LogoAnimation : Sprite
    {
        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders)
        {
            TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, @"LogoAnimation");
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

            protected override void Blit(IRenderer renderer)
            {
                TextureShader.GetUniform<float>("progress").UpdateValue(ref progress);

                base.Blit(renderer);
            }

            protected override bool CanDrawOpaqueInterior => false;
        }
    }
}

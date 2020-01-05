// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using osu.Framework.MathUtils;
using osuTK;

namespace osu.Game.Screens.Menu
{
    public class LazerLogo : Drawable
    {
        private IShader shader;
        private Texture texture;

        private double startTime = -1000;
        private double animationTime = -1000;

        private float animation;
        private float highlight;

        public LazerLogo()
        {
            Size = new Vector2(960);
        }

        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders, TextureStore textures)
        {
            shader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, @"LazerLogo");
            texture = textures.Get("Menu/logo-triangles.png");
        }

        public void Start(double delay, double duration)
        {
            startTime = Clock.CurrentTime + delay;
            animationTime = duration;
        }

        public override bool IsPresent => true;

        protected override void Update()
        {
            base.Update();

            if (animationTime < 0) return;

            highlight = Clock.CurrentTime < startTime + 0.4 * animationTime
                ? Interpolation.ValueAt(Clock.CurrentTime, 0f, 1f, startTime, startTime + animationTime * 1.07, Easing.OutCirc)
                : Interpolation.ValueAt(Clock.CurrentTime, 0.6f, 1f, startTime, startTime + animationTime * 0.9);

            animation = Clock.CurrentTime < startTime + 0.5 * animationTime
                ? Interpolation.ValueAt(Clock.CurrentTime, 0f, 0.8f, startTime, startTime + animationTime * 1.23, Easing.OutQuart)
                : Interpolation.ValueAt(Clock.CurrentTime, 0.4f, 1f, startTime, startTime + animationTime);
        }

        protected override DrawNode CreateDrawNode() => new LazerLogoDrawNode(this);

        private class LazerLogoDrawNode : DrawNode
        {
            protected new LazerLogo Source => (LazerLogo)base.Source;

            private IShader shader;
            private Texture texture;
            private Quad screenSpaceDrawQuad;
            private float animation;
            private float highlight;

            public LazerLogoDrawNode(LazerLogo source)
                : base(source)
            {
            }

            public override void ApplyState()
            {
                base.ApplyState();

                shader = Source.shader;
                texture = Source.texture;
                screenSpaceDrawQuad = Source.ScreenSpaceDrawQuad;
                animation = Source.animation;
                highlight = Source.highlight;
            }

            protected virtual void Blit(Action<TexturedVertex2D> vertexAction)
            {
                DrawQuad(texture, screenSpaceDrawQuad, DrawColourInfo.Colour, null, vertexAction);
            }

            public override void Draw(Action<TexturedVertex2D> vertexAction)
            {
                base.Draw(vertexAction);

                shader.Bind();
                shader.GetUniform<float>("highlight").Value = highlight;
                shader.GetUniform<float>("animation").Value = animation;

                Blit(vertexAction);

                shader.Unbind();
            }
        }
    }
}

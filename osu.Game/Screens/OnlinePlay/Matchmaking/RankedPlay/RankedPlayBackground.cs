// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Runtime.InteropServices;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shaders.Types;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class RankedPlayBackground : CompositeDrawable, IBufferedDrawable
    {
        public float GridSize = 10;

        public IShader? TextureShader { get; private set; }
        public Color4 BackgroundColour => Color4.Black;
        public DrawColourInfo? FrameBufferDrawColour => null;
        public Vector2 FrameBufferScale => new Vector2(0.1f);

        public Color4 GradientOutside = Color4Extensions.FromHex("AC6D97");
        public Color4 GradientInside = Color4Extensions.FromHex("544483");
        public Color4 DotsColour = Color4Extensions.FromHex("6b2980");

        public RankedPlayBackground()
        {
            InternalChildren =
            [
                new Triangles
                {
                    RelativeSizeAxes = Axes.Both,
                },
            ];
        }

        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders)
        {
            TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, @"RankedPlayBackground");
        }

        private readonly BufferedDrawNodeSharedData sharedData = new BufferedDrawNodeSharedData();

        protected override void Update()
        {
            base.Update();

            Invalidate(Invalidation.DrawNode);
        }

        protected override DrawNode CreateDrawNode() => new RankedPlayBackgroundDrawNode(this, sharedData);

        protected override void Dispose(bool isDisposing)
        {
            sharedData.Dispose();

            base.Dispose(isDisposing);
        }

        private class RankedPlayBackgroundDrawNode : BufferedDrawNode, ICompositeDrawNode
        {
            protected new RankedPlayBackground Source => (RankedPlayBackground)base.Source;

            protected new CompositeDrawableDrawNode Child => (CompositeDrawableDrawNode)base.Child;

            public RankedPlayBackgroundDrawNode(RankedPlayBackground source, BufferedDrawNodeSharedData sharedData)
                : base(source, new CompositeDrawableDrawNode(source), sharedData)
            {
            }

            private Vector2 drawSize;
            private float time;
            private float gridSize;
            private Color4 gradientOutside;
            private Color4 gradientInside;
            private Color4 dotsColour;

            private IUniformBuffer<RankedPlayBackgroundParameters>? shaderParameterBuffer;

            public override void ApplyState()
            {
                base.ApplyState();

                time = (float)(Source.Time.Current / 1000);
                drawSize = Source.DrawSize;
                gridSize = Source.GridSize;
                gradientOutside = Source.GradientOutside;
                gradientInside = Source.GradientInside;
                dotsColour = Source.DotsColour;
            }

            protected override void BindUniformResources(IShader shader, IRenderer renderer)
            {
                shaderParameterBuffer ??= renderer.CreateUniformBuffer<RankedPlayBackgroundParameters>();

                shaderParameterBuffer.Data = new RankedPlayBackgroundParameters
                {
                    DrawSize = drawSize,
                    Time = time,
                    GridSize = gridSize,
                    GradientOutside = new Vector4(gradientOutside.R, gradientOutside.G, gradientOutside.B, gradientOutside.A),
                    GradientInside = new Vector4(gradientInside.R, gradientInside.G, gradientInside.B, gradientInside.A),
                    DotsColour = new Vector4(dotsColour.R, dotsColour.G, dotsColour.B, dotsColour.A),
                };

                shader.BindUniformBlock("m_RankedPlayBackgroundParameters", shaderParameterBuffer);
            }

            public List<DrawNode>? Children
            {
                get => Child.Children;
                set => Child.Children = value;
            }

            public bool AddChildDrawNodes => RequiresRedraw;

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            private record struct RankedPlayBackgroundParameters
            {
                public UniformVector2 DrawSize;
                public UniformFloat Time;
                public UniformFloat GridSize;
                public UniformVector4 GradientOutside;
                public UniformVector4 GradientInside;
                public UniformVector4 DotsColour;
            }
        }

        public partial class Triangles : CompositeDrawable
        {
            private Texture triangleTexture = null!;

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                triangleTexture = textures.Get("Online/RankedPlay/triangle");
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                for (int i = 0; i < 20; i++)
                {
                    AddInternal(new Triangle
                    {
                        Texture = triangleTexture,
                        RelativePositionAxes = Axes.Both,
                        X = RNG.NextSingle(),
                        Y = -0.2f + RNG.NextSingle() * 1.4f,
                        Origin = Anchor.Centre,
                        Rotation = RNG.NextSingle() * 360,
                        AngularVelocity = RNG.NextSingle() - 0.75f,
                        Size = new Vector2(100 + RNG.NextSingle() * 1000),
                        MovementSpeed = 0.25f + RNG.NextSingle() * 0.75f,
                        Alpha = 0.5f + RNG.NextSingle() * 0.5f,
                    });
                }
            }

            public float ParticleVelocity = 1;

            protected override void Update()
            {
                base.Update();

                if (DrawHeight <= 0)
                    return;

                float baseVelocity = 0.03f * ParticleVelocity / DrawHeight;
                float elapsed = (float)Time.Elapsed;

                foreach (var c in InternalChildren)
                {
                    var triangle = (Triangle)c;

                    triangle.Y -= baseVelocity * elapsed * triangle.MovementSpeed;

                    triangle.Rotation += triangle.AngularVelocity * elapsed * 0.02f;

                    // wrap vertically
                    if (triangle.Y < -0.2f)
                    {
                        triangle.X = RNG.NextSingle();
                        triangle.Y = 1.2f;
                        triangle.Alpha = 0.5f + RNG.NextSingle() * 0.5f;
                    }
                    else if (triangle.Y > 1.2f)
                    {
                        triangle.X = RNG.NextSingle();
                        triangle.Y = -0.2f;
                        triangle.Alpha = 0.5f + RNG.NextSingle() * 0.5f;
                    }
                }
            }

            private partial class Triangle : Sprite
            {
                public float MovementSpeed = 1;
                public float AngularVelocity;
            }
        }
    }
}

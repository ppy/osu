// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public partial class CentreMarker : CompositeDrawable
    {
        public float TriangleHeightRatio
        {
            get => triangles.TriangleHeightRatio;
            set => triangles.TriangleHeightRatio = value;
        }

        private readonly VerticalTriangles triangles;

        public CentreMarker()
        {
            RelativeSizeAxes = Axes.Y;
            Masking = true;
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Y,
                    Width = 1.4f,
                    EdgeSmoothness = new Vector2(1, 0)
                },
                triangles = new VerticalTriangles
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colours) => Colour = colours.Highlight1;

        /// <summary>
        /// Triangles drawn at the top and bottom of <see cref="CentreMarker"/>.
        /// </summary>
        /// <remarks>
        /// Since framework-side triangles don't support antialiasing we are using custom implementation involving shaders to avoid mismatch
        /// in antialiasing between top and bottom triangles when drawable moves across the screen.
        /// </remarks>
        private partial class VerticalTriangles : Sprite
        {
            private float triangleHeightRatio = 1f;

            public float TriangleHeightRatio
            {
                get => triangleHeightRatio;
                set
                {
                    triangleHeightRatio = value;
                    Invalidate(Invalidation.DrawNode);
                }
            }

            [BackgroundDependencyLoader]
            private void load(ShaderManager shaders, IRenderer renderer)
            {
                TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "TriangleBorder");
                Texture = renderer.WhitePixel;
            }

            protected override DrawNode CreateDrawNode() => new VerticalTrianglesDrawNode(this);

            private class VerticalTrianglesDrawNode : SpriteDrawNode
            {
                private const float aa = 1.5f; // across how many pixels antialiasing is being applied

                public new VerticalTriangles Source => (VerticalTriangles)base.Source;

                public VerticalTrianglesDrawNode(VerticalTriangles source)
                    : base(source)
                {
                }

                private float texelSize;
                private float triangleScreenSpaceHeight;

                public override void ApplyState()
                {
                    base.ApplyState();

                    triangleScreenSpaceHeight = ScreenSpaceDrawQuad.Width * Source.TriangleHeightRatio;
                    texelSize = aa / Math.Max(ScreenSpaceDrawQuad.Width, 1);
                }

                protected override void Blit(IRenderer renderer)
                {
                    if (triangleScreenSpaceHeight == 0)
                        return;

                    // TriangleBorder shader makes a smooth triangle for all its sides, which we want to avoid at the top and bottom.
                    // To do that we are expanding triangles outside the drawable by the aa value and applying masking at the top level.
                    Quad topTriangle = new Quad
                    (
                        ScreenSpaceDrawQuad.TopLeft + new Vector2(-aa, triangleScreenSpaceHeight),
                        ScreenSpaceDrawQuad.TopRight + new Vector2(aa, triangleScreenSpaceHeight),
                        ScreenSpaceDrawQuad.TopLeft - new Vector2(aa),
                        ScreenSpaceDrawQuad.TopRight + new Vector2(aa, -aa)
                    );

                    Quad bottomTriangle = new Quad
                    (
                        ScreenSpaceDrawQuad.BottomLeft - new Vector2(aa, triangleScreenSpaceHeight),
                        ScreenSpaceDrawQuad.BottomRight - new Vector2(-aa, triangleScreenSpaceHeight),
                        ScreenSpaceDrawQuad.BottomLeft + new Vector2(-aa, aa),
                        ScreenSpaceDrawQuad.BottomRight + new Vector2(aa)
                    );

                    renderer.DrawQuad(Texture, topTriangle, DrawColourInfo.Colour);
                    renderer.DrawQuad(Texture, bottomTriangle, DrawColourInfo.Colour);
                }

                private IUniformBuffer<TriangleBorderData>? borderDataBuffer;

                protected override void BindUniformResources(IShader shader, IRenderer renderer)
                {
                    base.BindUniformResources(shader, renderer);

                    borderDataBuffer ??= renderer.CreateUniformBuffer<TriangleBorderData>();
                    borderDataBuffer.Data = borderDataBuffer.Data with
                    {
                        Thickness = 1f,
                        TexelSize = texelSize
                    };

                    shader.BindUniformBlock("m_BorderData", borderDataBuffer);
                }

                protected override bool CanDrawOpaqueInterior => false;

                protected override void Dispose(bool isDisposing)
                {
                    base.Dispose(isDisposing);
                    borderDataBuffer?.Dispose();
                }
            }
        }
    }
}

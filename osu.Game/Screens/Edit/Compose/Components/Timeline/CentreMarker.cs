// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
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
                    RelativeSizeAxes = Axes.Both,
                    EdgeSmoothness = Vector2.One
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colours) => Colour = colours.Highlight1;

        /// <summary>
        /// Triangles drawn at the top and bottom of <see cref="CentreMarker"/>.
        /// </summary>
        /// <remarks>
        /// Since framework-side triangles don't support antialiasing we are using custom implementation involving rotated smoothened boxes to avoid
        /// mismatch in antialiasing between top and bottom triangles when drawable moves across the screen.
        /// To "trim" boxes we must enable masking at the top level.
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
            private void load(IRenderer renderer)
            {
                Texture = renderer.WhitePixel;
            }

            protected override DrawNode CreateDrawNode() => new VerticalTrianglesDrawNode(this);

            private class VerticalTrianglesDrawNode : SpriteDrawNode
            {
                public new VerticalTriangles Source => (VerticalTriangles)base.Source;

                public VerticalTrianglesDrawNode(VerticalTriangles source)
                    : base(source)
                {
                }

                private float triangleScreenSpaceHeight;

                public override void ApplyState()
                {
                    base.ApplyState();

                    triangleScreenSpaceHeight = ScreenSpaceDrawQuad.Width * Source.TriangleHeightRatio;
                }

                protected override void Blit(IRenderer renderer)
                {
                    if (triangleScreenSpaceHeight == 0 || DrawRectangle.Width == 0 || DrawRectangle.Height == 0)
                        return;

                    Vector2 inflation = new Vector2(InflationAmount.X / DrawRectangle.Width, InflationAmount.Y / (DrawRectangle.Width * Source.TriangleHeightRatio));

                    Quad topTriangle = new Quad
                    (
                        ScreenSpaceDrawQuad.TopLeft,
                        ScreenSpaceDrawQuad.TopLeft + new Vector2(ScreenSpaceDrawQuad.Width * 0.5f, -triangleScreenSpaceHeight),
                        ScreenSpaceDrawQuad.TopLeft + new Vector2(ScreenSpaceDrawQuad.Width * 0.5f, triangleScreenSpaceHeight),
                        ScreenSpaceDrawQuad.TopRight
                    );

                    Quad bottomTriangle = new Quad
                    (
                        ScreenSpaceDrawQuad.BottomLeft,
                        ScreenSpaceDrawQuad.BottomLeft + new Vector2(ScreenSpaceDrawQuad.Width * 0.5f, -triangleScreenSpaceHeight),
                        ScreenSpaceDrawQuad.BottomLeft + new Vector2(ScreenSpaceDrawQuad.Width * 0.5f, triangleScreenSpaceHeight),
                        ScreenSpaceDrawQuad.BottomRight
                    );

                    renderer.DrawQuad(Texture, topTriangle, DrawColourInfo.Colour, inflationPercentage: inflation);
                    renderer.DrawQuad(Texture, bottomTriangle, DrawColourInfo.Colour, inflationPercentage: inflation);
                }

                protected override bool CanDrawOpaqueInterior => false;
            }
        }
    }
}

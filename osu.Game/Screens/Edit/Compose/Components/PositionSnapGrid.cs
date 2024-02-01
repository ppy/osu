// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Layout;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public abstract partial class PositionSnapGrid : CompositeDrawable
    {
        /// <summary>
        /// The position of the origin of this <see cref="PositionSnapGrid"/> in local coordinates.
        /// </summary>
        public Bindable<Vector2> StartPosition { get; } = new Bindable<Vector2>(Vector2.Zero);

        protected readonly LayoutValue GridCache = new LayoutValue(Invalidation.RequiredParentSizeToFit);

        protected PositionSnapGrid()
        {
            StartPosition.BindValueChanged(_ => GridCache.Invalidate());

            AddLayout(GridCache);
        }

        protected override void Update()
        {
            base.Update();

            if (GridCache.IsValid) return;

            ClearInternal();

            if (DrawWidth > 0 && DrawHeight > 0)
                CreateContent();

            GridCache.Validate();
        }

        protected abstract void CreateContent();

        protected void GenerateOutline(Vector2 drawSize)
        {
            // Make lines the same width independent of display resolution.
            float lineWidth = DrawWidth / ScreenSpaceDrawQuad.Width;

            AddRangeInternal(new[]
            {
                new Box
                {
                    Colour = Colour4.White,
                    Alpha = 0.3f,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = lineWidth,
                    Y = 0,
                },
                new Box
                {
                    Colour = Colour4.White,
                    Alpha = 0.3f,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = lineWidth,
                    Y = drawSize.Y,
                },
                new Box
                {
                    Colour = Colour4.White,
                    Alpha = 0.3f,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Y,
                    Width = lineWidth,
                    X = 0,
                },
                new Box
                {
                    Colour = Colour4.White,
                    Alpha = 0.3f,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Y,
                    Width = lineWidth,
                    X = drawSize.X,
                },
            });
        }

        public abstract Vector2 GetSnappedPosition(Vector2 original);
    }
}

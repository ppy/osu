// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Edit.Masks.SliderMasks.Components
{
    public class SliderControlPoint : CompositeDrawable
    {
        private readonly Path path;
        private readonly CircularContainer marker;

        private OsuColour colours;

        public SliderControlPoint()
        {
            Size = new Vector2(5);
            Origin = Anchor.Centre;

            NextPoint = Position;

            InternalChildren = new Drawable[]
            {
                path = new SmoothPath
                {
                    BypassAutoSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    PathWidth = 1,
                },
                marker = new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Child = new Box { RelativeSizeAxes = Axes.Both }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            this.colours = colours;

            marker.Colour = colours.YellowDark;
        }

        public bool SegmentSeparator
        {
            set => marker.Colour = value ? colours.Red : colours.YellowDark;
        }

        private Vector2 nextPoint;

        public Vector2 NextPoint
        {
            set
            {
                nextPoint = value;
                pathCache.Invalidate();
            }
        }

        protected override void Update()
        {
            base.Update();

            validatePath();
        }

        private Cached pathCache = new Cached();

        private void validatePath()
        {
            if (pathCache.IsValid)
                return;

            path.ClearVertices();
            path.AddVertex(nextPoint - Position);
            path.AddVertex(Vector2.Zero);
            path.OriginPosition = path.PositionInBoundingBox(Vector2.Zero);

            pathCache.Validate();
        }
    }
}

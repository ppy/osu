// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Layout;
using osu.Game.Graphics.Backgrounds;
using osuTK;

namespace osu.Game.Storyboards.Drawables
{
    public partial class DrawableStoryboardLayer : CompositeDrawable, IColouredDimmable
    {
        public StoryboardLayer Layer { get; }
        public bool Enabled;

        public override bool IsPresent => Enabled && base.IsPresent;

        protected LayerElementContainer ElementContainer { get; }

        private LayoutValue<Colour4> drawColourOffsetBacking = new LayoutValue<Colour4>(Invalidation.DrawNode | Invalidation.Colour | Invalidation.DrawInfo | Invalidation.RequiredParentSizeToFit | Invalidation.Presence);

        public Colour4 DrawColourOffset => drawColourOffsetBacking.IsValid ? drawColourOffsetBacking.Value : drawColourOffsetBacking.Value = computeDrawColourOffset();

        private Colour4 computeDrawColourOffset()
        {
            // Direct Parent is a Container, so we need to go up two levels to get the DrawableStoryboard.
            if (Parent?.Parent is IColouredDimmable colouredDimmableParent)
                return colouredDimmableParent.DrawColourOffset;

            return Colour4.Black;
        }

        public DrawableStoryboardLayer(StoryboardLayer layer)
        {
            Layer = layer;
            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Enabled = layer.VisibleWhenPassing;
            Masking = layer.Masking;

            InternalChild = ElementContainer = new LayerElementContainer(layer);

            AddLayout(drawColourOffsetBacking);
        }

        public partial class LayerElementContainer : LifetimeManagementContainer, IColouredDimmable
        {
            private readonly StoryboardLayer storyboardLayer;

            public IEnumerable<Drawable> Elements => InternalChildren;

            private LayoutValue<Colour4> drawColourOffsetBacking = new LayoutValue<Colour4>(Invalidation.DrawNode | Invalidation.Colour | Invalidation.DrawInfo | Invalidation.RequiredParentSizeToFit | Invalidation.Presence);

            public Colour4 DrawColourOffset => drawColourOffsetBacking.IsValid ? drawColourOffsetBacking.Value : drawColourOffsetBacking.Value = computeDrawColourOffset();

            private Colour4 computeDrawColourOffset()
            {
                // Double `Parent` because DrawableStoryboard is a child of a Container, which is a child of a DimmableStoryboard.
                if (Parent is IColouredDimmable colouredDimmableParent)
                    return colouredDimmableParent.DrawColourOffset;

                return Colour4.Black;
            }

            public LayerElementContainer(StoryboardLayer layer)
            {
                storyboardLayer = layer;

                Size = new Vector2(640, 480);

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                AddLayout(drawColourOffsetBacking);
            }

            [BackgroundDependencyLoader]
            private void load(CancellationToken? cancellationToken)
            {
                foreach (var element in storyboardLayer.Elements)
                {
                    cancellationToken?.ThrowIfCancellationRequested();

                    if (element.IsDrawable)
                        AddInternal(element.CreateDrawable());
                }
            }
        }
    }
}

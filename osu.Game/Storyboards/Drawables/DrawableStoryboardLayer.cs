// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Storyboards.Drawables
{
    public class DrawableStoryboardLayer : CompositeDrawable
    {
        public StoryboardLayer Layer { get; }
        public bool Enabled;

        public override bool IsPresent => Enabled && base.IsPresent;

        public DrawableStoryboardLayer(StoryboardLayer layer)
        {
            Layer = layer;
            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Enabled = layer.VisibleWhenPassing;
            Masking = layer.Masking;

            InternalChild = new LayerElementContainer(layer);
        }

        private class LayerElementContainer : LifetimeManagementContainer
        {
            private readonly StoryboardLayer storyboardLayer;

            public LayerElementContainer(StoryboardLayer layer)
            {
                storyboardLayer = layer;

                Width = 640;
                Height = 480;
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
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

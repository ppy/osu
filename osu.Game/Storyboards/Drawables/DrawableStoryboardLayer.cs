// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Storyboards.Drawables
{
    public class DrawableStoryboardLayer : LifetimeManagementContainer
    {
        public StoryboardLayer Layer { get; private set; }
        public bool Enabled;

        public override bool IsPresent => Enabled && base.IsPresent;

        public DrawableStoryboardLayer(StoryboardLayer layer)
        {
            Layer = layer;
            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Enabled = layer.EnabledWhenPassing;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            foreach (var element in Layer.Elements)
            {
                if (element.IsDrawable)
                    AddInternal(element.CreateDrawable());
            }
        }
    }
}

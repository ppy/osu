// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Storyboards.Drawables
{
    public class DrawableStoryboardLayer : Container
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
                var drawable = element.CreateDrawable();
                if (drawable != null)
                    Add(drawable);
            }
        }
    }
}

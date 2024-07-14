// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Storyboards.Drawables;
using osuTK;

namespace osu.Game.Storyboards
{
    public partial class StoryboardVideoLayer : StoryboardLayer
    {
        public StoryboardVideoLayer(string name, int depth, bool masking)
            : base(name, depth, masking)
        {
        }

        public override DrawableStoryboardLayer CreateDrawable()
            => new DrawableStoryboardVideoLayer(this) { Depth = Depth, Name = Name };

        public partial class DrawableStoryboardVideoLayer : DrawableStoryboardLayer
        {
            public DrawableStoryboardVideoLayer(StoryboardVideoLayer layer)
                : base(layer)
            {
                // for videos we want to take on the full size of the storyboard container hierarchy
                // to allow the video to fill the full available region.
                ElementContainer.RelativeSizeAxes = Axes.Both;
                ElementContainer.Size = Vector2.One;
            }
        }
    }
}

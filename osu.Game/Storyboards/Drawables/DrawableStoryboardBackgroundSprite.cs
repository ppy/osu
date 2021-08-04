// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Storyboards.Drawables
{
    public class DrawableStoryboardBackgroundSprite : CompositeDrawable
    {
        public StoryboardBackgroundSprite Sprite;

        public override bool RemoveWhenNotAlive => false;

        public DrawableStoryboardBackgroundSprite(StoryboardBackgroundSprite sprite)
        {
            Sprite = sprite;
            LifetimeStart = sprite.StartTime;
            LifetimeEnd = sprite.EndTime;

            AutoSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textureStore, Storyboard storyboard)
        {
            var drawable = storyboard.CreateSpriteFromResourcePath(Sprite.Path, textureStore);

            if (drawable != null)
            {
                InternalChild = drawable;
                drawable.ScaleTo(480f / drawable.Height);
            }
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Storyboards.Drawables;
using osuTK;

namespace osu.Game.Storyboards
{
    public class StoryboardSprite : StoryboardElementWithDuration<DrawableStoryboardSprite>
    {
        public StoryboardSprite(string path, Anchor origin, Vector2 initialPosition)
            : base(path, origin, initialPosition)
        {
        }

        public override DrawableStoryboardSprite CreateStoryboardDrawable() => new DrawableStoryboardSprite(this);
    }
}

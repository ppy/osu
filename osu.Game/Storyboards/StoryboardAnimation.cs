// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Framework.Graphics;
using osu.Game.Storyboards.Drawables;

namespace osu.Game.Storyboards
{
    public class StoryboardAnimation : StoryboardElementWithDuration<DrawableStoryboardAnimation>
    {
        public int FrameCount;
        public double FrameDelay;
        public AnimationLoopType LoopType;

        public StoryboardAnimation(string path, Anchor origin, Vector2 initialPosition, int frameCount, double frameDelay, AnimationLoopType loopType)
            : base(path, origin, initialPosition)
        {
            FrameCount = frameCount;
            FrameDelay = frameDelay;
            LoopType = loopType;
        }

        public override DrawableStoryboardAnimation CreateStoryboardDrawable() => new DrawableStoryboardAnimation(this);
    }

    public enum AnimationLoopType
    {
        LoopForever,
        LoopOnce,
    }
}

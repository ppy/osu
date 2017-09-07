// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Game.Storyboards.Drawables;

namespace osu.Game.Storyboards
{
    public class AnimationDefinition : SpriteDefinition
    {
        public int FrameCount;
        public double FrameDelay;
        public AnimationLoopType LoopType;

        public AnimationDefinition(string path, Anchor origin, Vector2 initialPosition, int frameCount, double frameDelay, AnimationLoopType loopType)
            : base(path, origin, initialPosition)
        {
            FrameCount = frameCount;
            FrameDelay = frameDelay;
            LoopType = loopType;
        }

        public override Drawable CreateDrawable()
            => new StoryboardAnimation(this);
    }

    public enum AnimationLoopType
    {
        LoopForever,
        LoopOnce,
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Performance;
using osuTK;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatcherTrailEntry : LifetimeEntry<CatcherTrailEntry>
    {
        public readonly CatcherAnimationState CatcherState;

        public readonly float Position;

        /// <summary>
        /// The scaling of the catcher body. It also represents a flipped catcher (negative x component).
        /// </summary>
        public readonly Vector2 Scale;

        public readonly CatcherTrailAnimation Animation;

        public CatcherTrailEntry(double startTime, CatcherAnimationState catcherState, float position, Vector2 scale, CatcherTrailAnimation animation)
        {
            LifetimeStart = startTime;
            CatcherState = catcherState;
            Position = position;
            Scale = scale;
            Animation = animation;
        }
    }
}

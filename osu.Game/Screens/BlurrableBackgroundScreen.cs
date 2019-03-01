// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;
using osu.Game.Graphics.Backgrounds;
using osuTK;

namespace osu.Game.Screens
{
    public abstract class BlurrableBackgroundScreen : BackgroundScreen
    {
        protected Background Background;

        protected Vector2 BlurTarget;

        public TransformSequence<Background> BlurTo(Vector2 sigma, double duration, Easing easing = Easing.None)
            => Background?.BlurTo(BlurTarget = sigma, duration, easing);
    }
}

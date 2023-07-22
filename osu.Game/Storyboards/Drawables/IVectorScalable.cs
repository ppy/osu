// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;
using osuTK;

namespace osu.Game.Storyboards.Drawables
{
    internal interface IVectorScalable : ITransformable
    {
        Vector2 VectorScale { get; set; }
    }

    internal static class VectorScalableExtensions
    {
        public static TransformSequence<T> VectorScaleTo<T>(this T target, Vector2 newVectorScale, double duration = 0, Easing easing = Easing.None)
            where T : class, IVectorScalable
            => target.TransformTo(nameof(IVectorScalable.VectorScale), newVectorScale, duration, easing);
    }
}

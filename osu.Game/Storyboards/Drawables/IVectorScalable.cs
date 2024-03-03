// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Transforms;
using osuTK;

namespace osu.Game.Storyboards.Drawables
{
    internal interface IVectorScalable : ITransformable
    {
        Vector2 VectorScale { get; set; }
    }
}

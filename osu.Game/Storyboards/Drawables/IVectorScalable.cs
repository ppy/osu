// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osuTK;

namespace osu.Game.Storyboards.Drawables
{
    public interface IVectorScalable : IDrawable
    {
        Vector2 VectorScale { get; set; }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Transforms;
using osuTK;

namespace osu.Game.Storyboards.Drawables
{
    public interface IDrawableStoryboardElement : ITransformable
    {
        bool FlipH { get; set; }
        bool FlipV { get; set; }
        Vector2 VectorScale { get; set; }
    }
}

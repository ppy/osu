// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Graphics
{
    /// <summary>
    /// An interface for objects that provide colour offset to drawables.
    /// </summary>
    public interface IColouredDimmableSource : IDrawable
    {
        ColouredDimmableSharedData? ColouredDimmableSharedData { get; }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Denotes a drawable component which should be serialised as part of a skin.
    /// Use <see cref="ISkinnableComponent"/> for components which should be mutable by the user / editor.
    /// </summary>
    public interface ISkinSerialisable : IDrawable
    {
    }
}

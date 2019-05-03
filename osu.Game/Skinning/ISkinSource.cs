// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Provides access to skinnable elements.
    /// </summary>
    public interface ISkinSource : ISkin
    {
        event Action SourceChanged;
    }
}

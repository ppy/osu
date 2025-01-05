// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A lookup type intended for use for skinnable components.
    /// </summary>
    /// <typeparam name="T">An enum lookup type.</typeparam>
    public class SkinComponentLookup<T> : ISkinComponentLookup
        where T : Enum
    {
        public readonly T Component;

        public SkinComponentLookup(T component)
        {
            Component = component;
        }
    }
}

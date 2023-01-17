// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Skinning
{
    public class UnsupportedSkinComponentException : Exception
    {
        public UnsupportedSkinComponentException(ISkinComponentLookup lookup)
            : base($@"Unsupported component type: {lookup.GetType()} (lookup: ""{lookup}"").")
        {
        }
    }
}

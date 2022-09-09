// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;

namespace osu.Game.Skinning
{
    public class UnsupportedSkinComponentException : Exception
    {
        public UnsupportedSkinComponentException(ISkinComponent component)
            : base($@"Unsupported component type: {component.GetType()} (lookup: ""{component.LookupName}"").")
        {
        }
    }
}

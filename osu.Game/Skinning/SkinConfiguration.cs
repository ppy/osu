// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;

namespace osu.Game.Skinning
{
    /// <summary>
    /// An empty skin configuration.
    /// </summary>
    public class SkinConfiguration
    {
        public readonly SkinInfo SkinInfo = new SkinInfo();

        public ColourConfiguration Colours = new ColourConfiguration();

        public readonly Dictionary<string, string> ConfigDictionary = new Dictionary<string, string>();
    }
}

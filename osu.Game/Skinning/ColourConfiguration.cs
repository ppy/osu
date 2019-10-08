// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Graphics;
using System.Collections.Generic;

namespace osu.Game.Skinning
{
    public class ColourConfiguration
    {
        public List<Color4> ComboColours { get; set; } = new List<Color4>();

        public Dictionary<string, Color4> CustomColours { get; set; } = new Dictionary<string, Color4>();
    }
}

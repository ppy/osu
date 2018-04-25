// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Beatmaps.Formats;
using OpenTK.Graphics;

namespace osu.Game.Skinning
{
    public class SkinConfiguration : IHasComboColours, IHasCustomColours
    {
        public readonly SkinInfo SkinInfo = new SkinInfo();

        public List<Color4> ComboColours { get; set; } = new List<Color4>();

        public Dictionary<string, Color4> CustomColours { get; set; } = new Dictionary<string, Color4>();
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps.Formats;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    public class SkinConfiguration : IHasComboColours, IHasCustomColours
    {
        public readonly SkinInfo SkinInfo = new SkinInfo();

        public List<Color4> ComboColours { get; set; } = new List<Color4>
        {
            new Color4(17, 136, 170, 255),
            new Color4(102, 136, 0, 255),
            new Color4(204, 102, 0, 255),
            new Color4(121, 9, 13, 255)
        };

        public Dictionary<string, Color4> CustomColours { get; set; } = new Dictionary<string, Color4>();

        public string HitCircleFont { get; set; } = "default";

        public int HitCircleOverlap { get; set; }

        public bool? CursorExpand { get; set; } = true;
    }
}

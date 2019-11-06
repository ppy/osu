// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps.Formats;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    /// <summary>
    /// An empty skin configuration.
    /// </summary>
    public class SkinConfiguration : IHasComboColours, IHasCustomColours
    {
        public readonly SkinInfo SkinInfo = new SkinInfo();

        internal bool AllowDefaultColoursFallback;

        public static List<Color4> DefaultComboColours = new List<Color4>
        {
            new Color4(255, 192, 0, 255),
            new Color4(0, 202, 0, 255),
            new Color4(18, 124, 255, 255),
            new Color4(242, 24, 57, 255),
        };

        private List<Color4> comboColours = new List<Color4>();

        public List<Color4> ComboColours
        {
            get
            {
                if (comboColours.Count > 0)
                    return comboColours;

                if (AllowDefaultColoursFallback)
                    return DefaultComboColours;

                return null;
            }
            set => comboColours = value;
        }

        public Dictionary<string, Color4> CustomColours { get; set; } = new Dictionary<string, Color4>();

        public readonly Dictionary<string, string> ConfigDictionary = new Dictionary<string, string>();
    }
}

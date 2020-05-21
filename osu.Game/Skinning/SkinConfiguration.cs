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

        /// <summary>
        /// Whether to allow <see cref="DefaultComboColours"/> as a fallback list for when no combo colours are provided.
        /// </summary>
        internal bool AllowDefaultComboColoursFallback = true;

        public static List<Colour4> DefaultComboColours { get; } = new List<Colour4>
        {
            new Colour4(255, 192, 0, 255),
            new Colour4(0, 202, 0, 255),
            new Colour4(18, 124, 255, 255),
            new Colour4(242, 24, 57, 255),
        };

        private readonly List<Colour4> comboColours = new List<Colour4>();

        public IReadOnlyList<Colour4> ComboColours
        {
            get
            {
                if (comboColours.Count > 0)
                    return comboColours;

                if (AllowDefaultComboColoursFallback)
                    return DefaultComboColours;

                return null;
            }
        }

        public void AddComboColours(params Colour4[] colours) => comboColours.AddRange(colours);

        public Dictionary<string, Colour4> CustomColours { get; set; } = new Dictionary<string, Colour4>();

        public readonly Dictionary<string, string> ConfigDictionary = new Dictionary<string, string>();
    }
}

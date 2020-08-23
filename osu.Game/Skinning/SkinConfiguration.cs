// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps.Formats;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    /// <summary>
    /// An empty skin configuration.
    /// </summary>
    public class SkinConfiguration : IEquatable<SkinConfiguration>, IHasComboColours, IHasCustomColours
    {
        public readonly SkinInfo SkinInfo = new SkinInfo();

        /// <summary>
        /// Whether to allow <see cref="DefaultComboColours"/> as a fallback list for when no combo colours are provided.
        /// </summary>
        internal bool AllowDefaultComboColoursFallback = true;

        public static List<Color4> DefaultComboColours { get; } = new List<Color4>
        {
            new Color4(255, 192, 0, 255),
            new Color4(0, 202, 0, 255),
            new Color4(18, 124, 255, 255),
            new Color4(242, 24, 57, 255),
        };

        private readonly List<Color4> comboColours = new List<Color4>();

        public IReadOnlyList<Color4> ComboColours
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

        public void AddComboColours(params Color4[] colours) => comboColours.AddRange(colours);

        public IDictionary<string, Color4> CustomColours { get; } = new SortedDictionary<string, Color4>();

        public readonly SortedDictionary<string, string> ConfigDictionary = new SortedDictionary<string, string>();

        public bool Equals(SkinConfiguration other) => other != null && ConfigDictionary.SequenceEqual(other.ConfigDictionary) && ComboColours.SequenceEqual(other.ComboColours) && CustomColours?.SequenceEqual(other.CustomColours) == true;
    }
}

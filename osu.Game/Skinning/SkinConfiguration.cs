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
    public class SkinConfiguration : IHasComboColours, IHasCustomColours, IHasSnapColours
    {
        public readonly SkinInfo SkinInfo = new SkinInfo();

        public const decimal LATEST_VERSION = 2.7m;

        /// <summary>
        /// Whether to allow <see cref="DefaultComboColours"/> as a fallback list for when no combo colours are provided.
        /// </summary>
        internal bool AllowDefaultComboColoursFallback = true;

        internal bool AllowDefaultSnapColoursFallback = true;

        /// <summary>
        /// Legacy version of this skin.
        /// </summary>
        public decimal? LegacyVersion { get; internal set; }

        public enum LegacySetting
        {
            Version,
            ComboPrefix,
            ComboOverlap,
            ScorePrefix,
            ScoreOverlap,
            HitCirclePrefix,
            HitCircleOverlap,
            AnimationFramerate,
            LayeredHitSounds,
            AllowSliderBallTint,
        }

        public static List<Color4> DefaultComboColours { get; } = new List<Color4>
        {
            new Color4(255, 192, 0, 255),
            new Color4(0, 202, 0, 255),
            new Color4(18, 124, 255, 255),
            new Color4(242, 24, 57, 255),
        };

        public static List<Color4> DefaultSnapColours { get; } = new List<Color4>
        {
            new Color4(255, 255, 255, 255),
            new Color4(255, 0, 0, 255),
            new Color4(0, 0, 255, 255),
            new Color4(102, 45, 145, 255),
            new Color4(255, 255, 0, 255),
            new Color4(153, 153, 153, 255),
            new Color4(255, 0, 255, 255),
            new Color4(247, 148, 29, 255),
            new Color4(0, 255, 255, 255),
            new Color4(0, 198, 0, 255),
            new Color4(255, 153, 153, 255),
            new Color4(153, 143, 255, 255)
        };

        public List<Color4> CustomComboColours { get; set; } = new List<Color4>();

        public List<Color4> CustomSnapColours { get; set; } = new List<Color4>();

        public IReadOnlyList<Color4>? ComboColours
        {
            get
            {
                if (CustomComboColours.Count > 0)
                    return CustomComboColours;

                if (AllowDefaultComboColoursFallback)
                    return DefaultComboColours;

                return null;
            }
        }

        public IReadOnlyList<Color4>? SnapColours
        {
            get
            {
                if (CustomSnapColours.Count > 0)
                    return CustomSnapColours;

                if (AllowDefaultSnapColoursFallback)
                    return DefaultSnapColours;

                return null;
            }
        }

        public Dictionary<string, Color4> CustomColours { get; } = new Dictionary<string, Color4>();

        public readonly Dictionary<string, string> ConfigDictionary = new Dictionary<string, string>();
    }
}

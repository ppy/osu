// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// A default implementation of an icon used to represent beatmap statistics.
    /// </summary>
    public class BeatmapStatisticIcon : Sprite
    {
        private readonly BeatmapStatisticsIconType iconType;

        public BeatmapStatisticIcon(BeatmapStatisticsIconType iconType)
        {
            this.iconType = iconType;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            Texture = textures.Get($"Icons/BeatmapDetails/{iconType.ToString().Kebaberize()}");
        }
    }

    public enum BeatmapStatisticsIconType
    {
        Accuracy,
        ApproachRate,
        Bpm,
        Circles,
        HpDrain,
        Length,
        OverallDifficulty,
        Size,
        Sliders,
        Spinners,
    }
}

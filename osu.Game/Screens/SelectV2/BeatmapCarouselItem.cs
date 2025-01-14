// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;
using osu.Game.Database;

namespace osu.Game.Screens.SelectV2
{
    public class BeatmapCarouselItem : CarouselItem
    {
        public readonly Guid ID;

        public override float DrawHeight => Model is BeatmapInfo ? 40 : 80;

        public BeatmapCarouselItem(object model)
            : base(model)
        {
            ID = (Model as IHasGuidPrimaryKey)?.ID ?? Guid.NewGuid();
        }

        public override string? ToString()
        {
            switch (Model)
            {
                case BeatmapInfo bi:
                    return $"Difficulty: {bi.DifficultyName} ({bi.StarRating:N1}*)";

                case BeatmapSetInfo si:
                    return $"{si.Metadata}";
            }

            return Model.ToString();
        }
    }
}

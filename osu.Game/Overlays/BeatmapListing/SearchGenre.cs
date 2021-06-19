// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.ComponentModel;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.BeatmapListing
{
    [LocalisableEnum(typeof(SearchGenreEnumLocalisationMapper))]
    public enum SearchGenre
    {
        [Description("任意")]
        Any = 0,
        [Description("未指定")]
        Unspecified = 1,

        [Description("游戏")]
        VideoGame = 2,
        [Description("动漫")]
        Anime = 3,
        [Description("摇滚")]
        Rock = 4,
        [Description("流行")]
        Pop = 5,
        [Description("其他")]
        Other = 6,
        [Description("新奇")]
        Novelty = 7,

        [Description("嘻哈")]
        HipHop = 9,
        [Description("电子")]
        Electronic = 10,
        [Description("金属")]
        Metal = 11,
        [Description("古典")]
        Classical = 12,
        [Description("民歌")]
        Folk = 13,
        [Description("爵士")]
        Jazz = 14
    }

    public class SearchGenreEnumLocalisationMapper : EnumLocalisationMapper<SearchGenre>
    {
        public override LocalisableString Map(SearchGenre value)
        {
            switch (value)
            {
                case SearchGenre.Any:
                    return BeatmapsStrings.GenreAny;

                case SearchGenre.Unspecified:
                    return BeatmapsStrings.GenreUnspecified;

                case SearchGenre.VideoGame:
                    return BeatmapsStrings.GenreVideoGame;

                case SearchGenre.Anime:
                    return BeatmapsStrings.GenreAnime;

                case SearchGenre.Rock:
                    return BeatmapsStrings.GenreRock;

                case SearchGenre.Pop:
                    return BeatmapsStrings.GenrePop;

                case SearchGenre.Other:
                    return BeatmapsStrings.GenreOther;

                case SearchGenre.Novelty:
                    return BeatmapsStrings.GenreNovelty;

                case SearchGenre.HipHop:
                    return BeatmapsStrings.GenreHipHop;

                case SearchGenre.Electronic:
                    return BeatmapsStrings.GenreElectronic;

                case SearchGenre.Metal:
                    return BeatmapsStrings.GenreMetal;

                case SearchGenre.Classical:
                    return BeatmapsStrings.GenreClassical;

                case SearchGenre.Folk:
                    return BeatmapsStrings.GenreFolk;

                case SearchGenre.Jazz:
                    return BeatmapsStrings.GenreJazz;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}

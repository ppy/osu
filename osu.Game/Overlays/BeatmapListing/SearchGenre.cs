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
        Any = 0,
        Unspecified = 1,

        [Description("Video Game")]
        VideoGame = 2,
        Anime = 3,
        Rock = 4,
        Pop = 5,
        Other = 6,
        Novelty = 7,

        [Description("Hip Hop")]
        HipHop = 9,
        Electronic = 10,
        Metal = 11,
        Classical = 12,
        Folk = 13,
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

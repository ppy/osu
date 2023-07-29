// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.BeatmapListing
{
    public enum SearchGenre
    {
        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.GenreAny))]
        Any = 0,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.GenreUnspecified))]
        Unspecified = 1,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.GenreVideoGame))]
        [Description("Video Game")]
        VideoGame = 2,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.GenreAnime))]
        Anime = 3,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.GenreRock))]
        Rock = 4,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.GenrePop))]
        Pop = 5,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.GenreOther))]
        Other = 6,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.GenreNovelty))]
        Novelty = 7,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.GenreHipHop))]
        [Description("Hip Hop")]
        HipHop = 9,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.GenreElectronic))]
        Electronic = 10,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.GenreMetal))]
        Metal = 11,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.GenreClassical))]
        Classical = 12,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.GenreFolk))]
        Folk = 13,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.GenreJazz))]
        Jazz = 14
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Localisation;

#nullable enable

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Metadata representing a beatmap. May be shared between multiple beatmap difficulties.
    /// </summary>
    public interface IBeatmapMetadataInfo : IEquatable<IBeatmapMetadataInfo>
    {
        /// <summary>
        /// The romanised title of this beatmap.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// The unicode title of this beatmap.
        /// </summary>
        string TitleUnicode { get; }

        /// <summary>
        /// The romanised artist of this beatmap.
        /// </summary>
        string Artist { get; }

        /// <summary>
        /// The unicode artist of this beatmap.
        /// </summary>
        string ArtistUnicode { get; }

        /// <summary>
        /// The author of this beatmap.
        /// </summary>
        string Author { get; } // eventually should be linked to a persisted User.

        /// <summary>
        /// The source of this beatmap.
        /// </summary>
        string Source { get; }

        /// <summary>
        /// The tags of this beatmap.
        /// </summary>
        string Tags { get; }

        /// <summary>
        /// The time in milliseconds to begin playing the track for preview purposes.
        /// If -1, the track should begin playing at 40% of its length.
        /// </summary>
        int PreviewTime { get; }

        /// <summary>
        /// The filename of the audio file consumed by this beatmap.
        /// </summary>
        string AudioFile { get; }

        /// <summary>
        /// The filename of the background image file consumed by this beatmap.
        /// </summary>
        string BackgroundFile { get; }

        /// <summary>
        /// A user-presentable display title representing this metadata.
        /// </summary>
        string DisplayTitle
        {
            get
            {
                string author = string.IsNullOrEmpty(Author) ? string.Empty : $"({Author})";
                return $"{Artist} - {Title} {author}".Trim();
            }
        }

        /// <summary>
        /// A user-presentable display title representing this metadata, with localisation handling for potentially romanisable fields.
        /// </summary>
        RomanisableString DisplayTitleRomanisable
        {
            get
            {
                string author = string.IsNullOrEmpty(Author) ? string.Empty : $"({Author})";
                var artistUnicode = string.IsNullOrEmpty(ArtistUnicode) ? Artist : ArtistUnicode;
                var titleUnicode = string.IsNullOrEmpty(TitleUnicode) ? Title : TitleUnicode;

                return new RomanisableString($"{artistUnicode} - {titleUnicode} {author}".Trim(), $"{Artist} - {Title} {author}".Trim());
            }
        }

        /// <summary>
        /// An array of all searchable terms provided in contained metadata.
        /// </summary>
        string[] SearchableTerms => new[]
        {
            Author,
            Artist,
            ArtistUnicode,
            Title,
            TitleUnicode,
            Source,
            Tags
        }.Where(s => !string.IsNullOrEmpty(s)).ToArray();

        bool IEquatable<IBeatmapMetadataInfo>.Equals(IBeatmapMetadataInfo? other)
        {
            if (other == null)
                return false;

            return Title == other.Title
                   && TitleUnicode == other.TitleUnicode
                   && Artist == other.Artist
                   && ArtistUnicode == other.ArtistUnicode
                   && Author == other.Author
                   && Source == other.Source
                   && Tags == other.Tags
                   && PreviewTime == other.PreviewTime
                   && AudioFile == other.AudioFile
                   && BackgroundFile == other.BackgroundFile;
        }
    }
}

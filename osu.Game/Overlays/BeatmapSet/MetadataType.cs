// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.BeatmapSet
{
    public enum MetadataType
    {
        [Description("User Tags")] // TODO: use translated string after osu-resources update
        UserTags,

        [Description("Mapper Tags")] // TODO: use translated string after osu-resources update
        MapperTags,

        [LocalisableDescription(typeof(BeatmapsetsStrings), nameof(BeatmapsetsStrings.ShowInfoSource))]
        Source,

        [LocalisableDescription(typeof(BeatmapsetsStrings), nameof(BeatmapsetsStrings.ShowInfoDescription))]
        Description,

        [LocalisableDescription(typeof(BeatmapsetsStrings), nameof(BeatmapsetsStrings.ShowInfoGenre))]
        Genre,

        [LocalisableDescription(typeof(BeatmapsetsStrings), nameof(BeatmapsetsStrings.ShowInfoLanguage))]
        Language,

        [LocalisableDescription(typeof(BeatmapsetsStrings), nameof(BeatmapsetsStrings.ShowInfoNominators))]
        Nominators,
    }
}

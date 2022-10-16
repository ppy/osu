// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Comments
{
    public enum CommentReportReason
    {
        [LocalisableDescription(typeof(UsersStrings), nameof(UsersStrings.ReportOptionsInsults))]
        Insults,

        [LocalisableDescription(typeof(UsersStrings), nameof(UsersStrings.ReportOptionsSpam))]
        Spam,

        [LocalisableDescription(typeof(UsersStrings), nameof(UsersStrings.ReportOptionsUnwantedContent))]
        UnwantedContent,

        [LocalisableDescription(typeof(UsersStrings), nameof(UsersStrings.ReportOptionsNonsense))]
        Nonsense,

        [LocalisableDescription(typeof(UsersStrings), nameof(UsersStrings.ReportOptionsOther))]
        Other
    }
}

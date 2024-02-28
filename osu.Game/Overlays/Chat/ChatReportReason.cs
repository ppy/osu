// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Chat
{
    /// <remarks>
    /// References:
    /// https://github.com/ppy/osu-web/blob/0a41b13acf5f47bb0d2b08bab42a9646b7ab5821/app/Models/UserReport.php#L50
    /// https://github.com/ppy/osu-web/blob/0a41b13acf5f47bb0d2b08bab42a9646b7ab5821/app/Models/UserReport.php#L39
    /// </remarks>
    public enum ChatReportReason
    {
        [Description("Insulting People")]
        [LocalisableDescription(typeof(UsersStrings), nameof(UsersStrings.ReportOptionsInsults))]
        Insults,

        [Description("Spam")]
        [LocalisableDescription(typeof(UsersStrings), nameof(UsersStrings.ReportOptionsSpam))]
        Spam,

        [Description("Unwanted Content")]
        [LocalisableDescription(typeof(UsersStrings), nameof(UsersStrings.ReportOptionsUnwantedContent))]
        UnwantedContent,

        [Description("Nonsense")]
        [LocalisableDescription(typeof(UsersStrings), nameof(UsersStrings.ReportOptionsNonsense))]
        Nonsense,

        [Description("Other")]
        [LocalisableDescription(typeof(UsersStrings), nameof(UsersStrings.ReportOptionsOther))]
        Other
    }
}

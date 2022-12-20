// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Chat
{
    public enum ChatReportReason
    {
        [Description("Insulting People")]
        [LocalisableDescription(typeof(UsersStrings), nameof(UsersStrings.ReportOptionsInsults))]
        Insults,

        [Description("Spam")]
        [LocalisableDescription(typeof(UsersStrings), nameof(UsersStrings.ReportOptionsSpam))]
        Spam,

        [Description("Cheating")]
        [LocalisableDescription(typeof(UsersStrings), nameof(UsersStrings.ReportOptionsCheating))]
        FoulPlay,

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

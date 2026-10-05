// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Profile
{
    public enum UserReportReason
    {
        [LocalisableDescription(typeof(UsersStrings), nameof(UsersStrings.ReportOptionsCheating))]
        Cheating,

        [LocalisableDescription(typeof(UsersStrings), nameof(UsersStrings.ReportOptionsMultipleAccounts))]
        MultipleAccounts,

        [LocalisableDescription(typeof(UsersStrings), nameof(UsersStrings.ReportOptionsInappropriateChat))]
        InappropriateChat,

        [LocalisableDescription(typeof(UsersStrings), nameof(UsersStrings.ReportOptionsUnwantedContent))]
        UnwantedContent,

        [LocalisableDescription(typeof(UsersStrings), nameof(UsersStrings.ReportOptionsOther))]
        Other,
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Team
{
    /// <remarks>
    /// References:
    /// https://github.com/ppy/osu-web/blob/4579103d681d98851e40f51d50f7243d999968b0/app/Models/UserReport.php#L54
    /// </remarks>
    public enum TeamReportReason
    {
        [LocalisableDescription(typeof(UsersStrings), nameof(UsersStrings.ReportOptionsUnwantedContent))]
        UnwantedContent,

        [LocalisableDescription(typeof(UsersStrings), nameof(UsersStrings.ReportOptionsOther))]
        Other,
    }
}

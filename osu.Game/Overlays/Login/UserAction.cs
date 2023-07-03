// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Login
{
    public enum UserAction
    {
        [LocalisableDescription(typeof(UsersStrings), nameof(UsersStrings.StatusOnline))]
        Online,

        [LocalisableDescription(typeof(LoginPanelStrings), nameof(LoginPanelStrings.DoNotDisturb))]
        DoNotDisturb,

        [LocalisableDescription(typeof(LoginPanelStrings), nameof(LoginPanelStrings.AppearOffline))]
        AppearOffline,

        [LocalisableDescription(typeof(LoginPanelStrings), nameof(LoginPanelStrings.SignOut))]
        SignOut,
    }
}

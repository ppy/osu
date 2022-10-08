// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.ComponentModel;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Login
{
    public enum UserAction
    {
        [LocalisableDescription(typeof(UsersStrings), nameof(UsersStrings.StatusOnline))]
        Online,

        [Description(@"请勿打扰")]
        DoNotDisturb,

        [Description(@"隐身")]
        AppearOffline,

        [Description(@"登出")]
        SignOut,
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Overlays.Login
{
    public enum UserAction
    {
        [Description("在线")]
        Online,

        [Description(@"请勿打扰")]
        DoNotDisturb,

        [Description(@"隐身")]
        AppearOffline,

        [Description(@"登出")]
        SignOut,
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Overlays.Login
{
    public enum UserAction
    {
        Online,

        [Description(@"Do not disturb")]
        DoNotDisturb,

        [Description(@"Appear offline")]
        AppearOffline,

        [Description(@"Sign out")]
        SignOut,
    }
}
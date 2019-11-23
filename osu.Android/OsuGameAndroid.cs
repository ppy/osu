// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Android.App;
using osu.Game;

namespace osu.Android
{
    public class OsuGameAndroid : OsuGame
    {
        public override Version AssemblyVersion => new Version(Application.Context.ApplicationContext.PackageManager.GetPackageInfo(Application.Context.ApplicationContext.PackageName, 0).VersionName);
    }
}

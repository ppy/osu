// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Android.App;
using osu.Game;

namespace osu.Android
{
    public class OsuGameAndroid : OsuGame
    {
        public override Version AssemblyVersion
        {
            get
            {
                string versionName = Application.Context.ApplicationContext.PackageManager.GetPackageInfo(Application.Context.ApplicationContext.PackageName, 0).VersionCode.ToString();

                try
                {
                    // undo play store version garbling
                    return new Version(int.Parse(versionName.Substring(0, 4)), int.Parse(versionName.Substring(4, 4)), int.Parse(versionName.Substring(8, 1)));
                }
                catch
                {
                }

                return new Version(versionName);
            }
        }
    }
}
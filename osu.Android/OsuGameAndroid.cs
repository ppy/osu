// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Android.App;
using osu.Game;
using osu.Game.Updater;

namespace osu.Android
{
    public class OsuGameAndroid : OsuGame
    {
        public override Version AssemblyVersion
        {
            get
            {
                var packageInfo = Application.Context.ApplicationContext.PackageManager.GetPackageInfo(Application.Context.ApplicationContext.PackageName, 0);

                try
                {
                    // todo: needs checking before play store redeploy.
                    string versionName = packageInfo.VersionName;
                    // undo play store version garbling
                    return new Version(int.Parse(versionName.Substring(0, 4)), int.Parse(versionName.Substring(4, 4)), int.Parse(versionName.Substring(8, 1)));
                }
                catch
                {
                }

                return new Version(packageInfo.VersionName);
            }
        }

        protected override UpdateManager CreateUpdateManager() => new SimpleUpdateManager();
    }
}
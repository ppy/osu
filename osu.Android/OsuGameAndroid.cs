// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using Android.App;
using osu.Game;

namespace osu.Android
{
    public class OsuGameAndroid : OsuGame
    {
        public override Version AssemblyVersion => new Version(Application.Context.ApplicationContext.PackageManager.GetPackageInfo(Application.Context.ApplicationContext.PackageName, 0).VersionName);

        private string fileToLoad;

        public OsuGameAndroid(params string[] args)
        {
            fileToLoad = args?.Length > 0 ? args[0] : null;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if(fileToLoad != null)
            {
                Task.Factory.StartNew(() => Import(fileToLoad), TaskCreationOptions.LongRunning);
            }
        }
    }
}

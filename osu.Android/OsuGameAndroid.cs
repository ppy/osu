// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using Android.App;
using osu.Game;
using System.Collections.Generic;

namespace osu.Android
{
    public class OsuGameAndroid : OsuGame
    {
        public override Version AssemblyVersion => new Version(Application.Context.ApplicationContext.PackageManager.GetPackageInfo(Application.Context.ApplicationContext.PackageName, 0).VersionName);

        public OsuGameAndroid(params string[] args)
        {
            
        }

        public List<string> FilesToImport
        {
            get;
            set;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            if(FilesToImport != null && FilesToImport.Count > 0)
            {
                Task.Factory.StartNew(() => Import(FilesToImport.ToArray()), TaskCreationOptions.LongRunning);
            }
        }
    }
}

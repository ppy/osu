// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Util;
using Android.Net;
using Android.Content;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Framework.Android;

namespace osu.Android
{
    public class OsuGameActivity : AndroidGameActivity
    {
        protected override Framework.Game CreateGame()
        {
            return new OsuGameAndroid() { FilesToImport = getImportFilePath() };
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Window.AddFlags(WindowManagerFlags.Fullscreen);
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);
        }

        private List<string> getImportFilePath()
        {
            List<string> l = new List<string>();
            if (Intent.Action != null
                && Intent.Action.Equals("android.intent.action.VIEW"))
            {
                Uri uri = Intent.Data;
                if (ContentResolver.SchemeFile.Equals(uri.Scheme))
                {
                    l.Add(uri.Path);
                }
            }
            try
            {
                //most browsers download files to the same directory by default, scan the directory to auto import the downloaded osz
                Java.IO.File downloads = Environment.GetExternalStoragePublicDirectory(Environment.DirectoryDownloads);
                if (downloads.Exists())
                {
                    l.AddRange(new List<Java.IO.File>(downloads.ListFiles()).Where(n => n.IsFile && n.Name.EndsWith(".osz")).Select(n => n.AbsolutePath));
                }
            }
            catch (System.Exception e)
            {
                Log.Error("osu!lazer", e.ToString());
            }
            return l;
        }
    }
}

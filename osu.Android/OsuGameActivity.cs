﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.OS;
using Android.Provider;
using Android.Views;
using osu.Framework.Android;
using osu.Game.Database;

namespace osu.Android
{
    [Activity(Theme = "@android:style/Theme.NoTitleBar", MainLauncher = true, ScreenOrientation = ScreenOrientation.FullUser, SupportsPictureInPicture = false, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize, HardwareAccelerated = false, LaunchMode = LaunchMode.SingleInstance, Exported = true)]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault }, DataScheme = "content", DataPathPattern = ".*\\\\.osz", DataHost = "*", DataMimeType = "*/*")]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault }, DataScheme = "content", DataPathPattern = ".*\\\\.osk", DataHost = "*", DataMimeType = "*/*")]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault }, DataScheme = "content", DataMimeType = "application/x-osu-archive")]
    [IntentFilter(new[] { Intent.ActionSend, Intent.ActionSendMultiple }, Categories = new[] { Intent.CategoryDefault }, DataMimeTypes = new[] { "application/zip", "application/octet-stream", "application/download", "application/x-zip", "application/x-zip-compressed", "application/x-osu-archive" })]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault }, DataSchemes = new[] { "osu", "osump" })]
    public class OsuGameActivity : AndroidGameActivity
    {
        private static readonly string[] osu_url_schemes = { "osu", "osump" };

        private OsuGameAndroid game;

        protected override Framework.Game CreateGame() => game = new OsuGameAndroid(this);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            // The default current directory on android is '/'.
            // On some devices '/' maps to the app data directory. On others it maps to the root of the internal storage.
            // In order to have a consistent current directory on all devices the full path of the app data directory is set as the current directory.
            System.Environment.CurrentDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

            base.OnCreate(savedInstanceState);

            // OnNewIntent() only fires for an activity if it's *re-launched* while it's on top of the activity stack.
            // on first launch we still have to fire manually.
            // reference: https://developer.android.com/reference/android/app/Activity#onNewIntent(android.content.Intent)
            handleIntent(Intent);

            Window.AddFlags(WindowManagerFlags.Fullscreen);
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);
        }

        protected override void OnNewIntent(Intent intent) => handleIntent(intent);

        private void handleIntent(Intent intent)
        {
            switch (intent.Action)
            {
                case Intent.ActionDefault:
                    if (intent.Scheme == ContentResolver.SchemeContent)
                        handleImportFromUris(intent.Data);
                    else if (osu_url_schemes.Contains(intent.Scheme))
                        game.HandleLink(intent.DataString);
                    break;

                case Intent.ActionSend:
                case Intent.ActionSendMultiple:
                {
                    var uris = new List<Uri>();
                    for (int i = 0; i < intent.ClipData?.ItemCount; i++)
                    {
                        var content = intent.ClipData?.GetItemAt(i);
                        if (content != null)
                            uris.Add(content.Uri);
                    }
                    handleImportFromUris(uris.ToArray());
                    break;
                }
            }
        }

        private void handleImportFromUris(params Uri[] uris) => Task.Factory.StartNew(async () =>
        {
            var tasks = new List<ImportTask>();

            await Task.WhenAll(uris.Select(async uri =>
            {
                // there are more performant overloads of this method, but this one is the most backwards-compatible
                // (dates back to API 1).
                var cursor = ContentResolver?.Query(uri, null, null, null, null);

                if (cursor == null)
                    return;

                cursor.MoveToFirst();

                var filenameColumn = cursor.GetColumnIndex(OpenableColumns.DisplayName);
                string filename = cursor.GetString(filenameColumn);

                // SharpCompress requires archive streams to be seekable, which the stream opened by
                // OpenInputStream() seems to not necessarily be.
                // copy to an arbitrary-access memory stream to be able to proceed with the import.
                var copy = new MemoryStream();
                using (var stream = ContentResolver.OpenInputStream(uri))
                    await stream.CopyToAsync(copy).ConfigureAwait(false);

                lock (tasks)
                {
                    tasks.Add(new ImportTask(copy, filename));
                }
            })).ConfigureAwait(false);

            await game.Import(tasks.ToArray()).ConfigureAwait(false);
        }, TaskCreationOptions.LongRunning);
    }
}

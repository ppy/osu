// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AndroidApp = global::Android.App;
using AndroidContent = global::Android.Content;
using AndroidPM = global::Android.Content.PM;
using AndroidRes = global::Android.Content.Res;
using AndroidGraphics = global::Android.Graphics;
using AndroidInput = global::Android.Hardware.Input;
using AndroidOS = global::Android.OS;
using AndroidViews = global::Android.Views;
using osu.Framework.Android;
using osu.Game.Database;
using Debug = System.Diagnostics.Debug;
using Uri = global::Android.Net.Uri;

namespace osu.Android
{
    [AndroidApp.Activity(ConfigurationChanges = DEFAULT_CONFIG_CHANGES, Exported = true, LaunchMode = DEFAULT_LAUNCH_MODE, MainLauncher = true)]
    [AndroidContent.IntentFilter(new[] { AndroidContent.Intent.ActionView }, Categories = new[] { AndroidContent.Intent.CategoryDefault }, DataScheme = "content", DataPathPattern = ".*\\\\.osz", DataHost = "*", DataMimeType = "*/*")]
    [AndroidContent.IntentFilter(new[] { AndroidContent.Intent.ActionView }, Categories = new[] { AndroidContent.Intent.CategoryDefault }, DataScheme = "content", DataPathPattern = ".*\\\\.osk", DataHost = "*", DataMimeType = "*/*")]
    [AndroidContent.IntentFilter(new[] { AndroidContent.Intent.ActionView }, Categories = new[] { AndroidContent.Intent.CategoryDefault }, DataScheme = "content", DataPathPattern = ".*\\\\.osr", DataHost = "*", DataMimeType = "*/*")]
    [AndroidContent.IntentFilter(new[] { AndroidContent.Intent.ActionView }, Categories = new[] { AndroidContent.Intent.CategoryDefault }, DataScheme = "content", DataMimeType = "application/x-osu-beatmap-archive")]
    [AndroidContent.IntentFilter(new[] { AndroidContent.Intent.ActionView }, Categories = new[] { AndroidContent.Intent.CategoryDefault }, DataScheme = "content", DataMimeType = "application/x-osu-skin-archive")]
    [AndroidContent.IntentFilter(new[] { AndroidContent.Intent.ActionView }, Categories = new[] { AndroidContent.Intent.CategoryDefault }, DataScheme = "content", DataMimeType = "application/x-osu-replay")]
    [AndroidContent.IntentFilter(new[] { AndroidContent.Intent.ActionSend, AndroidContent.Intent.ActionSendMultiple }, Categories = new[] { AndroidContent.Intent.CategoryDefault }, DataMimeTypes = new[]
    {
        "application/zip",
        "application/octet-stream",
        "application/download",
        "application/x-zip",
        "application/x-zip-compressed",
        // newer official mime types (see https://osu.ppy.sh/wiki/en/osu%21_File_Formats).
        "application/x-osu-beatmap-archive",
        "application/x-osu-skin-archive",
        "application/x-osu-replay",
    })]
    [AndroidContent.IntentFilter(new[] { AndroidContent.Intent.ActionView }, Categories = new[] { AndroidContent.Intent.CategoryBrowsable, AndroidContent.Intent.CategoryDefault }, DataSchemes = new[] { "osu", "osump" })]
    public class OsuGameActivity : AndroidGameActivity
    {
        public override bool DispatchTouchEvent(AndroidViews.MotionEvent? e)
        {
            if (e != null)
            {
                for (int i = 0; i < e.PointerCount; i++)
                {
                    var toolType = e.GetToolType(i);
                    if (toolType == AndroidViews.MotionEventToolType.Stylus)
                    {
                        // S Pen detected. Hardware timestamps should be used for improved latency.
                        long timestampNano = AndroidOS.Build.VERSION.SdkInt >= AndroidOS.BuildVersionCodes.Q ? e.EventTimeNano : e.EventTime * 1000000;

                        // Process historical points for smoother/predicted input
                        for (int h = 0; h < e.HistorySize; h++)
                        {
                            float historicalX = e.GetHistoricalX(i, h);
                            float historicalY = e.GetHistoricalY(i, h);
                            long historicalTimeNano = AndroidOS.Build.VERSION.SdkInt >= AndroidOS.BuildVersionCodes.Q ? e.GetHistoricalEventTimeNano(h) : e.GetHistoricalEventTime(h) * 1000000;
                            game.HandleStylusInput(historicalX, historicalY, historicalTimeNano);
                        }
                    }
                }
            }

            return base.DispatchTouchEvent(e);
        }

        public new bool IsDeXMode()
        {
            var config = Resources?.Configuration;
            if (config == null) return false;

            return (config.UiMode & AndroidRes.UiMode.TypeMask) == AndroidRes.UiMode.TypeDesk;
        }

        public void ApplyPerformanceOptimizations(bool enabled)
        {
            RunOnUiThread(() =>
            {
                if (AndroidOS.Build.VERSION.SdkInt >= AndroidOS.BuildVersionCodes.N)
                    Window?.SetSustainedPerformanceMode(enabled);

                bool dexMode = IsDeXMode();

                if ((enabled || dexMode) && AndroidOS.Build.VERSION.SdkInt >= AndroidOS.BuildVersionCodes.M)
                {
#pragma warning disable CA1422
                    var preferredMode = WindowManager?.DefaultDisplay?.SupportedModes?.OrderByDescending(m => m.RefreshRate).FirstOrDefault();
                    if (preferredMode != null && Window != null)
                    {
                        var layoutParams = Window.Attributes;
                        layoutParams.PreferredDisplayModeId = preferredMode.ModeId;
                        Window.Attributes = layoutParams;
                    }
#pragma warning restore CA1422
                }
            });
        }

        public void ApplyAngleOptimizations(bool enabled)
        {
            // ANGLE (GLES to Vulkan) translation logic placeholder.
        }

        private static readonly string[] osu_url_schemes = { "osu", "osump" };

        /// <summary>
        /// The default screen orientation.
        /// </summary>
        /// <remarks>Adjusted on startup to match expected UX for the current device type (phone/tablet).</remarks>
        public AndroidPM.ScreenOrientation DefaultOrientation = AndroidPM.ScreenOrientation.Unspecified;

        public new bool IsTablet { get; private set; }

        private readonly OsuGameAndroid game;

        private bool gameCreated;

        protected override global::osu.Framework.Game CreateGame()
        {
            if (gameCreated)
                throw new InvalidOperationException("Framework tried to create a game twice.");

            gameCreated = true;
            return game;
        }

        public OsuGameActivity()
        {
            game = new OsuGameAndroid(this);
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (AndroidOS.Build.VERSION.SdkInt >= AndroidOS.BuildVersionCodes.R) Window?.DecorView?.RequestUnbufferedDispatch((int)AndroidViews.InputSourceType.Touchscreen);
        }

        protected override void OnCreate(AndroidOS.Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // OnNewIntent() only fires for an activity if it's *re-launched* while it's on top of the activity stack.
            // on first launch we still have to fire manually.
            // reference: https://developer.android.com/reference/android/app/Activity#onNewIntent(android.content.Intent)
            handleIntent(Intent);

            Debug.Assert(Window != null);

            Window.AddFlags(AndroidViews.WindowManagerFlags.Fullscreen);
            Window.AddFlags(AndroidViews.WindowManagerFlags.KeepScreenOn);

            Debug.Assert(WindowManager?.DefaultDisplay != null);
            Debug.Assert(Resources?.DisplayMetrics != null);

            AndroidGraphics.Point displaySize = new AndroidGraphics.Point();
#pragma warning disable CA1422 // GetSize is deprecated
            WindowManager.DefaultDisplay.GetSize(displaySize);
#pragma warning restore CA1422
            float smallestWidthDp = Math.Min(displaySize.X, displaySize.Y) / Resources.DisplayMetrics.Density;
            IsTablet = smallestWidthDp >= 600f;

            RequestedOrientation = DefaultOrientation = IsTablet ? AndroidPM.ScreenOrientation.FullUser : AndroidPM.ScreenOrientation.SensorLandscape;

            // Currently (SDK 6.0.200), BundleAssemblies is not runnable for net6-android.
            // The assembly files are not available as files either after native AOT.
            // Manually load them so that they can be loaded by RulesetStore.loadFromAppDomain.
            // REMEMBER to fully uninstall previous version every time when investigating this!
            // Don't forget osu.Game.Tests.Android too.
            Assembly.Load("osu.Game.Rulesets.Osu");
            Assembly.Load("osu.Game.Rulesets.Taiko");
            Assembly.Load("osu.Game.Rulesets.Catch");
            Assembly.Load("osu.Game.Rulesets.Mania");
        }

        protected override void OnResume()
        {
            base.OnResume();

            if (AndroidOS.Build.VERSION.SdkInt >= AndroidOS.BuildVersionCodes.S)
            {
                var gm = (AndroidApp.GameManager?)GetSystemService(GameService);
                if (gm != null)
                {
                    int mode = gm.GameMode;
                    ApplyPerformanceOptimizations(mode == (int)AndroidApp.GameMode.Performance);
                }
            }

            CheckInputDevices();
        }

        private void CheckInputDevices()
        {
            var inputManager = (AndroidInput.InputManager?)GetSystemService(InputService);
            int[] deviceIds = inputManager?.GetInputDeviceIds() ?? Array.Empty<int>();

            foreach (int id in deviceIds)
            {
                var device = inputManager?.GetInputDevice(id);
                if (device == null) continue;

                if ((device.Sources & AndroidViews.InputSourceType.Gamepad) == AndroidViews.InputSourceType.Gamepad)
                {
                    // Gamepad detected
                }
            }
        }

        public override void OnConfigurationChanged(AndroidRes.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);

            if (IsDeXMode())
            {
                ApplyPerformanceOptimizations(true);
            }
        }

        protected override void OnNewIntent(AndroidContent.Intent? intent) => handleIntent(intent);

        private void handleIntent(AndroidContent.Intent? intent)
        {
            if (intent == null)
                return;

            switch (intent.Action)
            {
                case AndroidContent.Intent.ActionDefault:
                    if (intent.Scheme == AndroidContent.ContentResolver.SchemeContent)
                    {
                        if (intent.Data != null)
                            handleImportFromUris(intent.Data);
                    }
                    else if (osu_url_schemes.Contains(intent.Scheme))
                    {
                        if (intent.DataString != null)
                            game.HandleLink(intent.DataString);
                    }

                    break;

                case AndroidContent.Intent.ActionSend:
                case AndroidContent.Intent.ActionSendMultiple:
                {
                    if (intent.ClipData == null)
                        break;

                    var uris = new List<Uri>();

                    for (int i = 0; i < intent.ClipData.ItemCount; i++)
                    {
                        var item = intent.ClipData.GetItemAt(i);
                        if (item?.Uri != null)
                            uris.Add(item.Uri);
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
                var task = await AndroidImportTask.Create(ContentResolver!, uri).ConfigureAwait(false);

                if (task != null)
                {
                    lock (tasks)
                    {
                        tasks.Add(task);
                    }
                }
            })).ConfigureAwait(false);

            await game.Import(tasks.ToArray()).ConfigureAwait(false);
        }, TaskCreationOptions.LongRunning);
    }
}

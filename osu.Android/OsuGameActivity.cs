// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using osu.Framework.Android;
using osu.Game.Database;
using Debug = System.Diagnostics.Debug;

namespace osu.Android
{
    [global::Android.App.Activity(ConfigurationChanges = global::Android.Content.PM.ConfigChanges.Orientation | global::Android.Content.PM.ConfigChanges.ScreenSize | global::Android.Content.PM.ConfigChanges.UiMode, Exported = true, LaunchMode = global::Android.Content.PM.LaunchMode.SingleInstance, MainLauncher = true)]
    [global::Android.App.IntentFilter(new[] { "android.intent.action.VIEW" }, Categories = new[] { "android.intent.category.DEFAULT" }, DataScheme = "content", DataPathPattern = ".*\\\\.osz", DataHost = "*", DataMimeType = "*/*")]
    [global::Android.App.IntentFilter(new[] { "android.intent.action.VIEW" }, Categories = new[] { "android.intent.category.DEFAULT" }, DataScheme = "content", DataPathPattern = ".*\\\\.osk", DataHost = "*", DataMimeType = "*/*")]
    [global::Android.App.IntentFilter(new[] { "android.intent.action.VIEW" }, Categories = new[] { "android.intent.category.DEFAULT" }, DataScheme = "content", DataPathPattern = ".*\\\\.osr", DataHost = "*", DataMimeType = "*/*")]
    [global::Android.App.IntentFilter(new[] { "android.intent.action.VIEW" }, Categories = new[] { "android.intent.category.DEFAULT" }, DataScheme = "content", DataMimeType = "application/x-osu-beatmap-archive")]
    [global::Android.App.IntentFilter(new[] { "android.intent.action.VIEW" }, Categories = new[] { "android.intent.category.DEFAULT" }, DataScheme = "content", DataMimeType = "application/x-osu-skin-archive")]
    [global::Android.App.IntentFilter(new[] { "android.intent.action.VIEW" }, Categories = new[] { "android.intent.category.DEFAULT" }, DataScheme = "content", DataMimeType = "application/x-osu-replay")]
    [global::Android.App.IntentFilter(new[] { "android.intent.action.SEND", "android.intent.action.SEND_MULTIPLE" }, Categories = new[] { "android.intent.category.DEFAULT" }, DataMimeTypes = new[]
    {
        "application/zip",
        "application/octet-stream",
        "application/download",
        "application/x-zip",
        "application/x-zip-compressed",
        "application/x-osu-beatmap-archive",
        "application/x-osu-skin-archive",
        "application/x-osu-replay",
    })]
    [global::Android.App.IntentFilter(new[] { "android.intent.action.VIEW" }, Categories = new[] { "android.intent.category.BROWSABLE", "android.intent.category.DEFAULT" }, DataSchemes = new[] { "osu", "osump" })]
    public class OsuGameActivity : AndroidGameActivity
    {
        public override bool DispatchTouchEvent(global::Android.Views.MotionEvent? e)
        {
            if (e != null)
            {
                for (int i = 0; i < e.PointerCount; i++)
                {
                    var toolType = e.GetToolType(i);
                    if (toolType == global::Android.Views.MotionEventToolType.Stylus)
                    {
                        // S Pen detected. Hardware timestamps should be used for improved latency.
                        // Using EventTime * 1000000 for maximum SDK compatibility as EventTimeNano is sometimes unavailable at compile-time.
                        long timestampNano = e.EventTime * 1000000;

                        // Process historical points for smoother/predicted input
                        for (int h = 0; h < e.HistorySize; h++)
                        {
                            float historicalX = e.GetHistoricalX(i, h);
                            float historicalY = e.GetHistoricalY(i, h);
                            long historicalTimeNano = e.GetHistoricalEventTime(h) * 1000000;
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

            return (config.UiMode & global::Android.Content.Res.UiMode.TypeMask) == global::Android.Content.Res.UiMode.TypeDesk;
        }

        public void ApplyPerformanceOptimizations(bool enabled)
        {
            RunOnUiThread(() =>
            {
                var window = Window;
                if (window != null && global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.N)
                    window.SetSustainedPerformanceMode(enabled);

                bool dexMode = IsDeXMode();
                var display = WindowManager?.DefaultDisplay;

                if ((enabled || dexMode) && global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.M && display != null)
                {
#pragma warning disable CA1422
                    var modes = display.GetSupportedModes();
                    var preferredMode = modes?.OrderByDescending(m => m.RefreshRate).FirstOrDefault();

                    if (preferredMode != null && window != null)
                    {
                        var layoutParams = window.Attributes;

                        if (layoutParams != null)
                        {
                            layoutParams.PreferredDisplayModeId = preferredMode.ModeId;
                            window.Attributes = layoutParams;
                        }
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
        public global::Android.Content.PM.ScreenOrientation DefaultOrientation = global::Android.Content.PM.ScreenOrientation.Unspecified;

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
            if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.R) Window?.DecorView?.RequestUnbufferedDispatch((int)global::Android.Views.InputSourceType.Touchscreen);
        }

        protected override void OnCreate(global::Android.OS.Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // OnNewIntent() only fires for an activity if it's *re-launched* while it's on top of the activity stack.
            // on first launch we still have to fire manually.
            // reference: https://developer.android.com/reference/android/app/Activity#onNewIntent(android.content.Intent)
            handleIntent(Intent);

            Debug.Assert(Window != null);

            Window.AddFlags(global::Android.Views.WindowManagerFlags.Fullscreen);
            Window.AddFlags(global::Android.Views.WindowManagerFlags.KeepScreenOn);

            Debug.Assert(WindowManager?.DefaultDisplay != null);
            Debug.Assert(Resources?.DisplayMetrics != null);

            global::Android.Graphics.Point displaySize = new global::Android.Graphics.Point();
#pragma warning disable CA1422 // GetSize is deprecated
            WindowManager.DefaultDisplay.GetSize(displaySize);
#pragma warning restore CA1422
            float smallestWidthDp = Math.Min(displaySize.X, displaySize.Y) / Resources.DisplayMetrics.Density;
            IsTablet = smallestWidthDp >= 600f;

            RequestedOrientation = DefaultOrientation = IsTablet ? global::Android.Content.PM.ScreenOrientation.FullUser : global::Android.Content.PM.ScreenOrientation.SensorLandscape;

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

            if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.S)
            {
                var gm = (global::Android.App.GameManager?)GetSystemService(GameService);
                if (gm != null)
                {
                    int mode = (int)gm.GameMode;
                    ApplyPerformanceOptimizations(mode == (int)global::Android.App.GameMode.Performance);
                }
            }

            CheckInputDevices();
        }

        private void CheckInputDevices()
        {
            var inputManager = (global::Android.Hardware.Input.InputManager?)GetSystemService(InputService);
            int[] deviceIds = inputManager?.GetInputDeviceIds() ?? Array.Empty<int>();

            foreach (int id in deviceIds)
            {
                var device = inputManager?.GetInputDevice(id);
                if (device == null) continue;

                if ((device.Sources & global::Android.Views.InputSourceType.Gamepad) == global::Android.Views.InputSourceType.Gamepad)
                {
                    // Gamepad detected
                }
            }
        }

        public override void OnConfigurationChanged(global::Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);

            if (IsDeXMode())
            {
                ApplyPerformanceOptimizations(true);
            }
        }

        protected override void OnNewIntent(global::Android.Content.Intent? intent) => handleIntent(intent);

        private void handleIntent(global::Android.Content.Intent? intent)
        {
            if (intent == null)
                return;

            switch (intent.Action)
            {
                case global::Android.Content.Intent.ActionMain:
                case global::Android.Content.Intent.ActionView:
                    if (intent.Scheme == global::Android.Content.ContentResolver.SchemeContent)
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

                case global::Android.Content.Intent.ActionSend:
                case global::Android.Content.Intent.ActionSendMultiple:
                {
                    if (intent.ClipData == null)
                        break;

                    var uris = new List<global::Android.Net.Uri>();

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

        private void handleImportFromUris(params global::Android.Net.Uri[] uris) => Task.Factory.StartNew(async () =>
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

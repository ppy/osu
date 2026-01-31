// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.Hardware.Input;
using Android.OS;
using Android.Views;
using osu.Framework.Android;
using osu.Game.Database;
using Debug = System.Diagnostics.Debug;
using Uri = Android.Net.Uri;

namespace osu.Android
{
    [Activity(ConfigurationChanges = DEFAULT_CONFIG_CHANGES, Exported = true, LaunchMode = DEFAULT_LAUNCH_MODE, MainLauncher = true)]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault }, DataScheme = "content", DataPathPattern = ".*\\\\.osz", DataHost = "*", DataMimeType = "*/*")]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault }, DataScheme = "content", DataPathPattern = ".*\\\\.osk", DataHost = "*", DataMimeType = "*/*")]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault }, DataScheme = "content", DataPathPattern = ".*\\\\.osr", DataHost = "*", DataMimeType = "*/*")]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault }, DataScheme = "content", DataMimeType = "application/x-osu-beatmap-archive")]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault }, DataScheme = "content", DataMimeType = "application/x-osu-skin-archive")]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault }, DataScheme = "content", DataMimeType = "application/x-osu-replay")]
    [IntentFilter(new[] { Intent.ActionSend, Intent.ActionSendMultiple }, Categories = new[] { Intent.CategoryDefault }, DataMimeTypes = new[]
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
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault }, DataSchemes = new[] { "osu", "osump" })]
    public class OsuGameActivity : AndroidGameActivity
    {
        public override bool DispatchTouchEvent(MotionEvent? e)
        {
            if (e != null)
            {
                for (int i = 0; i < e.PointerCount; i++)
                {
                    var toolType = e.GetToolType(i);
                    if (toolType == MotionEventToolType.Stylus)
                    {
                        // S Pen detected. Hardware timestamps should be used for improved latency.
                        // MotionEvent.EventTime is in ms, converting to nano
                        long timestampNano = e.EventTime * 1000000;

                        // Process historical points for smoother/predicted input
                        for (int h = 0; h < e.HistorySize; h++)
                        {
                            float historicalX = e.GetHistoricalX(i, h);
                            float historicalY = e.GetHistoricalY(i, h);
                            long historicalTimeNano = e.GetHistoricalEventTime(h) * 1000000;
                            // Feed timestamp + S Pen position into audio-aligned timeline
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

            return config.UiMode.HasFlag(UiMode.TypeDesk);
        }

        public void ApplyPerformanceOptimizations(bool enabled)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                Window?.SetSustainedPerformanceMode(enabled);

            bool dexMode = IsDeXMode();

            if ((enabled || dexMode) && Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
#pragma warning disable CA1422
                var preferredMode = WindowManager?.DefaultDisplay?.GetModes()?.OrderByDescending(m => m.RefreshRate).FirstOrDefault();
                if (preferredMode != null && Window != null)
                {
                    var layoutParams = Window.Attributes;
                    layoutParams.PreferredDisplayModeId = preferredMode.ModeId;
                    Window.Attributes = layoutParams;
                }
#pragma warning restore CA1422
            }
        }

        private static readonly string[] osu_url_schemes = { "osu", "osump" };

        /// <summary>
        /// The default screen orientation.
        /// </summary>
        /// <remarks>Adjusted on startup to match expected UX for the current device type (phone/tablet).</remarks>
        public ScreenOrientation DefaultOrientation = ScreenOrientation.Unspecified;

        public new bool IsTablet { get; private set; }

        private readonly OsuGameAndroid game;

        private bool gameCreated;

        protected override Framework.Game CreateGame()
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

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // OnNewIntent() only fires for an activity if it's *re-launched* while it's on top of the activity stack.
            // on first launch we still have to fire manually.
            // reference: https://developer.android.com/reference/android/app/Activity#onNewIntent(android.content.Intent)
            handleIntent(Intent);

            Debug.Assert(Window != null);

            Window.AddFlags(WindowManagerFlags.Fullscreen);
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);

            Debug.Assert(WindowManager?.DefaultDisplay != null);
            Debug.Assert(Resources?.DisplayMetrics != null);

            Point displaySize = new Point();
#pragma warning disable CA1422 // GetSize is deprecated
            WindowManager.DefaultDisplay.GetSize(displaySize);
#pragma warning restore CA1422
            float smallestWidthDp = Math.Min(displaySize.X, displaySize.Y) / Resources.DisplayMetrics.Density;
            IsTablet = smallestWidthDp >= 600f;

            RequestedOrientation = DefaultOrientation = IsTablet ? ScreenOrientation.FullUser : ScreenOrientation.SensorLandscape;

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

            if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
            {
                var gm = (GameManager?)GetSystemService(GameService);
                if (gm != null)
                {
                    int mode = gm.GameMode;
                    // PERFORMANCE -> push high perf path
                    // BATTERY/SAVE -> adapt lower fidelity
                    ApplyPerformanceOptimizations(mode == GameManager.GameModePerformance);
                }
            }

            CheckInputDevices();
        }

        private void CheckInputDevices()
        {
            var inputManager = (InputManager?)GetSystemService(InputService);
            int[] deviceIds = inputManager?.GetInputDeviceIds() ?? Array.Empty<int>();

            foreach (int id in deviceIds)
            {
                var device = inputManager?.GetInputDevice(id);
                if (device == null) continue;

                if ((device.Sources & InputSourceType.Gamepad) == InputSourceType.Gamepad)
                {
                    // Gamepad detected
                }
            }
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);

            if (IsDeXMode())
            {
                // Adapt swapchain/extents dynamically
                ApplyPerformanceOptimizations(true);
            }
        }

        protected override void OnNewIntent(Intent? intent) => handleIntent(intent);

        private void handleIntent(Intent? intent)
        {
            if (intent == null)
                return;

            switch (intent.Action)
            {
                case Intent.ActionDefault:
                    if (intent.Scheme == ContentResolver.SchemeContent)
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

                case Intent.ActionSend:
                case Intent.ActionSendMultiple:
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

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Platform;
using osu.Framework.Threading;
using osu.Game.Configuration;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace osu.Game.Graphics
{
    public class ScreenshotManager : Component, IKeyBindingHandler<GlobalAction>, IHandleGlobalKeyboardInput
    {
        private readonly BindableBool cursorVisibility = new BindableBool(true);

        /// <summary>
        /// Changed when screenshots are being or have finished being taken, to control whether cursors should be visible.
        /// If cursors should not be visible, cursors have 3 frames to hide themselves.
        /// </summary>
        public IBindable<bool> CursorVisibility => cursorVisibility;

        private Bindable<ScreenshotFormat> screenshotFormat;
        private Bindable<bool> captureMenuCursor;

        [Resolved]
        private GameHost host { get; set; }

        private Storage storage;

        [Resolved]
        private NotificationOverlay notificationOverlay { get; set; }

        private SampleChannel shutter;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, Storage storage, AudioManager audio)
        {
            this.storage = storage.GetStorageForDirectory(@"screenshots");

            screenshotFormat = config.GetBindable<ScreenshotFormat>(OsuSetting.ScreenshotFormat);
            captureMenuCursor = config.GetBindable<bool>(OsuSetting.ScreenshotCaptureMenuCursor);

            shutter = audio.Samples.Get("UI/shutter");
        }

        public bool OnPressed(GlobalAction action)
        {
            switch (action)
            {
                case GlobalAction.TakeScreenshot:
                    shutter.Play();
                    TakeScreenshotAsync();
                    return true;
            }

            return false;
        }

        public void OnReleased(GlobalAction action)
        {
        }

        private volatile int screenShotTasks;

        public Task TakeScreenshotAsync() => Task.Run(async () =>
        {
            Interlocked.Increment(ref screenShotTasks);

            if (!captureMenuCursor.Value)
            {
                cursorVisibility.Value = false;

                // We need to wait for at most 3 draw nodes to be drawn, following which we can be assured at least one DrawNode has been generated/drawn with the set value
                const int frames_to_wait = 3;

                int framesWaited = 0;

                using (var framesWaitedEvent = new ManualResetEventSlim(false))
                {
                    ScheduledDelegate waitDelegate = host.DrawThread.Scheduler.AddDelayed(() =>
                    {
                        if (framesWaited++ >= frames_to_wait)
                            // ReSharper disable once AccessToDisposedClosure
                            framesWaitedEvent.Set();
                    }, 10, true);

                    framesWaitedEvent.Wait();
                    waitDelegate.Cancel();
                }
            }

            using (var image = await host.TakeScreenshotAsync())
            {
                if (Interlocked.Decrement(ref screenShotTasks) == 0 && cursorVisibility.Value == false)
                    cursorVisibility.Value = true;

                var fileName = getFileName();
                if (fileName == null) return;

                var stream = storage.GetStream(fileName, FileAccess.Write);

                switch (screenshotFormat.Value)
                {
                    case ScreenshotFormat.Png:
                        await image.SaveAsPngAsync(stream);
                        break;

                    case ScreenshotFormat.Jpg:
                        const int jpeg_quality = 92;

                        await image.SaveAsJpegAsync(stream, new JpegEncoder { Quality = jpeg_quality });
                        break;

                    default:
                        throw new InvalidOperationException($"Unknown enum member {nameof(ScreenshotFormat)} {screenshotFormat.Value}.");
                }

                notificationOverlay.Post(new SimpleNotification
                {
                    Text = $"{fileName} saved!",
                    Activated = () =>
                    {
                        storage.OpenInNativeExplorer();
                        return true;
                    }
                });
            }
        });

        private string getFileName()
        {
            var dt = DateTime.Now;
            var fileExt = screenshotFormat.ToString().ToLowerInvariant();

            var withoutIndex = $"osu_{dt:yyyy-MM-dd_HH-mm-ss}.{fileExt}";
            if (!storage.Exists(withoutIndex))
                return withoutIndex;

            for (ulong i = 1; i < ulong.MaxValue; i++)
            {
                var indexedName = $"osu_{dt:yyyy-MM-dd_HH-mm-ss}-{i}.{fileExt}";
                if (!storage.Exists(indexedName))
                    return indexedName;
            }

            return null;
        }
    }
}

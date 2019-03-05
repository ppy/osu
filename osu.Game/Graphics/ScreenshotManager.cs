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
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Platform;
using osu.Framework.Threading;
using osu.Game.Configuration;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using SixLabors.ImageSharp;

namespace osu.Game.Graphics
{
    public class ScreenshotManager : Container, IKeyBindingHandler<GlobalAction>, IHandleGlobalInput
    {
        private readonly BindableBool cursorVisibility = new BindableBool(true);

        /// <summary>
        /// Changed when screenshots are being or have finished being taken, to control whether cursors should be visible.
        /// If cursors should not be visible, cursors have 3 frames to hide themselves.
        /// </summary>
        public IBindable<bool> CursorVisibility => cursorVisibility;

        private Bindable<ScreenshotFormat> screenshotFormat;
        private Bindable<bool> captureMenuCursor;

        private GameHost host;
        private Storage storage;
        private NotificationOverlay notificationOverlay;

        private SampleChannel shutter;

        [BackgroundDependencyLoader]
        private void load(GameHost host, OsuConfigManager config, Storage storage, NotificationOverlay notificationOverlay, AudioManager audio)
        {
            this.host = host;
            this.storage = storage.GetStorageForDirectory(@"screenshots");
            this.notificationOverlay = notificationOverlay;

            screenshotFormat = config.GetBindable<ScreenshotFormat>(OsuSetting.ScreenshotFormat);
            captureMenuCursor = config.GetBindable<bool>(OsuSetting.ScreenshotCaptureMenuCursor);

            shutter = audio.Sample.Get("UI/shutter");
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

        public bool OnReleased(GlobalAction action) => false;

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
                ScheduledDelegate waitDelegate = host.DrawThread.Scheduler.AddDelayed(() => framesWaited++, 0, true);
                while (framesWaited < frames_to_wait)
                    Thread.Sleep(10);

                waitDelegate.Cancel();
            }

            using (var image = await host.TakeScreenshotAsync())
            {
                Interlocked.Decrement(ref screenShotTasks);

                var fileName = getFileName();
                if (fileName == null) return;

                var stream = storage.GetStream(fileName, FileAccess.Write);

                switch (screenshotFormat.Value)
                {
                    case ScreenshotFormat.Png:
                        image.SaveAsPng(stream);
                        break;
                    case ScreenshotFormat.Jpg:
                        image.SaveAsJpeg(stream);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(screenshotFormat));
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

        protected override void Update()
        {
            base.Update();

            if (cursorVisibility.Value == false && Interlocked.CompareExchange(ref screenShotTasks, 0, 0) == 0)
                cursorVisibility.Value = true;
        }

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

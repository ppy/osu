// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osu.Framework.Threading;
using osu.Game.Configuration;
using osu.Game.Input.Bindings;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace osu.Game.Graphics
{
    public partial class ScreenshotManager : Component, IKeyBindingHandler<GlobalAction>, IHandleGlobalKeyboardInput
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

        [Resolved]
        private Clipboard clipboard { get; set; }

        private Storage storage;

        [Resolved]
        private INotificationOverlay notificationOverlay { get; set; }

        private Sample shutter;
        private Bindable<float> sizeX;
        private Bindable<float> sizeY;
        private Bindable<ScalingMode> scalingMode;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, Storage storage, AudioManager audio)
        {
            this.storage = storage.GetStorageForDirectory(@"screenshots");

            screenshotFormat = config.GetBindable<ScreenshotFormat>(OsuSetting.ScreenshotFormat);
            captureMenuCursor = config.GetBindable<bool>(OsuSetting.ScreenshotCaptureMenuCursor);

            shutter = audio.Samples.Get("UI/shutter");

            sizeX = config.GetBindable<float>(OsuSetting.ScalingSizeX);
            sizeY = config.GetBindable<float>(OsuSetting.ScalingSizeY);
            scalingMode = config.GetBindable<ScalingMode>(OsuSetting.Scaling);
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            switch (e.Action)
            {
                case GlobalAction.TakeScreenshot:
                    shutter.Play();
                    TakeScreenshotAsync().FireAndForget();
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        private volatile int screenShotTasks;

        public Task TakeScreenshotAsync() => Task.Run(async () =>
        {
            Interlocked.Increment(ref screenShotTasks);

            try
            {
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

                        if (!framesWaitedEvent.Wait(1000))
                            throw new TimeoutException("Screenshot data did not arrive in a timely fashion");

                        waitDelegate.Cancel();
                    }
                }

                using (var image = await host.TakeScreenshotAsync().ConfigureAwait(false))
                {
                    if (scalingMode.Value == ScalingMode.Everything)
                    {
                        image.Mutate(m =>
                        {
                            var size = m.GetCurrentSize();
                            var rect = new Rectangle(Point.Empty, size);
                            int sx = (size.Width - (int)(size.Width * sizeX.Value)) / 2;
                            int sy = (size.Height - (int)(size.Height * sizeY.Value)) / 2;
                            rect.Inflate(-sx, -sy);
                            m.Crop(rect);
                        });
                    }

                    clipboard.SetImage(image);

                    (string filename, var stream) = getWritableStream();

                    if (filename == null) return;

                    using (stream)
                    {
                        switch (screenshotFormat.Value)
                        {
                            case ScreenshotFormat.Png:
                                await image.SaveAsPngAsync(stream).ConfigureAwait(false);
                                break;

                            case ScreenshotFormat.Jpg:
                                const int jpeg_quality = 92;

                                await image.SaveAsJpegAsync(stream, new JpegEncoder { Quality = jpeg_quality }).ConfigureAwait(false);
                                break;

                            default:
                                throw new InvalidOperationException($"Unknown enum member {nameof(ScreenshotFormat)} {screenshotFormat.Value}.");
                        }
                    }

                    notificationOverlay.Post(new SimpleNotification
                    {
                        Text = $"Screenshot {filename} saved!",
                        Activated = () =>
                        {
                            storage.PresentFileExternally(filename);
                            return true;
                        }
                    });
                }
            }
            finally
            {
                if (Interlocked.Decrement(ref screenShotTasks) == 0)
                    cursorVisibility.Value = true;
            }
        });

        private static readonly object filename_reservation_lock = new object();

        private (string filename, Stream stream) getWritableStream()
        {
            lock (filename_reservation_lock)
            {
                var dt = DateTime.Now;
                string fileExt = screenshotFormat.ToString().ToLowerInvariant();

                string withoutIndex = $"osu_{dt:yyyy-MM-dd_HH-mm-ss}.{fileExt}";
                if (!storage.Exists(withoutIndex))
                    return (withoutIndex, storage.GetStream(withoutIndex, FileAccess.Write, FileMode.Create));

                for (ulong i = 1; i < ulong.MaxValue; i++)
                {
                    string indexedName = $"osu_{dt:yyyy-MM-dd_HH-mm-ss}-{i}.{fileExt}";
                    if (!storage.Exists(indexedName))
                        return (indexedName, storage.GetStream(indexedName, FileAccess.Write, FileMode.Create));
                }

                return (null, null);
            }
        }
    }
}

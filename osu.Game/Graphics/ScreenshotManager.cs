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
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osu.Framework.Threading;
using osu.Game.Configuration;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace osu.Game.Graphics
{
    public partial class ScreenshotManager : Component, IKeyBindingHandler<GlobalAction>, IHandleGlobalKeyboardInput
    {
        private const int jpeg_quality = 92;

        private readonly BindableBool cursorVisibility = new BindableBool(true);

        /// <summary>
        /// Changed when screenshots are being or have finished being taken, to control whether cursors should be visible.
        /// If cursors should not be visible, cursors have 3 frames to hide themselves.
        /// </summary>
        public IBindable<bool> CursorVisibility => cursorVisibility;

        [Resolved]
        private GameHost host { get; set; } = null!;

        [Resolved]
        private Clipboard clipboard { get; set; } = null!;

        [Resolved]
        private INotificationOverlay notificationOverlay { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        private Storage storage = null!;

        private Sample? shutter;

        [BackgroundDependencyLoader]
        private void load(Storage storage, AudioManager audio)
        {
            this.storage = storage.GetStorageForDirectory(@"screenshots");
            shutter = audio.Samples.Get(@"UI/shutter");
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            switch (e.Action)
            {
                case GlobalAction.TakeScreenshot:
                    shutter?.Play();
                    TakeScreenshotAsync().FireAndForget();
                    return true;

                case GlobalAction.TakeAndUploadScreeshot:
                    shutter?.Play();
                    UploadScreenshotAsync().FireAndForget();
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        private volatile int screenShotTasks;

        public Task<string?> TakeScreenshotAsync(bool forUpload = false) => Task.Run<string?>(async () =>
        {
            Interlocked.Increment(ref screenShotTasks);

            ScreenshotFormat screenshotFormat = config.Get<ScreenshotFormat>(OsuSetting.ScreenshotFormat);
            bool captureMenuCursor = config.Get<bool>(OsuSetting.ScreenshotCaptureMenuCursor);

            try
            {
                if (!captureMenuCursor)
                {
                    cursorVisibility.Value = false;

                    // We need to wait for at most 3 draw nodes to be drawn, following which we can be assured at least one DrawNode has been generated/drawn with the set value
                    const int frames_to_wait = 3;

                    int framesWaited = 0;

                    using (ManualResetEventSlim framesWaitedEvent = new ManualResetEventSlim(false))
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

                using (Image<Rgba32>? image = await host.TakeScreenshotAsync().ConfigureAwait(false))
                {
                    if (config.Get<ScalingMode>(OsuSetting.Scaling) == ScalingMode.Everything)
                    {
                        float posX = config.Get<float>(OsuSetting.ScalingPositionX);
                        float posY = config.Get<float>(OsuSetting.ScalingPositionY);
                        float sizeX = config.Get<float>(OsuSetting.ScalingSizeX);
                        float sizeY = config.Get<float>(OsuSetting.ScalingSizeY);

                        image.Mutate(m =>
                        {
                            Rectangle rect = new Rectangle(Point.Empty, m.GetCurrentSize());

                            // Reduce size by user scale settings...
                            int sx = (rect.Width - (int)(rect.Width * sizeX)) / 2;
                            int sy = (rect.Height - (int)(rect.Height * sizeY)) / 2;
                            rect.Inflate(-sx, -sy);

                            // ...then adjust the region based on their positional offset.
                            rect.X = (int)(rect.X * posX) * 2;
                            rect.Y = (int)(rect.Y * posY) * 2;

                            m.Crop(rect);
                        });
                    }

                    // Don't copy the image to clipboard when uploading a screenshot,
                    // as it's going to be overwritten by the URL anyway.
                    if (!forUpload)
                    {
                        clipboard.SetImage(image);
                    }

                    (string? filename, Stream? stream) = getWritableStream(screenshotFormat);

                    if (filename == null) return null;

                    using (stream)
                    {
                        switch (screenshotFormat)
                        {
                            case ScreenshotFormat.Png:
                                await image.SaveAsPngAsync(stream).ConfigureAwait(false);
                                break;

                            case ScreenshotFormat.Jpg:
                                await image.SaveAsJpegAsync(stream, new JpegEncoder { Quality = jpeg_quality }).ConfigureAwait(false);
                                break;

                            default:
                                throw new InvalidOperationException($"Unknown enum member {nameof(ScreenshotFormat)} {screenshotFormat}.");
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

                    return filename;
                }
            }
            finally
            {
                if (Interlocked.Decrement(ref screenShotTasks) == 0)
                    cursorVisibility.Value = true;
            }
        });

        public Task UploadScreenshotAsync() => Task.Run(async () =>
        {
            string? filename = await TakeScreenshotAsync(true).ConfigureAwait(false);

            if (filename == null)
            {
                return;
            }

            Stream stream;

            // Convert the taken screenshot to JPEG (to save storage) before uploading
            // if user set their screenshots to save in a different format.
            if (config.Get<ScreenshotFormat>(OsuSetting.ScreenshotFormat) != ScreenshotFormat.Jpg)
            {
                var image = await Image.LoadAsync(storage.GetFullPath(filename)).ConfigureAwait(false);

                stream = new MemoryStream();
                await image.SaveAsJpegAsync(stream, new JpegEncoder { Quality = jpeg_quality }).ConfigureAwait(false);
            }
            else
            {
                stream = storage.GetStream(filename, FileAccess.Read, FileMode.Open);
            }

            var uploadRequest = new UploadScreenshot(await stream.ReadAllBytesToArrayAsync().ConfigureAwait(false));

            var notification = new ProgressNotification
            {
                State = ProgressNotificationState.Active,
                Text = ScreenshotManagerStrings.UploadingScreenshot,
                CompletionText = ScreenshotManagerStrings.UploadSuccess,
                Progress = 0,
            };

            uploadRequest.Progressed += (current, total) => notification.Progress = (float)current / total;
            uploadRequest.Success += content =>
            {
                clipboard.SetText(content.Url);

                notification.Progress = 1;
                notification.State = ProgressNotificationState.Completed;
            };
            uploadRequest.Failure += _ =>
            {
                notification.State = ProgressNotificationState.Cancelled;
                notification.Text = ScreenshotManagerStrings.UploadFailure;
            };

            notificationOverlay.Post(notification);
            api.Queue(uploadRequest);
        });

        private static readonly object filename_reservation_lock = new object();

        private (string? filename, Stream? stream) getWritableStream(ScreenshotFormat format)
        {
            lock (filename_reservation_lock)
            {
                DateTime dt = DateTime.Now;
                string fileExt = format.ToString().ToLowerInvariant();

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

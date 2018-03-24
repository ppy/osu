// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Drawing.Imaging;
using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Graphics
{
    public class ScreenshotManager : Container, IKeyBindingHandler<GlobalAction>, IHandleGlobalInput
    {
        private Bindable<ScreenshotFormat> screenshotFormat;
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

        public async void TakeScreenshotAsync()
        {
            using (var bitmap = await host.TakeScreenshotAsync())
            {
                var fileName = getFileName();
                if (fileName == null) return;

                var stream = storage.GetStream(fileName, FileAccess.Write);

                switch (screenshotFormat.Value)
                {
                    case ScreenshotFormat.Png:
                        bitmap.Save(stream, ImageFormat.Png);
                        break;
                    case ScreenshotFormat.Jpg:
                        bitmap.Save(stream, ImageFormat.Jpeg);
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
        }

        private string getFileName()
        {
            var dt = DateTime.Now;
            var fileExt = screenshotFormat.ToString().ToLower();

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

using System;
using System.Drawing.Imaging;
using System.IO;
using osu.Framework.Allocation;
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

        [BackgroundDependencyLoader]
        private void load(GameHost host, OsuConfigManager config, Storage storage, NotificationOverlay notificationOverlay)
        {
            this.host = host;
            this.storage = storage.GetStorageForDirectory(@"screenshots");
            this.notificationOverlay = notificationOverlay;

            screenshotFormat = config.GetBindable<ScreenshotFormat>(OsuSetting.ScreenshotFormat);
        }

        public bool OnPressed(GlobalAction action)
        {
            switch (action)
            {
                case GlobalAction.TakeScreenshot:
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

                notificationOverlay.Post(new SimpleNotification { Text = $"{fileName} saved" });
            }
        }

        private string getFileName()
        {
            var fileExt = screenshotFormat.ToString().ToLower();

            var withoutIndex = $"Screenshot.{fileExt}";
            if (!storage.Exists(withoutIndex))
                return withoutIndex;

            for (ulong i = 1; i < ulong.MaxValue; i++)
            {
                var indexedName = $"Screenshot-{i}.{fileExt}";
                if (!storage.Exists(indexedName))
                    return indexedName;
            }

            throw new Exception($"Failed to find suitable file name for saving {fileExt} image");
        }
    }
}

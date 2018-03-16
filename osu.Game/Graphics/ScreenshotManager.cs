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

namespace osu.Game.Graphics
{
    public class ScreenshotManager : Container, IKeyBindingHandler<GlobalAction>, IHandleGlobalInput
    {
        private Bindable<ScreenshotFormat> screenshotFormat;
        private GameHost host;
        private Storage storage;

        [BackgroundDependencyLoader]
        private void load(GameHost host, OsuConfigManager config, Storage storage)
        {
            this.host = host;
            this.storage = storage.GetStorageForDirectory(@"screenshots");

            screenshotFormat = config.GetBindable<ScreenshotFormat>(OsuSetting.ScreenshotFormat);
        }

        public bool OnPressed(GlobalAction action)
        {
            switch (action)
            {
                case GlobalAction.TakeScreenshot:
                    TakeScreenshot();
                    return true;
            }

            return false;
        }

        public bool OnReleased(GlobalAction action) => false;

        public void TakeScreenshot()
        {
            host.TakeScreenshot(screenshotBitmap =>
            {
                var stream = getFileStream();

                switch (screenshotFormat.Value)
                {
                    case ScreenshotFormat.Png:
                        screenshotBitmap.Save(stream, ImageFormat.Png);
                        break;
                    case ScreenshotFormat.Jpg:
                        screenshotBitmap.Save(stream, ImageFormat.Jpeg);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(screenshotFormat));
                }
            });
        }

        private Stream getFileStream()
        {
            var fileExt = screenshotFormat.ToString().ToLower();

            var withoutIndex = $"Screenshot.{fileExt}";
            if (!storage.Exists(withoutIndex))
                return storage.GetStream(withoutIndex, FileAccess.Write);

            for (ulong i = 1; i < ulong.MaxValue; i++)
            {
                var indexedName = $"Screenshot-{i}.{fileExt}";
                if (!storage.Exists(indexedName))
                    return storage.GetStream(indexedName, FileAccess.Write);
            }

            throw new Exception($"Failed to get stream for saving {fileExt} file");
        }
    }
}

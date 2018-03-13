using System;
using System.Drawing.Imaging;
using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Game.Configuration;

namespace osu.Game.Graphics
{
    public class ScreenshotManager : Drawable
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

        public void TakeScreenshot()
        {
            host.TakeScreenshot(screenshotBitmap =>
            {
                var stream = storage.GetStream($"{DateTime.Now:yyyyMMddTHHmmss}.{screenshotFormat.ToString().ToLower()}", FileAccess.Write);

                switch (screenshotFormat.Value)
                {
                    case ScreenshotFormat.Bmp:
                        screenshotBitmap.Save(stream, ImageFormat.Bmp);
                        break;
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
    }
}

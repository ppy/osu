// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Settings.Sections.BeatmapDownloader;

namespace osu.Game.Overlays.Settings.Sections
{
    public class BeatmapDownloaderSection : SettingsSection
    {
        public override string Header => "Beatmap Downloader";

        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = FontAwesome.Solid.ArrowDown
        };

        public BeatmapDownloaderSection()
        {
            Children = new Drawable[]
            {
                new BeatmapDownloaderSettings(),
                new BeatmapDownloaderButtons(),
            };
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.BeatmapSet
{
    public class BeatmapSetHeader : OverlayHeader
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colors)
        {
            TitleBackgroundColour = colors.Gray2;
        }

        protected override ScreenTitle CreateTitle() => new BeatmapSetTitle();

        private class BeatmapSetTitle : ScreenTitle
        {
            public BeatmapSetTitle()
            {
                Title = "beatmap";
                Section = "info";
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                AccentColour = colours.Blue;
            }

            protected override Drawable CreateIcon() => new ScreenTitleTextureIcon(@"Icons/changelog");
        }
    }
}

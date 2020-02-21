// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.BeatmapListing
{
    public class BeatmapListingHeader : OverlayHeader
    {
        protected override ScreenTitle CreateTitle() => new BeatmapListingTitle();

        private class BeatmapListingTitle : ScreenTitle
        {
            public BeatmapListingTitle()
            {
                Title = @"beatmap";
                Section = @"listing";
            }

            protected override Drawable CreateIcon() => new ScreenTitleTextureIcon(@"Icons/changelog");
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Overlays.Wiki;

namespace osu.Game.Overlays
{
    public class WikiOverlay : OnlineOverlay<WikiHeader>
    {
        public WikiOverlay()
            : base(OverlayColourScheme.Orange, false)
        {
        }

        protected override WikiHeader CreateHeader() => new WikiHeader();
    }
}

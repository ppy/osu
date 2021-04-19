// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Overlays.Wiki
{
    public class WikiHeader : BreadcrumbControlOverlayHeader
    {
        private const string index_page_string = "index";

        public WikiHeader()
        {
            TabControl.AddItem(index_page_string);
        }

        protected override Drawable CreateBackground() => new OverlayHeaderBackground(@"Headers/wiki");

        protected override OverlayTitle CreateTitle() => new WikiHeaderTitle();

        private class WikiHeaderTitle : OverlayTitle
        {
            public WikiHeaderTitle()
            {
                Title = "wiki";
                Description = "knowledge base";
                IconTexture = "Icons/Hexacons/wiki";
            }
        }
    }
}

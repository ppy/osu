// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Wiki
{
    public class WikiHeader : BreadcrumbControlOverlayHeader
    {
        private const string index_page_string = "index";
        private const string index_path = "Main_Page";

        public Bindable<APIWikiPage> WikiPageData = new Bindable<APIWikiPage>();

        public WikiHeader()
        {
            TabControl.AddItem(index_page_string);
            Current.Value = index_page_string;

            WikiPageData.BindValueChanged(onWikiPageChange);
        }

        private void onWikiPageChange(ValueChangedEvent<APIWikiPage> e)
        {
            if (e.NewValue == null)
                return;

            TabControl.Clear();
            Current.Value = null;

            TabControl.AddItem(index_page_string);

            if (e.NewValue.Path == index_path)
            {
                Current.Value = index_page_string;
                return;
            }

            if (e.NewValue.Subtitle != null)
                TabControl.AddItem(e.NewValue.Subtitle);

            TabControl.AddItem(e.NewValue.Title);
            Current.Value = e.NewValue.Title;
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

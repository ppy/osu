// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Wiki
{
    public class WikiHeader : BreadcrumbControlOverlayHeader
    {
        private const string index_page_string = "index";
        private const string index_path = "Main_Page";

        public readonly Bindable<APIWikiPage> WikiPageData = new Bindable<APIWikiPage>();

        public Action ShowIndexPage;
        public Action ShowParentPage;

        public WikiHeader()
        {
            TabControl.AddItem(index_page_string);
            Current.Value = index_page_string;

            WikiPageData.BindValueChanged(onWikiPageChange);
            Current.BindValueChanged(onCurrentChange);
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

        private void onCurrentChange(ValueChangedEvent<string> e)
        {
            if (e.NewValue == TabControl.Items.LastOrDefault())
                return;

            if (e.NewValue == index_page_string)
            {
                ShowIndexPage?.Invoke();
                return;
            }

            ShowParentPage?.Invoke();
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

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Localisation;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Wiki
{
    public class WikiHeader : BreadcrumbControlOverlayHeader
    {
        private const string index_path = "Main_Page";
        public static LocalisableString IndexPageString => WikiStrings.IndexPageString;
        public static LocalisableString HeaderTitle => WikiStrings.HeaderTitle;
        public static LocalisableString HeaderDescription => WikiStrings.HeaderDescription;

        public readonly Bindable<APIWikiPage> WikiPageData = new Bindable<APIWikiPage>();

        public Action ShowIndexPage;
        public Action ShowParentPage;

        public WikiHeader()
        {
            TabControl.AddItem(IndexPageString);
            Current.Value = IndexPageString;

            WikiPageData.BindValueChanged(onWikiPageChange);
            Current.BindValueChanged(onCurrentChange);
        }

        private void onWikiPageChange(ValueChangedEvent<APIWikiPage> e)
        {
            if (e.NewValue == null)
                return;

            TabControl.Clear();
            Current.Value = string.Empty;

            TabControl.AddItem(IndexPageString);

            if (e.NewValue.Path == index_path)
            {
                Current.Value = IndexPageString;
                return;
            }

            if (e.NewValue.Subtitle != null)
                TabControl.AddItem(e.NewValue.Subtitle);

            TabControl.AddItem(e.NewValue.Title);
            Current.Value = e.NewValue.Title;
        }

        private void onCurrentChange(ValueChangedEvent<LocalisableString> e)
        {
            if (e.NewValue == TabControl.Items.LastOrDefault())
                return;

            if (e.NewValue == IndexPageString)
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
                Title = HeaderTitle;
                Description = HeaderDescription;
                IconTexture = "Icons/Hexacons/wiki";
            }
        }
    }
}

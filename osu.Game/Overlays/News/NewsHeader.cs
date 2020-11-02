// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace osu.Game.Overlays.News
{
    public class NewsHeader : BreadcrumbControlOverlayHeader
    {
        private const string front_page_string = "frontpage";

        public Action ShowFrontPage;

        private readonly Bindable<string> article = new Bindable<string>(null);

        public NewsHeader()
        {
            TabControl.AddItem(front_page_string);

            Current.BindValueChanged(e =>
            {
                if (e.NewValue == front_page_string)
                    ShowFrontPage?.Invoke();
            });

            article.BindValueChanged(onArticleChanged, true);
        }

        public void SetFrontPage() => article.Value = null;

        public void SetArticle(string slug) => article.Value = slug;

        private void onArticleChanged(ValueChangedEvent<string> e)
        {
            if (e.OldValue != null)
                TabControl.RemoveItem(e.OldValue);

            if (e.NewValue != null)
            {
                TabControl.AddItem(e.NewValue);
                Current.Value = e.NewValue;
            }
            else
            {
                Current.Value = front_page_string;
            }
        }

        protected override Drawable CreateBackground() => new OverlayHeaderBackground(@"Headers/news");

        protected override OverlayTitle CreateTitle() => new NewsHeaderTitle();

        private class NewsHeaderTitle : OverlayTitle
        {
            public NewsHeaderTitle()
            {
                Title = "news";
                Description = "get up-to-date on community happenings";
                IconTexture = "Icons/Hexacons/news";
            }
        }
    }
}

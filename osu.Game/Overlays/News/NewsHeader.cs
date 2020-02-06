// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using System;

namespace osu.Game.Overlays.News
{
    public class NewsHeader : BreadcrumbControlOverlayHeader
    {
        private const string front_page_string = "frontpage";

        private NewsHeaderTitle title;

        public readonly Bindable<string> Post = new Bindable<string>(null);

        public Action ShowFrontPage;

        public NewsHeader()
        {
            TabControl.AddItem(front_page_string);

            Current.ValueChanged += e =>
            {
                if (e.NewValue == front_page_string)
                    ShowFrontPage?.Invoke();
            };

            Post.ValueChanged += showPost;
        }

        private void showPost(ValueChangedEvent<string> e)
        {
            if (e.OldValue != null)
                TabControl.RemoveItem(e.OldValue);

            if (e.NewValue != null)
            {
                TabControl.AddItem(e.NewValue);
                Current.Value = e.NewValue;

                title.IsReadingPost = true;
            }
            else
            {
                Current.Value = front_page_string;
                title.IsReadingPost = false;
            }
        }

        protected override Drawable CreateBackground() => new OverlayHeaderBackground(@"Headers/news");

        protected override ScreenTitle CreateTitle() => title = new NewsHeaderTitle();

        private class NewsHeaderTitle : ScreenTitle
        {
            private const string post_string = "post";

            public bool IsReadingPost
            {
                set => Section = value ? post_string : front_page_string;
            }

            public NewsHeaderTitle()
            {
                Title = "news";
                IsReadingPost = false;
            }

            protected override Drawable CreateIcon() => new ScreenTitleTextureIcon(@"Icons/news");
        }
    }
}

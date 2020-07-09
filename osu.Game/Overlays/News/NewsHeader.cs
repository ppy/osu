// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace osu.Game.Overlays.News
{
    public class NewsHeader : BreadcrumbControlOverlayHeader
    {
        public const string FRONT_PAGE_STRING = "frontpage";

        public readonly Bindable<string> Post = new Bindable<string>(FRONT_PAGE_STRING);

        public NewsHeader()
        {
            TabControl.AddItem(FRONT_PAGE_STRING);
            Current.Value = FRONT_PAGE_STRING;
            Current.BindValueChanged(onCurrentChanged);
            Post.BindValueChanged(onPostChanged, true);
        }

        public void SetFrontPage() => Post.Value = FRONT_PAGE_STRING;

        public void SetArticle(string slug) => Post.Value = slug;

        private void onCurrentChanged(ValueChangedEvent<string> current)
        {
            if (current.NewValue == FRONT_PAGE_STRING)
                Post.Value = FRONT_PAGE_STRING;
        }

        private void onPostChanged(ValueChangedEvent<string> post)
        {
            if (post.OldValue != FRONT_PAGE_STRING)
                TabControl.RemoveItem(post.OldValue);

            if (post.NewValue != FRONT_PAGE_STRING)
            {
                TabControl.AddItem(post.NewValue);
                Current.Value = post.NewValue;
            }
            else
            {
                Current.Value = FRONT_PAGE_STRING;
            }
        }

        protected override Drawable CreateBackground() => new OverlayHeaderBackground(@"Headers/news");

        protected override OverlayTitle CreateTitle() => new NewsHeaderTitle();

        private class NewsHeaderTitle : OverlayTitle
        {
            public NewsHeaderTitle()
            {
                Title = "news";
                IconTexture = "Icons/news";
            }
        }
    }
}

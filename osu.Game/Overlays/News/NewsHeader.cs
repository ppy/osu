// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using System;

namespace osu.Game.Overlays.News
{
    public class NewsHeader : BreadcrumbControlOverlayHeader
    {
        private const string front_page_string = "frontpage";

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
                IconTexture = "Icons/news";
            }
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using System;

namespace osu.Game.Overlays.News
{
    public class NewsHeader : OverlayHeader
    {
        private const string front_page_string = "Front Page";

        private NewsHeaderTitle title;

        public readonly Bindable<string> Current = new Bindable<string>(null);

        public Action ShowFrontPage;

        public NewsHeader()
        {
            TabControl.AddItem(front_page_string);

            TabControl.Current.ValueChanged += e =>
            {
                if (e.NewValue == front_page_string)
                    ShowFrontPage?.Invoke();
            };

            Current.ValueChanged += showArticle;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            TabControl.AccentColour = colour.Violet;
        }

        private void showArticle(ValueChangedEvent<string> e)
        {
            if (e.OldValue != null)
                TabControl.RemoveItem(e.OldValue);

            if (e.NewValue != null)
            {
                TabControl.AddItem(e.NewValue);
                TabControl.Current.Value = e.NewValue;

                title.IsReadingArticle = true;
            }
            else
            {
                TabControl.Current.Value = front_page_string;
                title.IsReadingArticle = false;
            }
        }

        protected override Drawable CreateBackground() => new NewsHeaderBackground();

        protected override Drawable CreateContent() => new Container();

        protected override ScreenTitle CreateTitle() => title = new NewsHeaderTitle();

        private class NewsHeaderBackground : Sprite
        {
            public NewsHeaderBackground()
            {
                RelativeSizeAxes = Axes.Both;
                FillMode = FillMode.Fill;
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                Texture = textures.Get(@"Headers/news");
            }
        }

        private class NewsHeaderTitle : ScreenTitle
        {
            private const string article_string = "Article";

            public bool IsReadingArticle
            {
                set => Section = value ? article_string : front_page_string;
            }

            public NewsHeaderTitle()
            {
                Title = "News";
                IsReadingArticle = false;
            }

            protected override Drawable CreateIcon() => new ScreenTitleTextureIcon(@"Icons/news");

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                AccentColour = colours.Violet;
            }
        }
    }
}

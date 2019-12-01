// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Overlays.News
{
    public class NewsArticleCover : CompositeDrawable
    {
        public NewsArticleCover(string url)
        {
            NewsArticleCoverBackground background;
            InternalChild = new DelayedLoadWrapper(background = new NewsArticleCoverBackground(url));

            background.OnLoadComplete += bg => bg.FadeInFromZero(400, Easing.Out);
        }

        [LongRunningLoad]
        private class NewsArticleCoverBackground : Sprite
        {
            private string url;

            public NewsArticleCoverBackground(string url)
            {
                this.url = url;

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                RelativeSizeAxes = Axes.Both;
                FillMode = FillMode.Fill;
            }

            [BackgroundDependencyLoader]
            private void load(LargeTextureStore store)
            {
                Texture = store.Get(url ?? "Headers/news");
            }
        }
    }
}

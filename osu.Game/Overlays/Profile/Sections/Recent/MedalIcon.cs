// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Overlays.Profile.Sections.Recent
{
    [LongRunningLoad]
    public class MedalIcon : Container
    {
        private readonly string slug;
        private readonly Sprite sprite;

        private string url => $@"https://s.ppy.sh/images/medals-client/{slug}@2x.png";

        public MedalIcon(string slug)
        {
            this.slug = slug;

            Child = sprite = new Sprite
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
        }

        [BackgroundDependencyLoader]
        private void load(LargeTextureStore textures)
        {
            sprite.Texture = textures.Get(url);
        }
    }
}

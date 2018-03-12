// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Overlays.Profile.Sections.Recent
{
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
                Height = 40,
                Width = 40,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            sprite.Texture = textures.Get(url);
        }
    }
}

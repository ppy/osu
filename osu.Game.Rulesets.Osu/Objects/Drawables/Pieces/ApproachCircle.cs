// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class ApproachCircle : Container
    {
        public ApproachCircle()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            Child = new SkinnableDrawable("Play/osu/approachcircle", name => new Sprite { Texture = textures.Get(name) });
        }
    }
}

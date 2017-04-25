// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class ApproachCircle : Container
    {
        private readonly Sprite approachCircle;

        public ApproachCircle()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            AutoSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                approachCircle = new Sprite()
            };
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            approachCircle.Texture = textures.Get(@"Play/osu/approachcircle");
        }
    }
}
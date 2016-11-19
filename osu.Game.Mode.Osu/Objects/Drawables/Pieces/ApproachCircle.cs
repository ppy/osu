using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using OpenTK;

namespace osu.Game.Modes.Osu.Objects.Drawables.Pieces
{
    public class ApproachCircle : Container
    {
        private Sprite approachCircle;

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
            approachCircle.Texture = textures.Get(@"Play/osu/approachcircle@2x");
        }
    }
}
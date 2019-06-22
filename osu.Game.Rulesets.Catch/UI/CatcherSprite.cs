// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatcherSprite : SkinReloadableDrawable
    {
        public CatcherSprite()
        {
            Size = new Vector2(CatcherArea.CATCHER_SIZE);

            // Sets the origin roughly to the centre of the catcher's plate to allow for correct scaling.
            OriginPosition = new Vector2(-0.02f, 0.06f) * CatcherArea.CATCHER_SIZE;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Child = new SkinnableDrawable("fruit-catcher-idle", _ => new Sprite
                {
                    Texture = textures.Get(@"Play/Catch/fruit-catcher-idle"),
                    RelativeSizeAxes = Axes.Both,
                    Size = Vector2.One,
                }, restrictSize: true)
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                }
            };
        }
    }
}

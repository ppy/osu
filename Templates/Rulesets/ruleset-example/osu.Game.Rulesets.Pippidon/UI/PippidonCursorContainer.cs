// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Pippidon.UI
{
    public partial class PippidonCursorContainer : GameplayCursorContainer
    {
        private Sprite cursorSprite;
        private Texture cursorTexture;

        protected override Drawable CreateCursor() => cursorSprite = new Sprite
        {
            Scale = new Vector2(0.5f),
            Origin = Anchor.Centre,
            Texture = cursorTexture,
        };

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            cursorTexture = textures.Get("character");

            if (cursorSprite != null)
                cursorSprite.Texture = cursorTexture;
        }
    }
}

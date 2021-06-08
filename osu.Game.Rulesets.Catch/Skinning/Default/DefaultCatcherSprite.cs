// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Catch.UI;

namespace osu.Game.Rulesets.Catch.Skinning.Default
{
    public class DefaultCatcherSprite : Sprite
    {
        private readonly CatcherAnimationState state;

        public DefaultCatcherSprite(CatcherAnimationState state)
        {
            this.state = state;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            Texture = textures.Get($"Gameplay/catch/fruit-catcher-{state.ToString().ToLower()}");
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Tournament.Components
{
    public class DrawableTournamentHeaderText : Sprite
    {
        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            Texture = textures.Get("header-text");
        }
    }
}

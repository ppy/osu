// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Skinning.Legacy
{
    internal partial class LegacySwellCirclePiece : Sprite
    {
        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, SkinManager skinManager)
        {
            Texture = skin.GetTexture("spinner-warning") ?? skinManager.DefaultClassicSkin.GetTexture("spinner-circle");
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Scale = skin.GetTexture("spinner-warning") != null ? Vector2.One : new Vector2(0.18f);
        }
    }
}

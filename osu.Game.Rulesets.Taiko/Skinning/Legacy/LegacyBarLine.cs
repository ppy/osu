// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Skinning.Legacy
{
    public class LegacyBarLine : Sprite
    {
        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            Texture = skin.GetTexture("taiko-barline");

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(1, 0.88f);
            FillMode = FillMode.Fill;
        }
    }
}

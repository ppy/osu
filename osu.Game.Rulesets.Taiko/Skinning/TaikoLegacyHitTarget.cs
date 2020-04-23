// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Skinning
{
    public class TaikoLegacyHitTarget : CompositeDrawable
    {
        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new Sprite
                {
                    Texture = skin.GetTexture("approachcircle"),
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.73f) * 0.625f,
                    Alpha = 0.7f,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                new Sprite
                {
                    Texture = skin.GetTexture("taikobigcircle"),
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.7f) * 0.625f,
                    Alpha = 0.5f,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            };
        }
    }
}

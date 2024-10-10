// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Skinning.Legacy
{
    public partial class TaikoLegacyHitTarget : CompositeDrawable
    {
        /// <summary>
        /// In stable this is 0.7f (see https://github.com/peppy/osu-stable-reference/blob/7519cafd1823f1879c0d9c991ba0e5c7fd3bfa02/osu!/GameModes/Play/Rulesets/Taiko/RulesetTaiko.cs#L592)
        /// but for whatever reason this doesn't match visually.
        /// </summary>
        public const float SCALE = 0.8f;

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new Sprite
                {
                    Texture = skin.GetTexture("approachcircle"),
                    Scale = new Vector2(SCALE + 0.03f),
                    Alpha = 0.47f, // eyeballed to match stable
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                new Sprite
                {
                    Texture = skin.GetTexture("taikobigcircle"),
                    Scale = new Vector2(SCALE),
                    Alpha = 0.22f, // eyeballed to match stable
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            };
        }
    }
}

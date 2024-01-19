// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Skinning.Legacy
{
    public partial class TaikoLegacyHitTarget : CompositeDrawable
    {
        private Container content = null!;

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            RelativeSizeAxes = Axes.Both;

            InternalChild = content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children = new Drawable[]
                {
                    new Sprite
                    {
                        Texture = skin.GetTexture("approachcircle"),
                        Scale = new Vector2(0.83f),
                        Alpha = 0.47f, // eyeballed to match stable
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                    new Sprite
                    {
                        Texture = skin.GetTexture("taikobigcircle"),
                        Scale = new Vector2(0.8f),
                        Alpha = 0.22f, // eyeballed to match stable
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                }
            };
        }

        protected override void Update()
        {
            base.Update();

            // Relying on RelativeSizeAxes.Both + FillMode.Fit doesn't work due to the precise pixel layout requirements.
            // This is a bit ugly but makes the non-legacy implementations a lot cleaner to implement.
            content.Scale = new Vector2(DrawHeight / TaikoPlayfield.BASE_HEIGHT);
        }
    }
}

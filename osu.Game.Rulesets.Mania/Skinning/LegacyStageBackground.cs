// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Mania.Skinning
{
    public class LegacyStageBackground : LegacyManiaElement
    {
        private Drawable leftSprite;
        private Drawable rightSprite;

        public LegacyStageBackground()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            string leftImage = GetManiaSkinConfig<string>(skin, LegacyManiaSkinConfigurationLookups.LeftStageImage)?.Value
                               ?? "mania-stage-left";

            string rightImage = GetManiaSkinConfig<string>(skin, LegacyManiaSkinConfigurationLookups.RightStageImage)?.Value
                                ?? "mania-stage-right";

            InternalChildren = new[]
            {
                leftSprite = new Sprite
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopRight,
                    X = 0.05f,
                    Texture = skin.GetTexture(leftImage),
                },
                rightSprite = new Sprite
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopLeft,
                    X = -0.05f,
                    Texture = skin.GetTexture(rightImage)
                }
            };
        }

        protected override void Update()
        {
            base.Update();

            if (leftSprite?.Height > 0)
                leftSprite.Scale = new Vector2(1, DrawHeight / leftSprite.Height);

            if (rightSprite?.Height > 0)
                rightSprite.Scale = new Vector2(1, DrawHeight / rightSprite.Height);
        }
    }
}

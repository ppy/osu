// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning
{
    public class LegacySliderBall : CompositeDrawable
    {
        private readonly Drawable animationContent;

        public LegacySliderBall(Drawable animationContent)
        {
            this.animationContent = animationContent;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, DrawableHitObject drawableObject)
        {
            animationContent.Colour = skin.GetConfig<OsuSkinColour, Color4>(OsuSkinColour.SliderBall)?.Value ?? Color4.White;

            InternalChildren = new[]
            {
                new Sprite
                {
                    Texture = skin.GetTexture("sliderb-nd"),
                    Colour = new Color4(5, 5, 5, 255),
                },
                animationContent,
                new Sprite
                {
                    Texture = skin.GetTexture("sliderb-spec"),
                    Blending = BlendingParameters.Additive,
                },
            };
        }
    }
}

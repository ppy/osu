// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning
{
    public class LegacySliderTail : CompositeDrawable
    {
        public LegacySliderTail()
        {
            Size = new Vector2(OsuHitObject.OBJECT_RADIUS / 2);
        }

        private readonly IBindable<ArmedState> state = new Bindable<ArmedState>();
        private readonly Bindable<Color4> accentColour = new Bindable<Color4>();

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableObject, ISkinSource skin)
        {
            Sprite tailCircleSprite;

            InternalChildren = new Drawable[]
            {
                tailCircleSprite = new Sprite
                {
                    Texture = skin.GetTexture("sliderendcircle") ?? skin.GetTexture("hitcircle"),
                    Colour = drawableObject.AccentColour.Value,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                new Sprite
                {
                    Texture = skin.GetTexture("sliderendcircleoverlay") ?? skin.GetTexture("hitcircleoverlay"),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            };

            state.BindTo(drawableObject.State);
            state.BindValueChanged(updateState, true);

            accentColour.BindTo(drawableObject.AccentColour);
            accentColour.BindValueChanged(colour => tailCircleSprite.Colour = colour.NewValue, true);
        }

        private void updateState(ValueChangedEvent<ArmedState> state)
        {
            const double legacy_fade_duration = 240;

            switch (state.NewValue)
            {
                case ArmedState.Hit:
                    this.FadeOut(legacy_fade_duration, Easing.Out);
                    this.ScaleTo(1.4f, legacy_fade_duration, Easing.Out);
                    break;
            }
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning
{
    public class LegacyMainCirclePiece : CompositeDrawable
    {
        public LegacyMainCirclePiece()
        {
            Size = new Vector2(OsuHitObject.OBJECT_RADIUS * 2);
        }

        private readonly IBindable<ArmedState> state = new Bindable<ArmedState>();
        private readonly Bindable<Color4> accentColour = new Bindable<Color4>();
        private readonly IBindable<int> indexInCurrentCombo = new Bindable<int>();

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableObject, ISkinSource skin)
        {
            OsuHitObject osuObject = (OsuHitObject)drawableObject.HitObject;

            Sprite hitCircleSprite;
            SkinnableSpriteText hitCircleText;

            InternalChildren = new Drawable[]
            {
                hitCircleSprite = new Sprite
                {
                    Texture = skin.GetTexture("hitcircle"),
                    Colour = drawableObject.AccentColour.Value,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                hitCircleText = new SkinnableSpriteText(new OsuSkinComponent(OsuSkinComponents.HitCircleText), _ => new OsuSpriteText
                {
                    Font = OsuFont.Numeric.With(size: 40),
                    UseFullGlyphHeight = false,
                }, confineMode: ConfineMode.NoScaling),
                new Sprite
                {
                    Texture = skin.GetTexture("hitcircleoverlay"),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            };

            state.BindTo(drawableObject.State);
            state.BindValueChanged(updateState, true);

            accentColour.BindTo(drawableObject.AccentColour);
            accentColour.BindValueChanged(colour => hitCircleSprite.Colour = colour.NewValue, true);

            indexInCurrentCombo.BindTo(osuObject.IndexInCurrentComboBindable);
            indexInCurrentCombo.BindValueChanged(index => hitCircleText.Text = (index.NewValue + 1).ToString(), true);
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

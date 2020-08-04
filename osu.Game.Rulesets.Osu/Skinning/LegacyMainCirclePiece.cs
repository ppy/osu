// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
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
        private readonly string priorityLookup;

        public LegacyMainCirclePiece(string priorityLookup = null)
        {
            this.priorityLookup = priorityLookup;

            Size = new Vector2(OsuHitObject.OBJECT_RADIUS * 2);
        }

        private Sprite hitCircleSprite, hitCircleOverlay;
        private SkinnableSpriteText hitCircleText;

        private readonly IBindable<ArmedState> state = new Bindable<ArmedState>();
        private readonly Bindable<Color4> accentColour = new Bindable<Color4>();
        private readonly IBindable<int> indexInCurrentCombo = new Bindable<int>();

        [Resolved]
        private ISkinSource skin { get; set; }

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableObject)
        {
            OsuHitObject osuObject = (OsuHitObject)drawableObject.HitObject;

            InternalChildren = new Drawable[]
            {
                hitCircleSprite = new Sprite
                {
                    Texture = getTextureWithFallback(string.Empty),
                    Colour = drawableObject.AccentColour.Value,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                hitCircleText = new SkinnableSpriteText(new OsuSkinComponent(OsuSkinComponents.HitCircleText), _ => new OsuSpriteText
                {
                    Font = OsuFont.Numeric.With(size: 40),
                    UseFullGlyphHeight = false,
                }, confineMode: ConfineMode.NoScaling)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                hitCircleOverlay = new Sprite
                {
                    Texture = getTextureWithFallback("overlay"),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            };

            bool overlayAboveNumber = skin.GetConfig<OsuSkinConfiguration, bool>(OsuSkinConfiguration.HitCircleOverlayAboveNumber)?.Value ?? true;

            if (!overlayAboveNumber)
                ChangeInternalChildDepth(hitCircleText, -float.MaxValue);

            state.BindTo(drawableObject.State);
            accentColour.BindTo(drawableObject.AccentColour);
            indexInCurrentCombo.BindTo(osuObject.IndexInCurrentComboBindable);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            state.BindValueChanged(updateState, true);
            accentColour.BindValueChanged(colour => hitCircleSprite.Colour = colour.NewValue, true);
            indexInCurrentCombo.BindValueChanged(index => hitCircleText.Text = (index.NewValue + 1).ToString(), true);

            Texture getTextureWithFallback(string name)
            {
                Texture tex = null;

                if (!string.IsNullOrEmpty(priorityLookup))
                    tex = skin.GetTexture($"{priorityLookup}{name}");

                return tex ?? skin.GetTexture($"hitcircle{name}");
            }
        }

        private void updateState(ValueChangedEvent<ArmedState> state)
        {
            const double legacy_fade_duration = 240;

            switch (state.NewValue)
            {
                case ArmedState.Hit:
                    this.FadeOut(legacy_fade_duration, Easing.Out);

                    hitCircleSprite.ScaleTo(1.4f, legacy_fade_duration, Easing.Out);
                    hitCircleOverlay.ScaleTo(1.4f, legacy_fade_duration, Easing.Out);

                    if (skin.GetConfig<OsuSkinConfiguration, bool>(OsuSkinConfiguration.ExpandNumberPiece)?.Value ?? true)
                        hitCircleText.ScaleTo(1.4f, legacy_fade_duration, Easing.Out);

                    break;
            }
        }
    }
}

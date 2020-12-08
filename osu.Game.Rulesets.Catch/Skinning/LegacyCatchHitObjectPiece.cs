// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Skinning
{
    public abstract class LegacyCatchHitObjectPiece : PoolableDrawable
    {
        public readonly Bindable<Color4> AccentColour = new Bindable<Color4>();
        public readonly Bindable<bool> HyperDash = new Bindable<bool>();

        private readonly Sprite colouredSprite;
        private readonly Sprite overlaySprite;
        private readonly Sprite hyperSprite;

        [Resolved]
        protected ISkinSource Skin { get; private set; }

        [Resolved(canBeNull: true)]
        [CanBeNull]
        protected DrawableHitObject DrawableHitObject { get; private set; }

        protected LegacyCatchHitObjectPiece()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                colouredSprite = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                overlaySprite = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                hyperSprite = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Blending = BlendingParameters.Additive,
                    Depth = 1,
                    Alpha = 0,
                    Scale = new Vector2(1.2f),
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            var hitObject = (DrawablePalpableCatchHitObject)DrawableHitObject;

            if (hitObject != null)
            {
                AccentColour.BindTo(hitObject.AccentColour);
                HyperDash.BindTo(hitObject.HyperDash);
            }

            hyperSprite.Colour = Skin.GetConfig<CatchSkinColour, Color4>(CatchSkinColour.HyperDashFruit)?.Value ??
                                 Skin.GetConfig<CatchSkinColour, Color4>(CatchSkinColour.HyperDash)?.Value ??
                                 Catcher.DEFAULT_HYPER_DASH_COLOUR;

            AccentColour.BindValueChanged(colour =>
            {
                colouredSprite.Colour = LegacyColourCompatibility.DisallowZeroAlpha(colour.NewValue);
            }, true);

            HyperDash.BindValueChanged(hyper =>
            {
                hyperSprite.Alpha = hyper.NewValue ? 0.7f : 0;
            }, true);
        }

        protected void SetTexture(Texture texture, Texture overlayTexture)
        {
            colouredSprite.Texture = texture;
            overlaySprite.Texture = overlayTexture;
            hyperSprite.Texture = texture;
        }
    }
}

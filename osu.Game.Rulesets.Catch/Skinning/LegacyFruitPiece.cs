// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Skinning
{
    internal class LegacyFruitPiece : CompositeDrawable
    {
        private readonly string lookupName;

        private readonly IBindable<Color4> accentColour = new Bindable<Color4>();
        private Sprite colouredSprite;

        public LegacyFruitPiece(string lookupName)
        {
            this.lookupName = lookupName;
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableObject, ISkinSource skin)
        {
            DrawableCatchHitObject drawableCatchObject = (DrawableCatchHitObject)drawableObject;

            accentColour.BindTo(drawableCatchObject.AccentColour);

            InternalChildren = new Drawable[]
            {
                colouredSprite = new Sprite
                {
                    Texture = skin.GetTexture(lookupName),
                    Colour = drawableObject.AccentColour.Value,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                new Sprite
                {
                    Texture = skin.GetTexture($"{lookupName}-overlay"),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            };

            if (drawableCatchObject.HitObject.HyperDash)
            {
                var hyperDash = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Blending = BlendingParameters.Additive,
                    Depth = 1,
                    Alpha = 0.7f,
                    Scale = new Vector2(1.2f),
                    Texture = skin.GetTexture(lookupName),
                    Colour = skin.GetConfig<CatchSkinColour, Color4>(CatchSkinColour.HyperDashFruit)?.Value ??
                             skin.GetConfig<CatchSkinColour, Color4>(CatchSkinColour.HyperDash)?.Value ??
                             Catcher.DEFAULT_HYPER_DASH_COLOUR,
                };

                AddInternal(hyperDash);
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            accentColour.BindValueChanged(colour => colouredSprite.Colour = colour.NewValue, true);
        }
    }
}

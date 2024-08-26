// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Skinning.Legacy
{
    public class CatchLegacySkinTransformer : LegacySkinTransformer
    {
        public override bool IsProvidingLegacyResources => base.IsProvidingLegacyResources || hasPear;

        private bool hasPear => GetTexture("fruit-pear") != null;

        /// <summary>
        /// For simplicity, let's use legacy combo font texture existence as a way to identify legacy skins from default.
        /// </summary>
        private bool providesComboCounter => this.HasFont(LegacyFont.Combo);

        public CatchLegacySkinTransformer(ISkin skin)
            : base(skin)
        {
        }

        public override Drawable? GetDrawableComponent(ISkinComponentLookup lookup)
        {
            switch (lookup)
            {
                case GlobalSkinnableContainerLookup containerLookup:
                    // Only handle per ruleset defaults here.
                    if (containerLookup.Ruleset == null)
                        break;

                    // we don't have enough assets to display these components (this is especially the case on a "beatmap" skin).
                    if (!IsProvidingLegacyResources)
                        break;

                    // Our own ruleset components default.
                    switch (containerLookup.Lookup)
                    {
                        case GlobalSkinnableContainers.MainHUDComponents:
                            // todo: remove CatchSkinComponents.CatchComboCounter and refactor LegacyCatchComboCounter to be added here instead.
                            return new DefaultSkinComponentsContainer(container =>
                            {
                                var keyCounter = container.OfType<LegacyKeyCounterDisplay>().FirstOrDefault();

                                if (keyCounter != null)
                                {
                                    // set the anchor to top right so that it won't squash to the return button to the top
                                    keyCounter.Anchor = Anchor.CentreRight;
                                    keyCounter.Origin = Anchor.TopRight;
                                    keyCounter.Position = new Vector2(0, -40) * 1.6f;
                                }
                            })
                            {
                                Children = new Drawable[]
                                {
                                    new LegacyKeyCounterDisplay(),
                                }
                            };
                    }

                    break;

                case CatchSkinComponentLookup catchSkinComponent:
                    switch (catchSkinComponent.Component)
                    {
                        case CatchSkinComponents.Fruit:
                            if (hasPear)
                                return new LegacyFruitPiece();

                            break;

                        case CatchSkinComponents.Banana:
                            if (GetTexture("fruit-bananas") != null)
                                return new LegacyBananaPiece();

                            break;

                        case CatchSkinComponents.Droplet:
                            if (GetTexture("fruit-drop") != null)
                                return new LegacyDropletPiece();

                            break;

                        case CatchSkinComponents.Catcher:
                            decimal version = GetConfig<SkinConfiguration.LegacySetting, decimal>(SkinConfiguration.LegacySetting.Version)?.Value ?? 1;

                            if (version < 2.3m)
                            {
                                if (hasOldStyleCatcherSprite())
                                    return new LegacyCatcherOld();
                            }

                            if (hasNewStyleCatcherSprite())
                                return new LegacyCatcherNew();

                            break;

                        case CatchSkinComponents.CatchComboCounter:
                            if (providesComboCounter)
                                return new LegacyCatchComboCounter();

                            break;

                        case CatchSkinComponents.HitExplosion:
                            if (hasOldStyleCatcherSprite() || hasNewStyleCatcherSprite())
                                return new LegacyHitExplosion();

                            break;

                        default:
                            throw new UnsupportedSkinComponentException(lookup);
                    }

                    break;
            }

            return base.GetDrawableComponent(lookup);
        }

        private bool hasOldStyleCatcherSprite() =>
            GetTexture(@"fruit-ryuuta") != null
            || GetTexture(@"fruit-ryuuta-0") != null;

        private bool hasNewStyleCatcherSprite() =>
            GetTexture(@"fruit-catcher-idle") != null
            || GetTexture(@"fruit-catcher-idle-0") != null;

        public override IBindable<TValue>? GetConfig<TLookup, TValue>(TLookup lookup)
        {
            switch (lookup)
            {
                case CatchSkinColour colour:
                    var result = (Bindable<Color4>?)base.GetConfig<SkinCustomColourLookup, TValue>(new SkinCustomColourLookup(colour));
                    if (result == null)
                        return null;

                    result.Value = LegacyColourCompatibility.DisallowZeroAlpha(result.Value);
                    return (IBindable<TValue>)result;
            }

            return base.GetConfig<TLookup, TValue>(lookup);
        }
    }
}

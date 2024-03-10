// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Skinning;
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
            if (lookup is SkinComponentsContainerLookup containerLookup)
            {
                switch (containerLookup.Target)
                {
                    case SkinComponentsContainerLookup.TargetArea.MainHUDComponents:
                        var components = base.GetDrawableComponent(lookup) as Container;

                        if (providesComboCounter && components != null)
                        {
                            // catch may provide its own combo counter; hide the default.
                            // todo: this should be done in an elegant way per ruleset, defining which HUD skin components should be displayed.
                            foreach (var legacyComboCounter in components.OfType<LegacyComboCounter>())
                                legacyComboCounter.HiddenByRulesetImplementation = false;
                        }

                        return components;
                }
            }

            if (lookup is CatchSkinComponentLookup catchSkinComponent)
            {
                switch (catchSkinComponent.Component)
                {
                    case CatchSkinComponents.Fruit:
                        if (hasPear)
                            return new LegacyFruitPiece();

                        return null;

                    case CatchSkinComponents.Banana:
                        if (GetTexture("fruit-bananas") != null)
                            return new LegacyBananaPiece();

                        return null;

                    case CatchSkinComponents.Droplet:
                        if (GetTexture("fruit-drop") != null)
                            return new LegacyDropletPiece();

                        return null;

                    case CatchSkinComponents.Catcher:
                        decimal version = GetConfig<SkinConfiguration.LegacySetting, decimal>(SkinConfiguration.LegacySetting.Version)?.Value ?? 1;

                        if (version < 2.3m)
                        {
                            if (hasOldStyleCatcherSprite())
                                return new LegacyCatcherOld();
                        }

                        if (hasNewStyleCatcherSprite())
                            return new LegacyCatcherNew();

                        return null;

                    case CatchSkinComponents.CatchComboCounter:
                        if (providesComboCounter)
                            return new LegacyCatchComboCounter();

                        return null;

                    case CatchSkinComponents.HitExplosion:
                        if (hasOldStyleCatcherSprite() || hasNewStyleCatcherSprite())
                            return new LegacyHitExplosion();

                        return null;

                    default:
                        throw new UnsupportedSkinComponentException(lookup);
                }
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

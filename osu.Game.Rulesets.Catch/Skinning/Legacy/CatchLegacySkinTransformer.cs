// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Skinning.Legacy
{
    public class CatchLegacySkinTransformer : LegacySkinTransformer
    {
        /// <summary>
        /// For simplicity, let's use legacy combo font texture existence as a way to identify legacy skins from default.
        /// </summary>
        private bool providesComboCounter => this.HasFont(LegacyFont.Combo);

        public CatchLegacySkinTransformer(ISkin skin)
            : base(skin)
        {
        }

        public override Drawable GetDrawableComponent(ISkinComponent component)
        {
            if (component is SkinnableTargetComponent targetComponent)
            {
                switch (targetComponent.Target)
                {
                    case SkinnableTarget.MainHUDComponents:
                        var components = base.GetDrawableComponent(component) as SkinnableTargetComponentsContainer;

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

            if (component is CatchSkinComponent catchSkinComponent)
            {
                switch (catchSkinComponent.Component)
                {
                    case CatchSkinComponents.Fruit:
                        if (GetTexture("fruit-pear") != null)
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
                            return new LegacyCatchComboCounter(Skin);

                        return null;

                    case CatchSkinComponents.HitExplosion:
                        if (hasOldStyleCatcherSprite() || hasNewStyleCatcherSprite())
                            return new LegacyHitExplosion();

                        return null;
                }
            }

            return base.GetDrawableComponent(component);
        }

        private bool hasOldStyleCatcherSprite() =>
            GetTexture(@"fruit-ryuuta") != null
            || GetTexture(@"fruit-ryuuta-0") != null;

        private bool hasNewStyleCatcherSprite() =>
            GetTexture(@"fruit-catcher-idle") != null
            || GetTexture(@"fruit-catcher-idle-0") != null;

        public override IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup)
        {
            switch (lookup)
            {
                case CatchSkinColour colour:
                    var result = (Bindable<Color4>)base.GetConfig<SkinCustomColourLookup, TValue>(new SkinCustomColourLookup(colour));
                    if (result == null)
                        return null;

                    result.Value = LegacyColourCompatibility.DisallowZeroAlpha(result.Value);
                    return (IBindable<TValue>)result;

                case CatchSkinConfiguration config:
                    switch (config)
                    {
                        case CatchSkinConfiguration.FlipCatcherPlate:
                            // Don't flip catcher plate contents if the catcher is provided by this legacy skin.
                            if (GetDrawableComponent(new CatchSkinComponent(CatchSkinComponents.Catcher)) != null)
                                return (IBindable<TValue>)new Bindable<bool>();

                            break;
                    }

                    break;
            }

            return base.GetConfig<TLookup, TValue>(lookup);
        }
    }
}

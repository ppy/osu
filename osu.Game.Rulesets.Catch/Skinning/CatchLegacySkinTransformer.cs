// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Humanizer;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;
using static osu.Game.Skinning.LegacySkinConfiguration;

namespace osu.Game.Rulesets.Catch.Skinning
{
    public class CatchLegacySkinTransformer : LegacySkinTransformer
    {
        /// <summary>
        /// For simplicity, let's use legacy combo font texture existence as a way to identify legacy skins from default.
        /// </summary>
        private bool providesComboCounter => this.HasFont(GetConfig<LegacySetting, string>(LegacySetting.ComboPrefix)?.Value ?? "score");

        public CatchLegacySkinTransformer(ISkinSource source)
            : base(source)
        {
        }

        public override Drawable GetDrawableComponent(ISkinComponent component)
        {
            if (component is HUDSkinComponent hudComponent)
            {
                switch (hudComponent.Component)
                {
                    case HUDSkinComponents.ComboCounter:
                        // catch may provide its own combo counter; hide the default.
                        return providesComboCounter ? Drawable.Empty() : null;
                }
            }

            if (!(component is CatchSkinComponent catchSkinComponent))
                return null;

            switch (catchSkinComponent.Component)
            {
                case CatchSkinComponents.FruitApple:
                case CatchSkinComponents.FruitBananas:
                case CatchSkinComponents.FruitOrange:
                case CatchSkinComponents.FruitGrapes:
                case CatchSkinComponents.FruitPear:
                    var lookupName = catchSkinComponent.Component.ToString().Kebaberize();
                    if (GetTexture(lookupName) != null)
                        return new LegacyFruitPiece(lookupName);

                    break;

                case CatchSkinComponents.Droplet:
                    if (GetTexture("fruit-drop") != null)
                        return new LegacyFruitPiece("fruit-drop") { Scale = new Vector2(0.8f) };

                    break;

                case CatchSkinComponents.CatcherIdle:
                    return this.GetAnimation("fruit-catcher-idle", true, true, true) ??
                           this.GetAnimation("fruit-ryuuta", true, true, true);

                case CatchSkinComponents.CatcherFail:
                    return this.GetAnimation("fruit-catcher-fail", true, true, true) ??
                           this.GetAnimation("fruit-ryuuta", true, true, true);

                case CatchSkinComponents.CatcherKiai:
                    return this.GetAnimation("fruit-catcher-kiai", true, true, true) ??
                           this.GetAnimation("fruit-ryuuta", true, true, true);

                case CatchSkinComponents.CatchComboCounter:

                    if (providesComboCounter)
                        return new LegacyCatchComboCounter(Source);

                    break;
            }

            return null;
        }

        public override IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup)
        {
            switch (lookup)
            {
                case CatchSkinColour colour:
                    var result = (Bindable<Color4>)Source.GetConfig<SkinCustomColourLookup, TValue>(new SkinCustomColourLookup(colour));
                    if (result == null)
                        return null;

                    result.Value = LegacyColourCompatibility.DisallowZeroAlpha(result.Value);
                    return (IBindable<TValue>)result;
            }

            return Source.GetConfig<TLookup, TValue>(lookup);
        }
    }
}

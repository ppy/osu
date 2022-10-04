// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Skinning.Argon
{
    public class ManiaArgonSkinTransformer : SkinTransformer
    {
        public ManiaArgonSkinTransformer(ISkin skin)
            : base(skin)
        {
        }

        public override Drawable? GetDrawableComponent(ISkinComponent component)
        {
            switch (component)
            {
                case ManiaSkinComponent maniaComponent:
                    // TODO: Once everything is finalised, consider throwing UnsupportedSkinComponentException on missing entries.
                    switch (maniaComponent.Component)
                    {
                        case ManiaSkinComponents.HitTarget:
                            return new ArgonHitTarget();

                        case ManiaSkinComponents.KeyArea:
                            return new ArgonKeyArea();
                    }

                    break;
            }

            return base.GetDrawableComponent(component);
        }

        public override IBindable<TValue>? GetConfig<TLookup, TValue>(TLookup lookup)
        {
            if (lookup is ManiaSkinConfigurationLookup maniaLookup)
            {
                switch (maniaLookup.Lookup)
                {
                    case LegacyManiaSkinConfigurationLookups.ColumnBackgroundColour:
                        if (maniaLookup.StageDefinition.IsSpecialColumn(maniaLookup.ColumnIndex ?? 0))
                            return SkinUtils.As<TValue>(new Bindable<Color4>(Color4.Yellow));

                        // TODO: Add actual colours.
                        return SkinUtils.As<TValue>(new Bindable<Color4>(new Color4(RNG.NextSingle() * 0.5f, RNG.NextSingle() * 0.5f, RNG.NextSingle() * 0.5f, 1)));
                }

                return base.GetConfig<LegacyManiaSkinConfigurationLookup, TValue>(new LegacyManiaSkinConfigurationLookup(maniaLookup.StageDefinition.Columns, maniaLookup.Lookup,
                    maniaLookup.ColumnIndex));
            }

            return base.GetConfig<TLookup, TValue>(lookup);
        }
    }
}

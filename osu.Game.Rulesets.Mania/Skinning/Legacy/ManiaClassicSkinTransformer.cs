// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Skinning.Legacy
{
    public class ManiaClassicSkinTransformer : ManiaLegacySkinTransformer
    {
        public ManiaClassicSkinTransformer(ISkin skin, IBeatmap beatmap)
            : base(skin, beatmap)
        {
        }

        public override IBindable<TValue>? GetConfig<TLookup, TValue>(TLookup lookup)
        {
            if (lookup is ManiaSkinConfigurationLookup maniaLookup)
            {
                var baseLookup = base.GetConfig<TLookup, TValue>(lookup);

                if (baseLookup != null)
                    return baseLookup;

                // default provisioning.
                switch (maniaLookup.Lookup)
                {
                    case LegacyManiaSkinConfigurationLookups.ColumnBackgroundColour:
                        return SkinUtils.As<TValue>(new Bindable<Color4>(Color4.Black));
                }
            }

            return base.GetConfig<TLookup, TValue>(lookup);
        }
    }
}

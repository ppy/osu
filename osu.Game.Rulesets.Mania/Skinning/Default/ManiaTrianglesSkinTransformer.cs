// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Skinning.Default
{
    public class ManiaTrianglesSkinTransformer : SkinTransformer
    {
        public ManiaTrianglesSkinTransformer(ISkin skin)
            : base(skin)
        {
        }

        private readonly Color4 colourEven = new Color4(6, 84, 0, 255);
        private readonly Color4 colourOdd = new Color4(94, 0, 57, 255);
        private readonly Color4 colourSpecial = new Color4(0, 48, 63, 255);

        public override IBindable<TValue>? GetConfig<TLookup, TValue>(TLookup lookup)
        {
            if (lookup is ManiaSkinConfigurationLookup maniaLookup)
            {
                switch (maniaLookup.Lookup)
                {
                    case LegacyManiaSkinConfigurationLookups.ColumnBackgroundColour:
                        int column = maniaLookup.ColumnIndex ?? 0;

                        if (maniaLookup.StageDefinition.IsSpecialColumn(column))
                            return SkinUtils.As<TValue>(new Bindable<Color4>(colourSpecial));

                        int distanceToEdge = Math.Min(column, (maniaLookup.StageDefinition.Columns - 1) - column);
                        return SkinUtils.As<TValue>(new Bindable<Color4>(distanceToEdge % 2 == 0 ? colourOdd : colourEven));
                }
            }

            return base.GetConfig<TLookup, TValue>(lookup);
        }
    }
}

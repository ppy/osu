// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Skinning.Argon
{
    public class ManiaArgonSkinTransformer : SkinTransformer
    {
        private readonly ManiaBeatmap beatmap;

        public ManiaArgonSkinTransformer(ISkin skin, IBeatmap beatmap)
            : base(skin)
        {
            this.beatmap = (ManiaBeatmap)beatmap;
        }

        public override Drawable? GetDrawableComponent(ISkinComponent component)
        {
            switch (component)
            {
                case ManiaSkinComponent maniaComponent:
                    // TODO: Once everything is finalised, consider throwing UnsupportedSkinComponentException on missing entries.
                    switch (maniaComponent.Component)
                    {
                        case ManiaSkinComponents.HoldNoteHead:
                        case ManiaSkinComponents.Note:
                            return new ArgonNotePiece();

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
                        int column = maniaLookup.ColumnIndex ?? 0;
                        var stage = beatmap.GetStageForColumnIndex(column);

                        if (stage.IsSpecialColumn(column))
                            return SkinUtils.As<TValue>(new Bindable<Color4>(Color4.Yellow));

                        // TODO: Add actual colours.
                        return SkinUtils.As<TValue>(new Bindable<Color4>(new Color4(RNG.NextSingle() * 0.5f, RNG.NextSingle() * 0.5f, RNG.NextSingle() * 0.5f, 1)));
                }
            }

            return base.GetConfig<TLookup, TValue>(lookup);
        }
    }
}

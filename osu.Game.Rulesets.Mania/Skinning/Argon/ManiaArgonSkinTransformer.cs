// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Scoring;
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
                case GameplaySkinComponent<HitResult> resultComponent:
                    return new ArgonJudgementPiece(resultComponent.Component);

                case ManiaSkinComponent maniaComponent:
                    // TODO: Once everything is finalised, consider throwing UnsupportedSkinComponentException on missing entries.
                    switch (maniaComponent.Component)
                    {
                        case ManiaSkinComponents.StageBackground:
                            return new ArgonStageBackground();

                        case ManiaSkinComponents.ColumnBackground:
                            return new ArgonColumnBackground();

                        case ManiaSkinComponents.HoldNoteBody:
                            return new ArgonHoldBodyPiece();

                        case ManiaSkinComponents.HoldNoteTail:
                            return new ArgonHoldNoteTailPiece();

                        case ManiaSkinComponents.HoldNoteHead:
                        case ManiaSkinComponents.Note:
                            return new ArgonNotePiece();

                        case ManiaSkinComponents.HitTarget:
                            return new ArgonHitTarget();

                        case ManiaSkinComponents.KeyArea:
                            return new ArgonKeyArea();

                        case ManiaSkinComponents.HitExplosion:
                            return new ArgonHitExplosion();
                    }

                    break;
            }

            return base.GetDrawableComponent(component);
        }

        public override IBindable<TValue>? GetConfig<TLookup, TValue>(TLookup lookup)
        {
            if (lookup is ManiaSkinConfigurationLookup maniaLookup)
            {
                int column = maniaLookup.ColumnIndex ?? 0;
                var stage = beatmap.GetStageForColumnIndex(column);

                switch (maniaLookup.Lookup)
                {
                    case LegacyManiaSkinConfigurationLookups.ColumnSpacing:
                        return SkinUtils.As<TValue>(new Bindable<float>(2));

                    case LegacyManiaSkinConfigurationLookups.StagePaddingBottom:
                    case LegacyManiaSkinConfigurationLookups.StagePaddingTop:
                        return SkinUtils.As<TValue>(new Bindable<float>(30));

                    case LegacyManiaSkinConfigurationLookups.ColumnWidth:
                        return SkinUtils.As<TValue>(new Bindable<float>(
                            stage.IsSpecialColumn(column) ? 120 : 60
                        ));

                    case LegacyManiaSkinConfigurationLookups.ColumnBackgroundColour:

                        Color4 colour;

                        if (stage.IsSpecialColumn(column))
                            colour = new Color4(159, 101, 255, 255);
                        else
                        {
                            switch (column % 8)
                            {
                                default:
                                    colour = new Color4(240, 216, 0, 255);
                                    break;

                                case 1:
                                    colour = new Color4(240, 101, 0, 255);
                                    break;

                                case 2:
                                    colour = new Color4(240, 0, 130, 255);
                                    break;

                                case 3:
                                    colour = new Color4(192, 0, 240, 255);
                                    break;

                                case 4:
                                    colour = new Color4(178, 0, 240, 255);
                                    break;

                                case 5:
                                    colour = new Color4(0, 96, 240, 255);
                                    break;

                                case 6:
                                    colour = new Color4(0, 226, 240, 255);
                                    break;

                                case 7:
                                    colour = new Color4(0, 240, 96, 255);
                                    break;
                            }
                        }

                        return SkinUtils.As<TValue>(new Bindable<Color4>(colour));
                }
            }

            return base.GetConfig<TLookup, TValue>(lookup);
        }
    }
}

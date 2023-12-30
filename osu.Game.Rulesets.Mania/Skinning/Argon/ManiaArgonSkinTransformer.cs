// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Skinning.Argon
{
    public class ManiaArgonSkinTransformer : ArgonSkinTransformer
    {
        private readonly ManiaBeatmap beatmap;

        public ManiaArgonSkinTransformer(ISkin skin, IBeatmap beatmap)
            : base(skin)
        {
            this.beatmap = (ManiaBeatmap)beatmap;
        }

        public override Drawable? GetDrawableComponent(ISkinComponentLookup lookup)
        {
            switch (lookup)
            {
                case SkinComponentsContainerLookup containerLookup:
                    switch (containerLookup.Target)
                    {
                        case SkinComponentsContainerLookup.TargetArea.MainHUDComponents when containerLookup.Ruleset != null:
                            Debug.Assert(containerLookup.Ruleset.ShortName == ManiaRuleset.SHORT_NAME);

                            var rulesetHUDComponents = Skin.GetDrawableComponent(lookup);

                            rulesetHUDComponents ??= new DefaultSkinComponentsContainer(container =>
                            {
                                var combo = container.ChildrenOfType<ArgonManiaComboCounter>().FirstOrDefault();

                                if (combo != null)
                                {
                                    combo.Anchor = Anchor.TopCentre;
                                    combo.Origin = Anchor.Centre;
                                    combo.Y = 200;
                                }
                            })
                            {
                                new ArgonManiaComboCounter(),
                            };

                            return rulesetHUDComponents;
                    }

                    break;

                case GameplaySkinComponentLookup<HitResult> resultComponent:
                    // This should eventually be moved to a skin setting, when supported.
                    if (Skin is ArgonProSkin && resultComponent.Component >= HitResult.Great)
                        return Drawable.Empty();

                    return new ArgonJudgementPiece(resultComponent.Component);

                case ManiaSkinComponentLookup maniaComponent:
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
                            return new ArgonHoldNoteHeadPiece();

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

            return base.GetDrawableComponent(lookup);
        }

        private static readonly Color4 colour_special_column = new Color4(169, 106, 255, 255);

        private const int total_colours = 6;

        private static readonly Color4 colour_yellow = new Color4(255, 197, 40, 255);
        private static readonly Color4 colour_orange = new Color4(252, 109, 1, 255);
        private static readonly Color4 colour_pink = new Color4(213, 35, 90, 255);
        private static readonly Color4 colour_purple = new Color4(203, 60, 236, 255);
        private static readonly Color4 colour_cyan = new Color4(72, 198, 255, 255);
        private static readonly Color4 colour_green = new Color4(100, 192, 92, 255);

        public override IBindable<TValue>? GetConfig<TLookup, TValue>(TLookup lookup)
        {
            if (lookup is ManiaSkinConfigurationLookup maniaLookup)
            {
                int columnIndex = maniaLookup.ColumnIndex ?? 0;
                var stage = beatmap.GetStageForColumnIndex(columnIndex);

                switch (maniaLookup.Lookup)
                {
                    case LegacyManiaSkinConfigurationLookups.ColumnSpacing:
                        return SkinUtils.As<TValue>(new Bindable<float>(2));

                    case LegacyManiaSkinConfigurationLookups.StagePaddingBottom:
                    case LegacyManiaSkinConfigurationLookups.StagePaddingTop:
                        return SkinUtils.As<TValue>(new Bindable<float>(30));

                    case LegacyManiaSkinConfigurationLookups.ColumnWidth:
                        bool isSpecialColumn = stage.IsSpecialColumn(columnIndex);

                        float width = 60 * (isSpecialColumn ? 2 : 1);

                        return SkinUtils.As<TValue>(new Bindable<float>(width));

                    case LegacyManiaSkinConfigurationLookups.ColumnBackgroundColour:

                        var colour = getColourForLayout(columnIndex, stage);

                        return SkinUtils.As<TValue>(new Bindable<Color4>(colour));
                }
            }

            return base.GetConfig<TLookup, TValue>(lookup);
        }

        private Color4 getColourForLayout(int columnIndex, StageDefinition stage)
        {
            // Account for cases like dual-stage (assume that all stages have the same column count for now).
            columnIndex %= stage.Columns;

            // For now, these are defined per column count as per https://user-images.githubusercontent.com/50823728/218038463-b450f46c-ef21-4551-b133-f866be59970c.png
            // See https://github.com/ppy/osu/discussions/21996 for discussion.
            switch (stage.Columns)
            {
                case 1:
                    return colour_yellow;

                case 2:
                    switch (columnIndex)
                    {
                        case 0: return colour_green;

                        case 1: return colour_cyan;

                        default: throw new ArgumentOutOfRangeException();
                    }

                case 3:
                    switch (columnIndex)
                    {
                        case 0: return colour_green;

                        case 1: return colour_special_column;

                        case 2: return colour_cyan;

                        default: throw new ArgumentOutOfRangeException();
                    }

                case 4:
                    switch (columnIndex)
                    {
                        case 0: return colour_yellow;

                        case 1: return colour_orange;

                        case 2: return colour_pink;

                        case 3: return colour_purple;

                        default: throw new ArgumentOutOfRangeException();
                    }

                case 5:
                    switch (columnIndex)
                    {
                        case 0: return colour_pink;

                        case 1: return colour_orange;

                        case 2: return colour_yellow;

                        case 3: return colour_green;

                        case 4: return colour_cyan;

                        default: throw new ArgumentOutOfRangeException();
                    }

                case 6:
                    switch (columnIndex)
                    {
                        case 0: return colour_pink;

                        case 1: return colour_orange;

                        case 2: return colour_green;

                        case 3: return colour_cyan;

                        case 4: return colour_orange;

                        case 5: return colour_pink;

                        default: throw new ArgumentOutOfRangeException();
                    }

                case 7:
                    switch (columnIndex)
                    {
                        case 0: return colour_pink;

                        case 1: return colour_orange;

                        case 2: return colour_pink;

                        case 3: return colour_special_column;

                        case 4: return colour_pink;

                        case 5: return colour_orange;

                        case 6: return colour_pink;

                        default: throw new ArgumentOutOfRangeException();
                    }

                case 8:
                    switch (columnIndex)
                    {
                        case 0: return colour_purple;

                        case 1: return colour_pink;

                        case 2: return colour_orange;

                        case 3: return colour_green;

                        case 4: return colour_cyan;

                        case 5: return colour_orange;

                        case 6: return colour_pink;

                        case 7: return colour_purple;

                        default: throw new ArgumentOutOfRangeException();
                    }

                case 9:
                    switch (columnIndex)
                    {
                        case 0: return colour_purple;

                        case 1: return colour_pink;

                        case 2: return colour_orange;

                        case 3: return colour_yellow;

                        case 4: return colour_special_column;

                        case 5: return colour_yellow;

                        case 6: return colour_orange;

                        case 7: return colour_pink;

                        case 8: return colour_purple;

                        default: throw new ArgumentOutOfRangeException();
                    }

                case 10:
                    switch (columnIndex)
                    {
                        case 0: return colour_purple;

                        case 1: return colour_pink;

                        case 2: return colour_orange;

                        case 3: return colour_yellow;

                        case 4: return colour_green;

                        case 5: return colour_cyan;

                        case 6: return colour_yellow;

                        case 7: return colour_orange;

                        case 8: return colour_pink;

                        case 9: return colour_purple;

                        default: throw new ArgumentOutOfRangeException();
                    }
            }

            // fallback for unhandled scenarios

            if (stage.IsSpecialColumn(columnIndex))
                return colour_special_column;

            switch (columnIndex % total_colours)
            {
                case 0: return colour_yellow;

                case 1: return colour_orange;

                case 2: return colour_pink;

                case 3: return colour_purple;

                case 4: return colour_cyan;

                case 5: return colour_green;

                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}
